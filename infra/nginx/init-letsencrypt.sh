#!/bin/sh
# Let's Encrypt certificate setup. Safe to re-run: an existing valid certificate
# is reused rather than re-issued, so repeat runs only refresh the nginx config.
#
# Usage:
#   chmod +x infra/nginx/init-letsencrypt.sh
#   ./infra/nginx/init-letsencrypt.sh
#
#   FORCE_RENEW=1 ./infra/nginx/init-letsencrypt.sh   # re-issue even if valid
#
# Prerequisites:
#   - DNS for $DOMAIN_REDIRECT and $DOMAIN_API must already point to this server.
#   - Docker + docker compose must be installed.
#   - .env must contain DOMAIN_REDIRECT, DOMAIN_API, CERTBOT_EMAIL.
#   - Cloudflare proxy (orange cloud) must be OFF during issuance.

set -eu

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
ENV_FILE="${PROJECT_ROOT}/.env"
CONF_D="${SCRIPT_DIR}/conf.d"
TEMPLATE="${SCRIPT_DIR}/templates/20-https.conf.template"
HTTPS_CONF="${CONF_D}/20-https.conf"
COMPOSE_FILE="${PROJECT_ROOT}/docker-compose.yml"

FORCE_RENEW="${FORCE_RENEW:-0}"

# Docker Compose project name defaults to the lowercased directory name.
PROJECT_NAME="$(basename "${PROJECT_ROOT}" | tr '[:upper:]' '[:lower:]')"

read_env() {
  if [ -f "${ENV_FILE}" ]; then
    grep -E "^${1}=" "${ENV_FILE}" | head -1 | cut -d= -f2- | tr -d '\r"'"'"
  fi
}

DOMAIN_REDIRECT="${DOMAIN_REDIRECT:-$(read_env DOMAIN_REDIRECT)}"
DOMAIN_API="${DOMAIN_API:-$(read_env DOMAIN_API)}"
CERTBOT_EMAIL="${CERTBOT_EMAIL:-$(read_env CERTBOT_EMAIL)}"

: "${DOMAIN_REDIRECT:?DOMAIN_REDIRECT not set. Add it to .env or export it before running.}"
: "${DOMAIN_API:?DOMAIN_API not set. Add it to .env or export it before running.}"
: "${CERTBOT_EMAIL:?CERTBOT_EMAIL not set. Add it to .env or export it before running.}"

CERT_NAME="${DOMAIN_REDIRECT}"
STAGING_CERT_NAME="${DOMAIN_REDIRECT}-staging"

echo "Project  : ${PROJECT_NAME}"
echo "Domains  : ${DOMAIN_REDIRECT}  /  www.${DOMAIN_REDIRECT}  /  ${DOMAIN_API}"
echo "Email    : ${CERTBOT_EMAIL}"
echo ""

# Run certbot via `docker run` directly — the compose service overrides the
# entrypoint with a renewal loop, which would swallow these subcommands.
#
# There is no TTY here on purpose, so every certbot call must be
# --non-interactive: without it certbot prints a prompt, reads EOF, and dies
# with `EOFError` instead of failing cleanly.
certbot_run() {
  docker run --rm \
    --network "${PROJECT_NAME}_default" \
    -v "${PROJECT_NAME}_letsencrypt_data:/etc/letsencrypt" \
    -v "${PROJECT_NAME}_certbot_www:/var/www/certbot" \
    certbot/certbot:latest \
    "$@"
}

# Shell inside the certbot image (its entrypoint is the certbot binary).
certbot_sh() {
  docker run --rm \
    -v "${PROJECT_NAME}_letsencrypt_data:/etc/letsencrypt" \
    --entrypoint sh \
    certbot/certbot:latest \
    -c "$1"
}

# ── Step 1: Remove stale HTTPS config so nginx can start without a cert ───────
echo ">>> Removing stale HTTPS config (if any)..."
rm -f "${HTTPS_CONF}"

# ── Step 2: Start nginx (HTTP-only — serves the ACME challenge) ───────────────
echo ">>> Starting nginx (HTTP-only)..."
docker compose -f "${COMPOSE_FILE}" up -d nginx
sleep 3

# ── Step 3: Reuse an existing certificate when there is one ───────────────────
HAVE_CERT=0
if certbot_sh "[ -s /etc/letsencrypt/live/${CERT_NAME}/fullchain.pem ]" 2>/dev/null; then
  HAVE_CERT=1
fi

if [ "${HAVE_CERT}" = "1" ] && [ "${FORCE_RENEW}" != "1" ]; then
  echo ">>> Existing certificate found for ${CERT_NAME} — skipping issuance."
  echo "    (re-run with FORCE_RENEW=1 to replace it)"
  echo ""
  certbot_sh "openssl x509 -in /etc/letsencrypt/live/${CERT_NAME}/fullchain.pem \
    -noout -subject -enddate -ext subjectAltName" 2>/dev/null || true
  echo ""
else
  if [ "${HAVE_CERT}" = "1" ]; then
    echo ">>> FORCE_RENEW=1 — replacing the existing certificate."
  fi

  # Staging first, under its own cert name so it can never collide with the
  # production certificate that lives in the same /etc/letsencrypt volume.
  echo ">>> Requesting staging certificate (connectivity check)..."
  certbot_run certonly \
    --non-interactive \
    --webroot \
    --webroot-path=/var/www/certbot \
    --email "${CERTBOT_EMAIL}" \
    --agree-tos \
    --no-eff-email \
    --staging \
    --cert-name "${STAGING_CERT_NAME}" \
    --force-renewal \
    -d "${DOMAIN_REDIRECT}" \
    -d "www.${DOMAIN_REDIRECT}" \
    -d "${DOMAIN_API}"

  echo ">>> Staging OK — discarding the throwaway staging certificate."
  certbot_run delete --non-interactive --cert-name "${STAGING_CERT_NAME}" >/dev/null 2>&1 || true

  # ── Step 4: Production certificate ──────────────────────────────────────────
  echo ">>> Requesting production certificate..."
  FORCE_FLAG=""
  if [ "${HAVE_CERT}" = "1" ]; then
    FORCE_FLAG="--force-renewal"
  fi

  # shellcheck disable=SC2086  # FORCE_FLAG is intentionally word-split (empty or one flag)
  certbot_run certonly \
    --non-interactive \
    --webroot \
    --webroot-path=/var/www/certbot \
    --email "${CERTBOT_EMAIL}" \
    --agree-tos \
    --no-eff-email \
    --cert-name "${CERT_NAME}" \
    ${FORCE_FLAG} \
    -d "${DOMAIN_REDIRECT}" \
    -d "www.${DOMAIN_REDIRECT}" \
    -d "${DOMAIN_API}"

  echo ">>> Production certificate obtained."
fi

# ── Step 5: Render HTTPS nginx config ────────────────────────────────────────
echo ">>> Writing HTTPS nginx config..."
sed \
  -e "s|\${DOMAIN_REDIRECT}|${DOMAIN_REDIRECT}|g" \
  -e "s|\${DOMAIN_API}|${DOMAIN_API}|g" \
  "${TEMPLATE}" > "${HTTPS_CONF}"

# ── Step 6: Load the new config ──────────────────────────────────────────────
echo ">>> Reloading nginx..."
docker compose -f "${COMPOSE_FILE}" up -d nginx
# `up -d` is a no-op when nginx is already running, so reload explicitly.
docker compose -f "${COMPOSE_FILE}" exec -T nginx nginx -s reload 2>/dev/null \
  || docker compose -f "${COMPOSE_FILE}" restart nginx

echo ""
echo "Done. Start the full stack:"
echo "  docker compose up -d"
echo ""
echo "Note: ${HTTPS_CONF} is generated — do not commit it."

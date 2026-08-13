#!/bin/sh
# One-time Let's Encrypt certificate initialisation.
# Run this ONCE before starting the full stack with docker-compose.yml.
#
# Usage:
#   chmod +x infra/nginx/init-letsencrypt.sh
#   ./infra/nginx/init-letsencrypt.sh
#
# Prerequisites:
#   - DNS for $DOMAIN_REDIRECT and $DOMAIN_API must already point to this server.
#   - Docker + docker compose must be installed.
#   - .env must contain DOMAIN_REDIRECT, DOMAIN_API, CERTBOT_EMAIL.
#   - Cloudflare proxy (orange cloud) must be OFF during this script.

set -eu

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
ENV_FILE="${PROJECT_ROOT}/.env"
CONF_D="${SCRIPT_DIR}/conf.d"
TEMPLATE="${SCRIPT_DIR}/templates/20-https.conf.template"
HTTPS_CONF="${CONF_D}/20-https.conf"
COMPOSE_FILE="${PROJECT_ROOT}/docker-compose.yml"

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

echo "Project  : ${PROJECT_NAME}"
echo "Domains  : ${DOMAIN_REDIRECT}  /  ${DOMAIN_API}"
echo "Email    : ${CERTBOT_EMAIL}"
echo ""

# Helper: run certbot directly via docker run (bypasses compose entrypoint override).
certbot_run() {
  docker run --rm \
    --network "${PROJECT_NAME}_default" \
    -v "${PROJECT_NAME}_letsencrypt_data:/etc/letsencrypt" \
    -v "${PROJECT_NAME}_certbot_www:/var/www/certbot" \
    certbot/certbot:latest \
    "$@"
}

# ── Step 1: Remove stale HTTPS config so nginx starts HTTP-only ───────────────
echo ">>> Removing stale HTTPS config (if any)..."
rm -f "${HTTPS_CONF}"

# ── Step 2: Start nginx (HTTP-only — no SSL cert required) ────────────────────
echo ">>> Starting nginx (HTTP-only)..."
docker compose -f "${COMPOSE_FILE}" up -d nginx
sleep 3

# ── Step 3: Staging cert — verify ACME before hitting rate limits ─────────────
echo ">>> Requesting Let's Encrypt staging certificate (connectivity check)..."
certbot_run certonly \
  --webroot \
  --webroot-path=/var/www/certbot \
  --email "${CERTBOT_EMAIL}" \
  --agree-tos \
  --no-eff-email \
  --staging \
  -d "${DOMAIN_REDIRECT}" \
  -d "www.${DOMAIN_REDIRECT}" \
  -d "${DOMAIN_API}"

echo ">>> Staging OK. Requesting production certificate..."
certbot_run delete --cert-name "${DOMAIN_REDIRECT}" --non-interactive 2>/dev/null || true

# ── Step 4: Production certificate ───────────────────────────────────────────
certbot_run certonly \
  --webroot \
  --webroot-path=/var/www/certbot \
  --email "${CERTBOT_EMAIL}" \
  --agree-tos \
  --no-eff-email \
  --force-renewal \
  -d "${DOMAIN_REDIRECT}" \
  -d "www.${DOMAIN_REDIRECT}" \
  -d "${DOMAIN_API}"

echo ">>> Production certificate obtained!"

# ── Step 5: Render HTTPS nginx config ────────────────────────────────────────
echo ">>> Writing HTTPS nginx config..."
sed \
  -e "s|\${DOMAIN_REDIRECT}|${DOMAIN_REDIRECT}|g" \
  -e "s|\${DOMAIN_API}|${DOMAIN_API}|g" \
  "${TEMPLATE}" > "${HTTPS_CONF}"

# ── Step 6: Reload nginx ──────────────────────────────────────────────────────
echo ">>> Reloading nginx with HTTPS config..."
docker compose -f "${COMPOSE_FILE}" exec nginx nginx -s reload

echo ""
echo "Done! Start the full stack:"
echo "  docker compose up -d"
echo ""
echo "Note: ${HTTPS_CONF} was generated — do not commit it."

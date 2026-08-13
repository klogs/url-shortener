#!/bin/sh
# One-time Let's Encrypt certificate initialisation.
# Run this ONCE before starting the full stack with docker-compose-with-nginx.yml.
#
# Usage:
#   chmod +x infra/nginx/init-letsencrypt.sh
#   ./infra/nginx/init-letsencrypt.sh
#
# Prerequisites:
#   - DNS for $DOMAIN_REDIRECT and $DOMAIN_API must already point to this server.
#   - Docker + docker compose must be installed.
#   - .env file in the project root must contain DOMAIN_REDIRECT, DOMAIN_API, CERTBOT_EMAIL.

set -eu

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
ENV_FILE="${PROJECT_ROOT}/.env"

# Read a single KEY=value from the .env file without sourcing it.
read_env() {
  if [ -f "${ENV_FILE}" ]; then
    grep -E "^${1}=" "${ENV_FILE}" | head -1 | cut -d= -f2- | tr -d '\r"'"'"
  fi
}

DOMAIN_REDIRECT="${DOMAIN_REDIRECT:-$(read_env DOMAIN_REDIRECT)}"
DOMAIN_API="${DOMAIN_API:-$(read_env DOMAIN_API)}"
CERTBOT_EMAIL="${CERTBOT_EMAIL:-$(read_env CERTBOT_EMAIL)}"

: "${DOMAIN_REDIRECT:?DOMAIN_REDIRECT not set. Add it to .env or export it before running this script.}"
: "${DOMAIN_API:?DOMAIN_API not set. Add it to .env or export it before running this script.}"
: "${CERTBOT_EMAIL:?CERTBOT_EMAIL not set. Add it to .env or export it before running this script.}"

COMPOSE_FILE="${PROJECT_ROOT}/docker-compose-with-nginx.yml"

echo "Domains  : ${DOMAIN_REDIRECT}  /  ${DOMAIN_API}"
echo "Email    : ${CERTBOT_EMAIL}"
echo ""

# ── Step 1: Create dummy self-signed cert so nginx can start ──────────────────
# nginx refuses to start if the ssl_certificate file doesn't exist.
echo ">>> Creating temporary self-signed certificate..."
docker compose -f "${COMPOSE_FILE}" run --rm --no-deps certbot \
  sh -c "
    mkdir -p /etc/letsencrypt/live/${DOMAIN_REDIRECT} &&
    openssl req -x509 -nodes -newkey rsa:2048 -days 1 \
      -keyout /etc/letsencrypt/live/${DOMAIN_REDIRECT}/privkey.pem \
      -out    /etc/letsencrypt/live/${DOMAIN_REDIRECT}/fullchain.pem \
      -subj   '/CN=localhost'
  "

# ── Step 2: Start nginx (it can now load the dummy cert) ─────────────────────
echo ">>> Starting nginx..."
docker compose -f "${COMPOSE_FILE}" up -d nginx
sleep 3

# ── Step 3: Request staging cert (verify ACME before hitting rate limits) ─────
echo ">>> Requesting Let's Encrypt staging certificate (connectivity check)..."
docker compose -f "${COMPOSE_FILE}" run --rm --no-deps certbot certonly \
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
docker compose -f "${COMPOSE_FILE}" run --rm --no-deps certbot delete \
  --cert-name "${DOMAIN_REDIRECT}" --non-interactive 2>/dev/null || true

# ── Step 4: Request production certificate ────────────────────────────────────
docker compose -f "${COMPOSE_FILE}" run --rm --no-deps certbot certonly \
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

# ── Step 5: Reload nginx with the real certificate ───────────────────────────
echo ">>> Reloading nginx with real certificate..."
docker compose -f "${COMPOSE_FILE}" exec nginx nginx -s reload

echo ""
echo "Done! Start the full stack:"
echo "  docker compose -f docker-compose-with-nginx.yml up -d"

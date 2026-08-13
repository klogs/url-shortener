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
# Handles comments, blank lines, and Windows CRLF endings.
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
echo "Compose  : ${COMPOSE_FILE}"
echo ""

# ── Create webroot directory ───────────────────────────────────────────────────
mkdir -p "${PROJECT_ROOT}/infra/nginx/certbot/www/.well-known/acme-challenge"

# ── Start nginx on port 80 only (no HTTPS yet — certs don't exist) ────────────
echo ">>> Starting nginx for ACME challenge..."
docker compose -f "${COMPOSE_FILE}" up -d nginx

sleep 3

# ── Staging cert: verify ACME connectivity before hitting rate limits ──────────
echo ">>> Requesting staging certificate (dry-run verification)..."
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

# ── Reload nginx to pick up TLS certificates ──────────────────────────────────
echo ">>> Reloading nginx..."
docker compose -f "${COMPOSE_FILE}" exec nginx nginx -s reload || \
  docker compose -f "${COMPOSE_FILE}" restart nginx

echo ""
echo "Done! Certs: /etc/letsencrypt/live/${DOMAIN_REDIRECT}/"
echo "Start the full stack:"
echo "  docker compose -f docker-compose-with-nginx.yml up -d"

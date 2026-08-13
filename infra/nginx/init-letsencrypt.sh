#!/usr/bin/env bash
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

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"

# ── Load .env ──────────────────────────────────────────────────────────────────
if [[ -f "${PROJECT_ROOT}/.env" ]]; then
  set -o allexport
  # shellcheck disable=SC1091
  source "${PROJECT_ROOT}/.env"
  set +o allexport
fi

DOMAIN_REDIRECT="${DOMAIN_REDIRECT:?DOMAIN_REDIRECT must be set in .env}"
DOMAIN_API="${DOMAIN_API:?DOMAIN_API must be set in .env}"
CERTBOT_EMAIL="${CERTBOT_EMAIL:?CERTBOT_EMAIL must be set in .env}"

COMPOSE_FILE="${PROJECT_ROOT}/docker-compose-with-nginx.yml"
DATA_PATH="/etc/letsencrypt"

# ── Create webroot directory ───────────────────────────────────────────────────
mkdir -p "${PROJECT_ROOT}/infra/nginx/certbot/www/.well-known/acme-challenge"

# ── Start nginx on port 80 only (no HTTPS yet — certs don't exist) ────────────
echo ">>> Starting nginx for ACME challenge…"
docker compose -f "${COMPOSE_FILE}" up -d nginx

# Give nginx a moment to come up
sleep 3

# ── Request staging cert first to verify connectivity ─────────────────────────
echo ">>> Requesting Let's Encrypt staging certificate to verify ACME…"
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

echo ">>> Staging cert OK. Deleting staging cert and requesting production cert…"
docker compose -f "${COMPOSE_FILE}" run --rm --no-deps certbot delete \
  --cert-name "${DOMAIN_REDIRECT}" --non-interactive || true

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

# ── Reload nginx to pick up the TLS certificates ──────────────────────────────
echo ">>> Reloading nginx…"
docker compose -f "${COMPOSE_FILE}" exec nginx nginx -s reload || \
  docker compose -f "${COMPOSE_FILE}" restart nginx

echo "
Done! Certificates are stored in /etc/letsencrypt/live/${DOMAIN_REDIRECT}/.
Next steps:
  docker compose -f docker-compose-with-nginx.yml up -d
Certbot will auto-renew every 12 hours via the certbot service.
"

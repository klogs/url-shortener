#!/bin/sh
# Lab / local-only setup: self-signed cert + nginx HTTPS config.
# DO NOT use on a real server — use init-letsencrypt.sh for production.
#
# Usage:
#   chmod +x infra/nginx/init-local.sh
#   ./infra/nginx/init-local.sh

set -eu

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
ENV_FILE="${PROJECT_ROOT}/.env"
CONF_D="${SCRIPT_DIR}/conf.d"
TEMPLATE="${SCRIPT_DIR}/templates/20-https.conf.template"
HTTPS_CONF="${CONF_D}/20-https.conf"
COMPOSE_FILE="${PROJECT_ROOT}/docker-compose.yml"

read_env() {
  if [ -f "${ENV_FILE}" ]; then
    grep -E "^${1}=" "${ENV_FILE}" | head -1 | cut -d= -f2- | tr -d '\r"'"'"
  fi
}

DOMAIN_REDIRECT="${DOMAIN_REDIRECT:-$(read_env DOMAIN_REDIRECT)}"
DOMAIN_API="${DOMAIN_API:-$(read_env DOMAIN_API)}"

: "${DOMAIN_REDIRECT:?DOMAIN_REDIRECT not set. Add it to .env}"
: "${DOMAIN_API:?DOMAIN_API not set. Add it to .env}"

echo "Domains  : ${DOMAIN_REDIRECT}  /  ${DOMAIN_API}"
echo ""

# ── Step 1: Remove stale HTTPS config so nginx starts HTTP-only ───────────────
rm -f "${HTTPS_CONF}"

# ── Step 2: Self-signed certificate (365 days) ────────────────────────────────
echo ">>> Creating self-signed certificate..."
docker compose -f "${COMPOSE_FILE}" run --rm --no-deps --entrypoint sh certbot \
  -c "mkdir -p /etc/letsencrypt/live/${DOMAIN_REDIRECT} &&
      openssl req -x509 -nodes -newkey rsa:2048 -days 365 \
        -keyout /etc/letsencrypt/live/${DOMAIN_REDIRECT}/privkey.pem \
        -out    /etc/letsencrypt/live/${DOMAIN_REDIRECT}/fullchain.pem \
        -subj   '/CN=${DOMAIN_REDIRECT}'"

# ── Step 3: Render HTTPS nginx config from template ───────────────────────────
echo ">>> Writing HTTPS nginx config..."
sed \
  -e "s|\${DOMAIN_REDIRECT}|${DOMAIN_REDIRECT}|g" \
  -e "s|\${DOMAIN_API}|${DOMAIN_API}|g" \
  "${TEMPLATE}" > "${HTTPS_CONF}"

# ── Step 4: Start or reload nginx ────────────────────────────────────────────
echo ">>> Starting/reloading nginx..."
docker compose -f "${COMPOSE_FILE}" up -d nginx

# Reload config if nginx was already running (up -d alone doesn't reload)
docker compose -f "${COMPOSE_FILE}" exec nginx nginx -s reload 2>/dev/null || true

echo ""
echo "Done!"
echo "  Browser will show an SSL warning — click 'Advanced > Proceed' to accept."
echo ""
echo "Start the full stack:"
echo "  docker compose -f docker-compose.yml up -d"

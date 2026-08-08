# Secrets Management

## Environment variable contract

All sensitive values are injected via environment variables. Never commit real secrets to source control.

| Variable | Service | Purpose |
|---|---|---|
| `Database__ConnectionString` | Api, Redirect, Worker, Migrator | PostgreSQL connection string |
| `Redis__ConnectionString` | Api, Redirect, Worker | Redis connection string |
| `RabbitMq__Host` / `__Port` / `__Username` / `__Password` | Api, Worker | RabbitMQ credentials |
| `Auth__Authority` | Api | OIDC issuer URL |
| `Auth__ClientId` | Api | OAuth client ID |
| `Auth__ClientSecret` | Api | OAuth client secret (confidential client only) |
| `AUTH_SECRET` | Web (Next.js) | NextAuth.js session-encryption secret (32+ random bytes) |
| `AUTH_ISSUER` | Web (Next.js) | OIDC issuer URL |
| `AUTH_CLIENT_ID` | Web (Next.js) | OAuth client ID |
| `AUTH_CLIENT_SECRET` | Web (Next.js) | OAuth client secret |
| `Captcha__SecretKey` | Api | Cloudflare Turnstile secret key |
| `NEXT_PUBLIC_CAPTCHA_SITE_KEY` | Web (build-time) | Turnstile site key (public) |
| `Geo__DatabasePath` | Redirect | Path to GeoLite2-Country.mmdb |

## Local development

Copy `.env.example` (once created) to `.env` and fill in values. The `.env` file is in `.gitignore` and must never be committed.

For the Next.js app, use `.env.local` (also gitignored by Next.js by default).

## Docker / docker compose

Use Docker secrets or a `.env` file mounted at runtime. Example with an env file:

```bash
docker compose --env-file .env.prod up -d
```

Never bake secrets into `Dockerfile` instructions — use `ENV` in `docker-compose.yml` from the env file, not in the image itself.

## Production

Preferred: inject secrets via a secrets manager (e.g. HashiCorp Vault, AWS Secrets Manager, Azure Key Vault, or Kubernetes Secrets with encryption at rest). The application reads all configuration from environment variables, so any mechanism that populates those variables works.

`AUTH_SECRET` for Next.js must be rotated independently of the OIDC client secret. Generate with:

```bash
openssl rand -base64 32
```

## What is NOT a secret

- `NEXT_PUBLIC_*` variables (they are bundled into the browser bundle)
- `Auth__Authority` and `AUTH_ISSUER` (public OIDC discovery endpoints)
- `NEXT_PUBLIC_API_URL`

## Rotation

- Rotate `AUTH_SECRET` (Next.js): invalidates all existing sessions — coordinate with users.
- Rotate `Auth__ClientSecret`: update at IdP and in deployment simultaneously.
- Rotate `Database__ConnectionString` (password only): update PostgreSQL, then rolling-restart services.
- Redis password rotation: update Redis, then rolling-restart services.

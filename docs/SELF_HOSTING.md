# Self-Hosting Guide

This document covers running the URL shortener on your own infrastructure.

## Prerequisites

- Docker + Docker Compose (or individual service installs)
- .NET 10 SDK (for building from source)
- An OIDC provider (optional in `SingleTenant` mode with anonymous-only links)

## Quick start (local development)

```bash
# 1. Start infrastructure
docker compose up -d postgres redis

# 2. Apply database migrations
dotnet run --project src/Shortener.Migrator

# 3. Start services (separate terminals)
dotnet run --project src/Shortener.Api
dotnet run --project src/Shortener.Redirect
dotnet run --project src/Shortener.Worker   # required only if analytics is enabled

# 4. Start frontend
cd web/shortener-web && npm install && npm run dev
```

API is at `http://localhost:5000`, redirect service at `http://localhost:5001`.

## Multitenancy modes

Set `MULTITENANCY__MODE` to choose the operating mode.

| Mode | Behaviour |
|---|---|
| `SingleTenant` (default) | One tenant seeded automatically on startup. Verification is skipped for the default domain. |
| `MultiTenant` | Tenants must be provisioned manually; each domain goes through DNS/HTTP verification. |

## Environment variables

All services read from `appsettings.json` and environment variable overrides.  
Environment variable names use the `__` separator (e.g. `Database__ConnectionString`).

### Common (all services)

| Variable | Example | Description |
|---|---|---|
| `Database__ConnectionString` | `Host=localhost;Database=shortener;Username=shortener;Password=…` | PostgreSQL connection string |
| `Redis__ConnectionString` | `localhost:6379` | Redis endpoint |
| `Multitenancy__Mode` | `SingleTenant` | `SingleTenant` or `MultiTenant` |

### Shortener.Api only

| Variable | Example | Description |
|---|---|---|
| `Auth__Authority` | `https://idp.klogs.io` | OIDC authority URL |
| `Auth__ClientId` | `klogs.web` | Expected audience/client ID |
| `Shortener__DefaultHost` | `s.example.com` | Used when host cannot be resolved from request |
| `Shortener__AnonymousExpirationDays` | `30` | Lifetime of anonymous (CAPTCHA-gated) links |
| `Captcha__Provider` | `Disabled` / `CloudflareTurnstile` | CAPTCHA backend |
| `Captcha__SecretKey` | `…` | Required when provider is `CloudflareTurnstile` |
| `RateLimit__PublicCreatePerMinute` | `10` | Anonymous create rate limit per IP per minute |

### Shortener.Worker only (analytics)

| Variable | Example | Description |
|---|---|---|
| `RabbitMQ__Host` | `localhost` | RabbitMQ hostname |
| `RabbitMQ__Port` | `5672` | RabbitMQ port |
| `RabbitMQ__VirtualHost` | `shortener` | RabbitMQ virtual host |
| `RabbitMQ__Username` | `shortener` | RabbitMQ username |
| `RabbitMQ__Password` | `…` | RabbitMQ password |

## Custom domains (Phase 3)

Custom domains let multiple hostnames serve short links. Each domain belongs to a tenant and can redirect differently.

### Adding a custom domain

1. **Register the domain** via the API:

```http
POST /api/v1/domains
Authorization: Bearer <token>
Content-Type: application/json

{ "host": "go.example.com" }
```

Response includes a `verificationToken` (e.g. `a1b2c3d4…`).

2. **Serve the verification file** at `https://go.example.com/.well-known/shortener-verify`.  
   The response body must contain the token string.  
   For nginx:

```nginx
location /.well-known/shortener-verify {
    return 200 "a1b2c3d4...";
    add_header Content-Type text/plain;
}
```

3. **Trigger verification**:

```http
POST /api/v1/domains/{id}/verify
Authorization: Bearer <token>
```

Returns `204 No Content` on success. The domain status changes to `Active` and redirect requests on that host start working.

### Default domain

The first domain you add for a tenant automatically becomes the default domain (used for anonymous link creation). It cannot be removed. To change the default, add the new domain and remove the old one via `DELETE /api/v1/domains/{id}`.

### SingleTenant mode

In `SingleTenant` mode the seeder creates one tenant and one `Active` domain from the `Shortener__DefaultHost` setting, skipping the HTTP verification step. This is suitable for private/self-hosted deployments where you control the hostname.

## Running with full Docker Compose (when Dockerfiles are ready)

Uncomment the application service blocks in `docker-compose.yml` and set required environment variables in a `.env` file:

```env
POSTGRES_PASSWORD=changeme
RABBITMQ_PASSWORD=changeme
AUTH__ISSUER=https://your-idp.example.com
AUTH__CLIENT_ID=your-client-id
```

Then:

```bash
docker compose --profile messaging up -d
```

The `messaging` profile starts RabbitMQ alongside the infrastructure; omit it if you are not using analytics.

## Database migrations

Migrations are applied by the standalone migrator, not at application startup:

```bash
dotnet run --project src/Shortener.Migrator
```

In production, run the migrator as a one-off job before deploying updated application images.  
The migrator creates and updates all tables including the partitioned `click_events` table.

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Status

Currently in **Phase 0 — Foundation**. The repository contains only the design document (README.md) and LICENSE. No source files exist yet. Phase 0 goal: single `docker compose up` starts everything locally.

## Planned Solution Structure

```
src/
  Shortener.Api          # Control-plane API (link/tenant/domain/API-key management, auth)
  Shortener.Redirect     # Data-plane: ultra-fast /{shortCode} redirect only
  Shortener.Worker       # Async: analytics aggregation, expiration sweep, outbox, cache reconciliation
  Shortener.Domain       # Entities, value objects, domain events, enums — no dependencies
  Shortener.Application  # Use cases, commands, queries, interfaces, validators — no Infrastructure dependency
  Shortener.Infrastructure  # EF Core, Dapper, Redis, RabbitMQ.Client implementations
  Shortener.Contracts    # Shared request/response contracts
  Shortener.Migrator     # Standalone migration console app (never auto-migrate at startup)

web/
  shortener-web          # Next.js + TypeScript frontend

tests/
  Shortener.UnitTests
  Shortener.IntegrationTests
  Shortener.ArchitectureTests
  Shortener.LoadTests    # k6 scenarios; BCL HttpClient fallback if k6 unavailable
```

## Development Commands (once scaffolded)

```bash
# Start infrastructure
docker compose up -d postgres redis

# Backend services
dotnet run --project src/Shortener.Api
dotnet run --project src/Shortener.Redirect
dotnet run --project src/Shortener.Worker

# Frontend
cd web/shortener-web && npm install && npm run dev

# Run all tests
dotnet test

# Run a single test project
dotnet test tests/Shortener.UnitTests

# Full stack
docker compose up
```

## Architecture

**Modular Monolith + Clean Architecture + CQRS-lite + Event-Driven Background Processing.**

Layer dependency rules (enforced by architecture tests):
- `Domain` → no dependencies on any other project
- `Application` → Domain only; never depends on Infrastructure
- `Infrastructure` → implements Application interfaces
- `Shortener.Redirect` → must not take a dependency on management/API concerns

**CQRS-lite**: Commands and Queries are separate handler classes under `Application/Links/Commands/` and `Application/Links/Queries/`. No MediatR — handlers are injected and called directly via DI.

**Redirect hot-path** (the most critical path in the system):
```
Request → Normalize Host → Redis Lookup
  HIT → validate Status/ExpiresAt → non-blocking analytics buffer → Redirect
  MISS → PostgreSQL → Redis SET → non-blocking analytics buffer → Redirect
```
The redirect path must never synchronously write analytics, call RabbitMQ, call an external HTTP endpoint, or run identity provider calls.

**Analytics pipeline**: `Bounded Channel<ClickEvent>` (in-memory, non-blocking) → Background Publisher → RabbitMQ → Analytics Consumer → PostgreSQL ClickEvent partitions (RANGE by `OccurredAtUtc`).

**Control-plane events** (LinkCreated, LinkUpdated, etc.): PostgreSQL Transactional Outbox → Worker → RabbitMQ. Outbox is written in the same DB transaction as the domain change — no event loss.

**Cache**: Redis is cache, not source-of-truth. Key format: `sl:{normalized-host}:{short-code}`. Redis failure must fall back to PostgreSQL, not bring the service down.

**Expiration**: Correctness is always at runtime (`if ExpiresAt <= UtcNow → 410 Gone`), not dependent on the sweep worker. Worker updates `Status = 'Expired'` for operational hygiene only.

## Technology Stack

| Layer | Choice |
|---|---|
| Backend | .NET 10, ASP.NET Core Minimal APIs, C# |
| Frontend | Next.js, TypeScript, React |
| Primary DB | PostgreSQL (source-of-truth) |
| Cache | Redis (redirect cache + rate-limit state) |
| Queue | RabbitMQ via `RabbitMQ.Client` directly |
| ORM | EF Core (control-plane writes) + Dapper (read-optimized queries) |
| CAPTCHA | Cloudflare Turnstile (abstracted behind `ICaptchaVerifier`) |
| Auth | OIDC Authorization Code + PKCE; configurable provider |
| Unit tests | xUnit |
| Integration tests | xUnit + Testcontainers for .NET |
| Load tests | k6 |

**Klogs-specific OIDC**: Authority `https://idp.klogs.io`, ClientId `klogs.web`.

## Package Licensing Policy

Only OSI-compatible permissive licenses: MIT, Apache-2.0, BSD-2-Clause, BSD-3-Clause, ISC, PostgreSQL License.

**Banned packages** (no version, ever):
- `MediatR` — commercial license from v13+
- `AutoMapper` — commercial license from v15+

**Replacement patterns**:
- MediatR → explicit `CreateShortLinkHandler`, `UpdateShortLinkHandler`, etc., injected directly
- AutoMapper → explicit mapping methods / extension methods

Every new NuGet dependency must be logged in `docs/DEPENDENCIES.md` with license, purpose, and approval date. CI must fail on license violations.

## Critical Design Rules

1. Redirect path must be minimal — no DB write, no synchronous analytics, no auth calls on hot path.
2. Redis is cache-aside only; PostgreSQL is source-of-truth.
3. Redis failure degrades to PostgreSQL fallback — service stays up.
4. Expiration check at runtime; worker is for reconciliation only.
5. Every tenant-owned entity carries `TenantId`; never trust client-supplied `TenantId` — resolve from token/session.
6. Uniqueness key is `(DomainId, ShortCode)` — same code can exist on different domains.
7. Never `UPDATE ShortLinks SET ClickCount = ClickCount + 1` on redirect — use async aggregation.
8. Authentication provider must be configurable via env vars (`AUTH__AUTHORITY`, `AUTH__CLIENT_ID`, etc.).
9. All timestamps UTC; use `TimeProvider` abstraction for testability.
10. Nullable reference types enabled, warnings-as-errors.

## Multi-Tenancy

Shared Database / Shared Schema / `TenantId` column model. All tenant-owned repository queries must filter by `TenantId` resolved server-side. Supports `MULTITENANCY__MODE=SingleTenant` (auto-creates default tenant) and `MultiTenant`.

## Short Code Generation

Random cryptographic Base62 (alphabet `0-9A-Za-z`), 7 characters → ~3.5 trillion combinations. Use `RandomNumberGenerator`. Retry on collision. DB constraint: `UNIQUE (DomainId, ShortCode)`. Never derive short code from sequential ID.

## API Conventions

- Versioning: `/api/v1/`, `/api/v2/` for breaking changes
- Error format: RFC 9457 Problem Details with machine-readable `code` field
- Pagination: keyset/cursor (`?after=...`) for link lists; offset acceptable only for small dashboards
- Idempotency: `Idempotency-Key` header for programmatic link creation
- Redirect responses: 302 default; 410 Gone for expired; configurable per-link (301/302/307/308)

## Phasing

- **Phase 0**: Solution scaffold, docker-compose, CI, architecture tests, coding standards
- **Phase 1**: Core MVP — public create, CAPTCHA, redirect, Redis cache, expiration, authenticated CRUD, basic dashboard
- **Phase 2**: Analytics pipeline end-to-end
- **Phase 3**: Custom domains + self-hosting docs
- **Phase 4**: API platform (API keys, bulk create, webhooks foundation)
- **Phase 5**: Abuse prevention operations
- **Phase 6**: Advanced features (QR, A/B, geo routing, etc.)

**Do not add Phase 2+ features while Phase 0/1 is incomplete.**

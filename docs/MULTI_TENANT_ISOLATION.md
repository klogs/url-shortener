# Multi-Tenant Isolation

## Model

Shared Database / Shared Schema with `tenant_id` column on every tenant-owned table.

## How isolation is enforced

### Authentication layer
Every authenticated API request must carry a JWT issued by the configured OIDC provider
(`Auth__Authority`). The JWT is verified against the provider's public keys (standard OIDC).

### Tenant identity
`TenantId` is resolved **server-side only** from the `tid` JWT claim (Klogs IdP convention).
No endpoint trusts a `tenantId` supplied by the caller in the request body or URL.

`ResolveTenantId(ClaimsPrincipal)` in `Shortener.Api/Program.cs` is the single authoritative
resolution point. All endpoints call it and return `403 Forbidden` when it returns `null`.

### Repository layer
Every repository method that reads or mutates tenant-owned data accepts a `tenantId` parameter
and filters by it:

```csharp
// Example — GetByIdAsync always scopes to tenantId:
db.ShortLinks.FirstOrDefaultAsync(l => l.Id == id && l.TenantId == tenantId, ct);
```

This means an authenticated user with a valid JWT for Tenant A cannot read or mutate
Tenant B's data even if they know Tenant B's object IDs.

### Uniqueness keys
The uniqueness constraint on short codes is `(domain_id, short_code)`. Domain records carry
`tenant_id`, so the full isolation key is `(tenant_id, domain_id, short_code)`.

### Redirect (public, unauthenticated)
The redirect service resolves links by `(normalized_host, short_code)`. Hosts belong to
domains, which belong to tenants — but no tenant-scoped auth is applied to the redirect
hot-path (by design: it is a public service). TenantId is recorded in click events for
analytics aggregation only.

## Invariants to maintain

1. Every new repository method that reads or deletes tenant-owned rows **must** accept and filter by `tenantId`.
2. Never expose a bulk-list endpoint that does not scope to the resolved `TenantId`.
3. `MULTITENANCY__MODE=SingleTenant` seeds a default Tenant on first startup — the seeder
   does not bypass the repository contract; the seeded `TenantId` is still injected into the JWT
   via the IdP (single-tenant deployments configure the IdP to always issue the same `tid`).

## Known limitations / Phase 9 work
- Architecture tests (Phase 9) will enforce that no Application layer code accesses
  Infrastructure directly, and that every repository interface method signature includes
  `tenantId` where appropriate.
- Integration tests (Phase 9) will assert cross-tenant object access returns 404/403.

const BASE = process.env.NEXT_PUBLIC_API_URL ?? "";

// ── Types ────────────────────────────────────────────────────────────────────

export interface CreateLinkResult {
  id: string;
  shortCode: string;
  shortUrl: string;
  destinationUrl: string;
  status: string;
  expiresAt: string | null;
}

export interface LinkSummary {
  id: string;
  shortCode: string;
  destinationUrl: string;
  title: string | null;
  status: string;
  createdAtUtc: string;
  expiresAt: string | null;
  clickCountSnapshot: number;
  isAbTest: boolean;
  hasGeoRoutes: boolean;
}

export interface LinkDetail extends LinkSummary {
  description: string | null;
  redirectType: number;
  updatedAtUtc: string | null;
  domainId: string;
}

export interface LinksResult {
  items: LinkSummary[];
  nextCursor: string | null;
}

export interface DomainSummary {
  id: string;
  host: string;
  isVerified: boolean;
  createdAtUtc: string;
}

export interface VariantDto {
  id: string;
  linkId: string;
  label: string;
  destinationUrl: string;
  weight: number;
  createdAtUtc: string;
}

export interface GeoRouteDto {
  id: string;
  linkId: string;
  countryCode: string;
  destinationUrl: string;
  createdAtUtc: string;
}

// ── Helpers ──────────────────────────────────────────────────────────────────

async function apiFetch(
  path: string,
  token: string | undefined,
  init?: RequestInit
): Promise<Response> {
  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    Accept: "application/json",
    ...(init?.headers as Record<string, string>),
  };
  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }
  return fetch(`${BASE}${path}`, { ...init, headers });
}

// ── Public ───────────────────────────────────────────────────────────────────

export async function createPublicLink(
  destinationUrl: string,
  captchaToken?: string
): Promise<CreateLinkResult> {
  const res = await fetch(`${BASE}/api/v1/public/links`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ destinationUrl, captchaToken }),
  });

  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    throw new Error(
      (body as { detail?: string }).detail ?? `Request failed (${res.status})`
    );
  }

  return res.json() as Promise<CreateLinkResult>;
}

export interface TenantStats {
  totalLinks: number;
  activeLinks: number;
  clicksToday: number;
}

// ── Authenticated: stats ─────────────────────────────────────────────────────

export async function getTenantStats(token: string): Promise<TenantStats> {
  const res = await apiFetch("/api/v1/links/stats", token);
  if (!res.ok) throw new Error(`Failed to load stats (${res.status})`);
  return res.json() as Promise<TenantStats>;
}

// ── Authenticated: links ─────────────────────────────────────────────────────

export async function listLinks(
  token: string,
  pageSize = 20,
  after?: string
): Promise<LinksResult> {
  const params = new URLSearchParams({ pageSize: String(pageSize) });
  if (after) params.set("after", after);
  const res = await apiFetch(`/api/v1/links?${params}`, token);
  if (!res.ok) throw new Error(`Failed to load links (${res.status})`);
  return res.json() as Promise<LinksResult>;
}

export async function getLink(
  token: string,
  id: string
): Promise<LinkDetail | null> {
  const res = await apiFetch(`/api/v1/links/${id}`, token);
  if (res.status === 404) return null;
  if (!res.ok) throw new Error(`Failed to load link (${res.status})`);
  return res.json() as Promise<LinkDetail>;
}

export async function updateLink(
  token: string,
  id: string,
  body: { destinationUrl?: string; title?: string; expiresAt?: string | null; redirectType?: number }
): Promise<void> {
  const res = await apiFetch(`/api/v1/links/${id}`, token, {
    method: "PUT",
    body: JSON.stringify(body),
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error((err as { detail?: string }).detail ?? `Update failed (${res.status})`);
  }
}

export async function deleteLink(token: string, id: string): Promise<void> {
  const res = await apiFetch(`/api/v1/links/${id}`, token, { method: "DELETE" });
  if (!res.ok) throw new Error(`Delete failed (${res.status})`);
}

// ── Authenticated: domains ────────────────────────────────────────────────────

export async function listDomains(token: string): Promise<DomainSummary[]> {
  const res = await apiFetch("/api/v1/domains", token);
  if (!res.ok) throw new Error(`Failed to load domains (${res.status})`);
  return res.json() as Promise<DomainSummary[]>;
}

// ── Authenticated: variants ───────────────────────────────────────────────────

export async function listVariants(
  token: string,
  linkId: string
): Promise<VariantDto[]> {
  const res = await apiFetch(`/api/v1/links/${linkId}/variants`, token);
  if (!res.ok) throw new Error(`Failed to load variants (${res.status})`);
  return res.json() as Promise<VariantDto[]>;
}

export async function createVariant(
  token: string,
  linkId: string,
  body: { label: string; destinationUrl: string; weight: number }
): Promise<VariantDto> {
  const res = await apiFetch(`/api/v1/links/${linkId}/variants`, token, {
    method: "POST",
    body: JSON.stringify(body),
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error((err as { detail?: string }).detail ?? `Failed to create variant (${res.status})`);
  }
  return res.json() as Promise<VariantDto>;
}

export async function deleteVariant(
  token: string,
  linkId: string,
  variantId: string
): Promise<void> {
  const res = await apiFetch(`/api/v1/links/${linkId}/variants/${variantId}`, token, {
    method: "DELETE",
  });
  if (!res.ok) throw new Error(`Failed to delete variant (${res.status})`);
}

// ── Authenticated: geo routes ─────────────────────────────────────────────────

export async function listGeoRoutes(
  token: string,
  linkId: string
): Promise<GeoRouteDto[]> {
  const res = await apiFetch(`/api/v1/links/${linkId}/geo-routes`, token);
  if (!res.ok) throw new Error(`Failed to load geo routes (${res.status})`);
  return res.json() as Promise<GeoRouteDto[]>;
}

export async function createGeoRoute(
  token: string,
  linkId: string,
  body: { countryCode: string; destinationUrl: string }
): Promise<GeoRouteDto> {
  const res = await apiFetch(`/api/v1/links/${linkId}/geo-routes`, token, {
    method: "POST",
    body: JSON.stringify(body),
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error((err as { detail?: string }).detail ?? `Failed to create geo route (${res.status})`);
  }
  return res.json() as Promise<GeoRouteDto>;
}

export async function deleteGeoRoute(
  token: string,
  linkId: string,
  routeId: string
): Promise<void> {
  const res = await apiFetch(`/api/v1/links/${linkId}/geo-routes/${routeId}`, token, {
    method: "DELETE",
  });
  if (!res.ok) throw new Error(`Failed to delete geo route (${res.status})`);
}

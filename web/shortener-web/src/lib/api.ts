const BASE = process.env.NEXT_PUBLIC_API_URL ?? "";

export interface CreateLinkResult {
  id: string;
  shortCode: string;
  shortUrl: string;
  destinationUrl: string;
  status: string;
  expiresAt: string | null;
}

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

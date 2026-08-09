import type { NextConfig } from "next";

const apiUrl = process.env.NEXT_PUBLIC_API_URL ?? "";
const idpOrigin = process.env.AUTH_ISSUER ?? "https://idp.klogs.io";

// CSP is intentionally permissive for Cloudflare Turnstile (challenges.cloudflare.com).
// script-src includes 'unsafe-inline' for Next.js inline scripts; tighten with nonces
// once a CSP nonce middleware is in place (Phase 8+).
const csp = [
  "default-src 'self'",
  `script-src 'self' 'unsafe-inline' https://challenges.cloudflare.com`,
  `style-src 'self' 'unsafe-inline'`,
  `img-src 'self' data: blob:`,
  `font-src 'self'`,
  `connect-src 'self' ${apiUrl} ${idpOrigin}`,
  `frame-src https://challenges.cloudflare.com`,
  `frame-ancestors 'none'`,
  `object-src 'none'`,
  `base-uri 'self'`,
  `form-action 'self' ${idpOrigin}`,
  `upgrade-insecure-requests`,
]
  .join("; ")
  .trim();

const securityHeaders = [
  { key: "X-Content-Type-Options", value: "nosniff" },
  { key: "X-Frame-Options", value: "DENY" },
  { key: "Referrer-Policy", value: "strict-origin-when-cross-origin" },
  { key: "Permissions-Policy", value: "camera=(), microphone=(), geolocation=()" },
  { key: "Strict-Transport-Security", value: "max-age=31536000; includeSubDomains; preload" },
  { key: "Content-Security-Policy", value: csp },
];

const nextConfig: NextConfig = {
  output: "standalone",

  // Expose only explicitly allowed env vars to the browser
  env: {
    NEXT_PUBLIC_API_URL: apiUrl,
    NEXT_PUBLIC_CAPTCHA_SITE_KEY: process.env.NEXT_PUBLIC_CAPTCHA_SITE_KEY ?? "",
  },

  // Proxy /api/* to the backend so the browser always calls the same origin.
  // BACKEND_API_URL is a server-side runtime env var — works in Docker, Codespaces, etc.
  async rewrites() {
    const backendUrl =
      process.env.BACKEND_API_URL ??
      (apiUrl || "http://localhost:8080");
    return [
      {
        source: "/api/:path*",
        destination: `${backendUrl}/api/:path*`,
      },
    ];
  },

  async headers() {
    return [
      {
        source: "/(.*)",
        headers: securityHeaders,
      },
    ];
  },
};

export default nextConfig;

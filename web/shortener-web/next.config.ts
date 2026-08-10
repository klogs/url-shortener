import type { NextConfig } from "next";

const isDev = process.env.NODE_ENV === "development";
const idpOrigin = process.env.AUTH_ISSUER ?? "https://idp.klogs.io";

// CSP is intentionally permissive for Cloudflare Turnstile (challenges.cloudflare.com).
// script-src includes 'unsafe-inline' for Next.js inline scripts; tighten with nonces
// once a CSP nonce middleware is in place (Phase 8+).
// unsafe-eval is added in dev mode only — React dev tools require it.
const csp = [
  "default-src 'self'",
  `script-src 'self' 'unsafe-inline'${isDev ? " 'unsafe-eval'" : ""} https://challenges.cloudflare.com`,
  `style-src 'self' 'unsafe-inline'`,
  `img-src 'self' data: blob:`,
  `font-src 'self'`,
  `connect-src 'self' ${idpOrigin}`,
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
    NEXT_PUBLIC_CAPTCHA_SITE_KEY: process.env.NEXT_PUBLIC_CAPTCHA_SITE_KEY ?? "",
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

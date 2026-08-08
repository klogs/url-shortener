import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  output: "standalone",

  // Expose only explicitly allowed env vars to the browser
  env: {
    NEXT_PUBLIC_API_URL: process.env.NEXT_PUBLIC_API_URL ?? "",
    NEXT_PUBLIC_CAPTCHA_SITE_KEY: process.env.NEXT_PUBLIC_CAPTCHA_SITE_KEY ?? "",
  },
};

export default nextConfig;

import { NextRequest, NextResponse } from "next/server";

const BACKEND = process.env.BACKEND_API_URL ?? "http://localhost:8080";

type Context = { params: Promise<{ path: string[] }> };

async function proxy(req: NextRequest, context: Context) {
  const { path } = await context.params;
  const target = `${BACKEND}/api/v1/${path.join("/")}${req.nextUrl.search}`;

  const headers = new Headers(req.headers);
  headers.delete("host");

  const hasBody = req.method !== "GET" && req.method !== "HEAD";

  const fetchOptions: RequestInit & { duplex?: string } = {
    method: req.method,
    headers,
  };
  if (hasBody) {
    fetchOptions.body = req.body;
    fetchOptions.duplex = "half";
  }

  const upstream = await fetch(target, fetchOptions);

  return new NextResponse(upstream.body, {
    status: upstream.status,
    statusText: upstream.statusText,
    headers: upstream.headers,
  });
}

export const GET = proxy;
export const POST = proxy;
export const PUT = proxy;
export const PATCH = proxy;
export const DELETE = proxy;

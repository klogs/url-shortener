import { signIn } from "@/auth";

export async function GET(req: Request) {
  const url = new URL(req.url);
  const callbackUrl = url.searchParams.get("callbackUrl") ?? "/app";
  return signIn("klogs", { redirectTo: callbackUrl });
}

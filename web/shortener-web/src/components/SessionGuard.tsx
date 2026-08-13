"use client";

import { useEffect } from "react";
import { usePathname } from "next/navigation";
import { useSession, signIn } from "next-auth/react";
import { AUTH_UNAUTHORIZED_EVENT } from "@/lib/api";

/**
 * Keeps the browser's idea of "signed in" honest.
 *
 * The middleware only sees the session cookie, which outlives the access token —
 * so a dead token still let you into /app and every request then 401'd. This
 * watches for both signals and bounces to the IdP:
 *
 *   1. session.error === "RefreshTokenError" — refresh failed server-side.
 *   2. Any API call came back 401 (see apiFetch in lib/api.ts).
 *
 * Renders nothing.
 */
export function SessionGuard() {
  const { data: session, status } = useSession();
  const pathname = usePathname();

  const sessionError = session?.error;
  // Public pages should stay usable while signed out; only guard the dashboard.
  const isProtected = pathname?.startsWith("/app") ?? false;

  useEffect(() => {
    if (!isProtected) return;

    function bounceToLogin() {
      if (redirecting) return;
      redirecting = true;
      void signIn("klogs", { redirectTo: pathname ?? "/app" });
    }

    // 1. Server-side refresh already failed.
    if (status === "authenticated" && sessionError === "RefreshTokenError") {
      bounceToLogin();
      return;
    }

    // 2. A request 401'd — the token is rejected even if the cookie looks fine.
    window.addEventListener(AUTH_UNAUTHORIZED_EVENT, bounceToLogin);
    return () => window.removeEventListener(AUTH_UNAUTHORIZED_EVENT, bounceToLogin);
  }, [status, sessionError, pathname, isProtected]);

  return null;
}

/** Module-scoped so concurrent 401s can only trigger one navigation. */
let redirecting = false;

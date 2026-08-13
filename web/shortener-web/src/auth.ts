import NextAuth from "next-auth";
import type { DefaultSession } from "next-auth";

const ISSUER = process.env.AUTH_ISSUER ?? "https://idp.klogs.io";
const CLIENT_ID = process.env.AUTH_CLIENT_ID ?? "klogs.web";
const CLIENT_SECRET = process.env.AUTH_CLIENT_SECRET ?? "";
// offline_access is what makes the IdP hand back a refresh token; override if
// your provider names it differently.
const SCOPE = process.env.AUTH_SCOPE ?? "openid profile email offline_access";

/** Refresh slightly early so a request never races the actual expiry. */
const EXPIRY_SKEW_SECONDS = 30;

export type SessionError = "RefreshTokenError";

declare module "next-auth" {
  interface Session extends DefaultSession {
    accessToken?: string;
    error?: SessionError;
  }
}

/**
 * Fields we stash on the JWT. Augmenting `next-auth/jwt` does not resolve under
 * next-auth v5 beta, so the callback casts to this instead.
 */
interface TokenExtras {
  accessToken?: string;
  refreshToken?: string;
  expiresAt?: number;
  error?: SessionError;
}

/** Cached OIDC discovery — the token endpoint is needed for refresh. */
let discovery: Promise<string> | null = null;

function getTokenEndpoint(): Promise<string> {
  discovery ??= fetch(`${ISSUER.replace(/\/$/, "")}/.well-known/openid-configuration`)
    .then((res) => {
      if (!res.ok) throw new Error(`OIDC discovery failed (${res.status})`);
      return res.json() as Promise<{ token_endpoint: string }>;
    })
    .then((config) => config.token_endpoint)
    .catch((err) => {
      discovery = null; // allow a retry on the next request
      throw err;
    });
  return discovery;
}

export const { handlers, auth, signIn, signOut } = NextAuth({
  trustHost: true,
  providers: [
    {
      id: "klogs",
      name: "Klogs",
      type: "oidc",
      issuer: ISSUER,
      clientId: CLIENT_ID,
      clientSecret: CLIENT_SECRET,
      authorization: {
        params: {
          scope: SCOPE,
          response_type: "code",
        },
      },
    },
  ],
  callbacks: {
    async jwt({ token, account }) {
      const t = token as typeof token & TokenExtras;

      // Initial sign-in: capture the whole token set.
      if (account) {
        t.accessToken = account.access_token;
        t.refreshToken = account.refresh_token;
        t.expiresAt =
          account.expires_at ?? Math.floor(Date.now() / 1000) + Number(account.expires_in ?? 3600);
        t.error = undefined;
        return t;
      }

      // Still valid — nothing to do.
      if (t.expiresAt && Date.now() < (t.expiresAt - EXPIRY_SKEW_SECONDS) * 1000) {
        return t;
      }

      // Expired and nothing to refresh with: the session is dead.
      if (!t.refreshToken) {
        return { ...t, accessToken: undefined, error: "RefreshTokenError" as const };
      }

      try {
        const endpoint = await getTokenEndpoint();
        const res = await fetch(endpoint, {
          method: "POST",
          headers: { "Content-Type": "application/x-www-form-urlencoded" },
          body: new URLSearchParams({
            grant_type: "refresh_token",
            refresh_token: t.refreshToken,
            client_id: CLIENT_ID,
            ...(CLIENT_SECRET ? { client_secret: CLIENT_SECRET } : {}),
          }),
        });

        const data = (await res.json()) as {
          access_token?: string;
          refresh_token?: string;
          expires_in?: number;
          error?: string;
        };

        if (!res.ok || !data.access_token) {
          throw new Error(data.error ?? `Token refresh failed (${res.status})`);
        }

        return {
          ...t,
          accessToken: data.access_token,
          // Refresh tokens are often single-use — keep the rotated one.
          refreshToken: data.refresh_token ?? t.refreshToken,
          expiresAt: Math.floor(Date.now() / 1000) + Number(data.expires_in ?? 3600),
          error: undefined,
        };
      } catch {
        return { ...t, accessToken: undefined, error: "RefreshTokenError" as const };
      }
    },

    async session({ session, token }) {
      const t = token as typeof token & TokenExtras;
      session.accessToken = t.accessToken;
      session.error = t.error;
      return session;
    },
  },
  pages: {
    signIn: "/login",
  },
});

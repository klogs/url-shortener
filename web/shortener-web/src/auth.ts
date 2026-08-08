import NextAuth from "next-auth";
import type { DefaultSession } from "next-auth";

declare module "next-auth" {
  interface Session extends DefaultSession {
    accessToken?: string;
  }
}

export const { handlers, auth, signIn, signOut } = NextAuth({
  providers: [
    {
      id: "klogs",
      name: "Klogs",
      type: "oidc",
      issuer: process.env.AUTH_ISSUER ?? "https://idp.klogs.io",
      clientId: process.env.AUTH_CLIENT_ID ?? "klogs.web",
      clientSecret: process.env.AUTH_CLIENT_SECRET ?? "",
      authorization: {
        params: {
          scope: "openid profile email",
          response_type: "code",
        },
      },
    },
  ],
  callbacks: {
    async jwt({ token, account }) {
      if (account?.access_token) {
        token.accessToken = account.access_token;
      }
      return token;
    },
    async session({ session, token }) {
      session.accessToken = token.accessToken as string | undefined;
      return session;
    },
  },
  pages: {
    signIn: "/login",
  },
});

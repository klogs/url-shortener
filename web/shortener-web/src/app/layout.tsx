import type { Metadata } from "next";
import "./globals.css";
import { AuthProvider } from "@/components/AuthProvider";
import { ThemeProvider } from "@/components/ThemeProvider";
import { SessionGuard } from "@/components/SessionGuard";

export const metadata: Metadata = {
  title: "klo.gs — Link Shortener by klogs",
  description: "Fast, reliable link shortening. Shorten any URL instantly — no sign-up needed.",
};

/**
 * Runs before first paint so the correct theme class is on <html> already —
 * without it the page flashes the default theme during hydration.
 */
const themeScript = `
(function () {
  try {
    var stored = localStorage.getItem("klogs-theme");
    var dark = stored
      ? stored === "dark"
      : !window.matchMedia("(prefers-color-scheme: light)").matches;
    document.documentElement.classList.toggle("dark", dark);
  } catch (e) {
    document.documentElement.classList.add("dark");
  }
})();
`;

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="en" className="h-full" suppressHydrationWarning>
      <head>
        <script dangerouslySetInnerHTML={{ __html: themeScript }} />
      </head>
      <body className="min-h-full flex flex-col bg-background text-foreground">
        <ThemeProvider>
          <AuthProvider>
            <SessionGuard />
            {children}
          </AuthProvider>
        </ThemeProvider>
      </body>
    </html>
  );
}

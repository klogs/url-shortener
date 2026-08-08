// Management panel layout — requires authentication
// Phase 1: add auth guard (OIDC session check), sidebar, nav

import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "Shortener — Dashboard",
};

export default function AppLayout({ children }: LayoutProps<"/app">) {
  return (
    <div className="flex min-h-full">
      {/* Sidebar nav will go here */}
      <main className="flex-1 p-8">{children}</main>
    </div>
  );
}

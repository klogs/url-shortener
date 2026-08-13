import type { Metadata } from "next";
import Link from "next/link";
import { UserMenu } from "@/components/UserMenu";
import { SidebarBrand } from "@/components/SidebarBrand";

export const metadata: Metadata = {
  title: "klo.gs — Dashboard",
};

export default function AppLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex h-screen overflow-hidden bg-background">
      <nav className="w-56 shrink-0 border-r border-edge bg-surface flex flex-col h-full">
        <SidebarBrand />
        <div className="px-3 py-4 flex flex-col gap-0.5 flex-1 overflow-y-auto">
          <p className="px-3 mb-3 text-xs font-semibold uppercase tracking-widest text-muted">
            Dashboard
          </p>
          <NavLink href="/app">Overview</NavLink>
          <NavLink href="/app/links">Links</NavLink>
          <NavLink href="/app/domains">Domains</NavLink>
          <NavLink href="/app/usage">Usage</NavLink>
          <NavLink href="/app/admin">Admin</NavLink>
        </div>
        <div className="px-3 py-4 border-t border-edge space-y-0.5">
          <NavLink href="/">← Home</NavLink>
          <UserMenu />
        </div>
      </nav>
      <main className="flex-1 p-8 overflow-y-auto">{children}</main>
    </div>
  );
}

function NavLink({ href, children }: { href: string; children: React.ReactNode }) {
  return (
    <Link
      href={href}
      className="block rounded-lg px-3 py-2 text-sm font-medium text-muted
        hover:text-foreground hover:bg-black/5 dark:hover:bg-white/5 transition-colors"
    >
      {children}
    </Link>
  );
}

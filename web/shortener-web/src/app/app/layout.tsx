import type { Metadata } from "next";
import Link from "next/link";
import { UserMenu } from "@/components/UserMenu";

export const metadata: Metadata = {
  title: "Shortener — Dashboard",
};

export default function AppLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex h-screen overflow-hidden">
      <nav className="w-56 shrink-0 border-r border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-950 flex flex-col h-full">
        <div className="px-4 py-6 flex flex-col gap-1 flex-1 overflow-y-auto">
          <p className="px-2 mb-4 text-xs font-semibold uppercase tracking-widest text-zinc-400 dark:text-zinc-600">
            Dashboard
          </p>
          <NavLink href="/app">Overview</NavLink>
          <NavLink href="/app/links">Links</NavLink>
          <NavLink href="/app/domains">Domains</NavLink>
          <NavLink href="/app/admin">Admin</NavLink>
        </div>
        <div className="px-4 py-4 border-t border-zinc-200 dark:border-zinc-800 space-y-1">
          <NavLink href="/">← Back to home</NavLink>
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
      className="block rounded-lg px-3 py-2 text-sm font-medium text-zinc-700 hover:bg-zinc-200
        dark:text-zinc-300 dark:hover:bg-zinc-800 transition-colors"
    >
      {children}
    </Link>
  );
}

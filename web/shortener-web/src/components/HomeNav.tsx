"use client";

import { useState, useRef, useEffect } from "react";
import Link from "next/link";
import { useSession, signOut } from "next-auth/react";
import { KlogsLogo } from "@/components/KlogsLogo";
import { ThemeToggle } from "@/components/ThemeToggle";
import { useTheme } from "@/components/ThemeProvider";

export function HomeNav() {
  const { data: session, status } = useSession();
  const { theme } = useTheme();
  const [open, setOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setOpen(false);
      }
    }
    if (open) { document.addEventListener("mousedown", handleClickOutside); }
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [open]);

  return (
    <header className="w-full border-b border-edge bg-background/90 backdrop-blur-sm sticky top-0 z-30">
      <div className="mx-auto flex max-w-5xl items-center justify-between px-4 h-14">
        <Link href="/" className="flex items-center hover:opacity-80 transition-opacity">
          <KlogsLogo height={22} textFill={theme === "dark" ? "#ffffff" : "#10101C"} />
        </Link>

        <nav className="flex items-center gap-2">
          <ThemeToggle className="mr-1" />
          {status === "loading" ? (
            <span className="h-7 w-20 animate-pulse rounded-lg bg-zinc-100 dark:bg-zinc-800" />
          ) : session?.user ? (
            <div className="relative" ref={menuRef}>
              <button
                onClick={() => setOpen((v) => !v)}
                className="flex items-center gap-2 rounded-lg px-3 py-1.5 text-sm hover:bg-zinc-100
                  dark:hover:bg-zinc-800 transition-colors"
              >
                {session.user.image ? (
                  // eslint-disable-next-line @next/next/no-img-element
                  <img
                    src={session.user.image}
                    alt=""
                    className="h-6 w-6 rounded-full object-cover"
                  />
                ) : (
                  <span className="flex h-6 w-6 items-center justify-center rounded-full bg-zinc-200 dark:bg-zinc-700 text-xs font-medium">
                    {(session.user.name ?? session.user.email ?? "?")[0].toUpperCase()}
                  </span>
                )}
                <span className="max-w-[120px] truncate text-sm font-medium">
                  {session.user.name ?? session.user.email}
                </span>
                <svg className="h-3.5 w-3.5 text-zinc-400" viewBox="0 0 16 16" fill="currentColor">
                  <path d="M8 10.5L3 5.5h10L8 10.5z" />
                </svg>
              </button>

              {open && (
                <div className="absolute right-0 mt-1 w-48 rounded-xl border border-zinc-200 dark:border-zinc-700
                  bg-white dark:bg-zinc-900 shadow-lg py-1 text-sm">
                  <Link
                    href="/app"
                    onClick={() => setOpen(false)}
                    className="flex items-center gap-2 px-4 py-2 text-zinc-700 dark:text-zinc-300
                      hover:bg-zinc-50 dark:hover:bg-zinc-800 transition-colors"
                  >
                    <svg className="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                      <path d="M10 2a8 8 0 100 16A8 8 0 0010 2zm0 3a1 1 0 011 1v3.586l2.707 2.707a1 1 0 01-1.414 1.414l-3-3A1 1 0 019 10V6a1 1 0 011-1z" />
                    </svg>
                    Dashboard
                  </Link>
                  <Link
                    href="/app/links"
                    onClick={() => setOpen(false)}
                    className="flex items-center gap-2 px-4 py-2 text-zinc-700 dark:text-zinc-300
                      hover:bg-zinc-50 dark:hover:bg-zinc-800 transition-colors"
                  >
                    <svg className="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                      <path fillRule="evenodd" d="M12.586 4.586a2 2 0 112.828 2.828l-3 3a2 2 0 01-2.828 0 1 1 0 00-1.414 1.414 4 4 0 005.656 0l3-3a4 4 0 00-5.656-5.656l-1.5 1.5a1 1 0 101.414 1.414l1.5-1.5zm-5 5a2 2 0 012.828 0 1 1 0 101.414-1.414 4 4 0 00-5.656 0l-3 3a4 4 0 105.656 5.656l1.5-1.5a1 1 0 10-1.414-1.414l-1.5 1.5a2 2 0 11-2.828-2.828l3-3z" clipRule="evenodd" />
                    </svg>
                    My links
                  </Link>
                  <div className="my-1 h-px bg-zinc-100 dark:bg-zinc-800" />
                  <button
                    onClick={() => { setOpen(false); void signOut({ redirectTo: "/" }); }}
                    className="flex w-full items-center gap-2 px-4 py-2 text-zinc-500 dark:text-zinc-400
                      hover:bg-zinc-50 dark:hover:bg-zinc-800 transition-colors"
                  >
                    <svg className="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                      <path fillRule="evenodd" d="M3 3a1 1 0 00-1 1v12a1 1 0 001 1h6a1 1 0 000-2H4V5h5a1 1 0 000-2H3zm10.293 4.293a1 1 0 011.414 0L17 9.586A2 2 0 0117 12l-2.293 2.293a1 1 0 01-1.414-1.414L14.586 12H9a1 1 0 010-2h5.586l-1.293-1.293a1 1 0 010-1.414z" clipRule="evenodd" />
                    </svg>
                    Sign out
                  </button>
                </div>
              )}
            </div>
          ) : (
            <div className="flex items-center gap-2">
              <Link
                href="/app"
                className="rounded-lg px-3 py-1.5 text-sm font-medium text-zinc-400 hover:text-white transition-colors"
              >
                Sign in
              </Link>
              <Link
                href="/app"
                className="rounded-lg bg-[#0D4DFF] hover:bg-[#3E6EFF] px-3 py-1.5 text-sm font-medium text-white transition-colors"
              >
                Get started
              </Link>
            </div>
          )}
        </nav>
      </div>
    </header>
  );
}

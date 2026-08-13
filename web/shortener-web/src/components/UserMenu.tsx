"use client";

import { useSession, signOut } from "next-auth/react";

export function UserMenu() {
  const { data: session } = useSession();

  if (!session?.user) return null;

  return (
    <div className="flex items-center gap-2 px-2 py-1.5">
      {session.user.image && (
        // eslint-disable-next-line @next/next/no-img-element
        <img
          src={session.user.image}
          alt=""
          className="h-6 w-6 rounded-full object-cover"
        />
      )}
      <span className="flex-1 truncate text-xs text-zinc-600 dark:text-zinc-400">
        {session.user.name ?? session.user.email}
      </span>
      <button
        onClick={() => signOut({ redirectTo: "/" })}
        className="shrink-0 rounded px-1.5 py-0.5 text-xs text-muted
          hover:bg-black/5 dark:hover:bg-white/8 transition-colors"
      >
        Sign out
      </button>
    </div>
  );
}

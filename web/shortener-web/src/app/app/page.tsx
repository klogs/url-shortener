"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import Link from "next/link";
import { getTenantStats, type TenantStats } from "@/lib/api";

export default function DashboardPage() {
  const { data: session, status } = useSession();
  const [stats, setStats] = useState<TenantStats | null>(null);

  useEffect(() => {
    if (status !== "authenticated" || !session?.accessToken) return;
    getTenantStats(session.accessToken)
      .then(setStats)
      .catch(() => null);
  }, [session, status]);

  return (
    <div className="space-y-6 max-w-3xl">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Overview</h1>
        <p className="mt-1 text-sm text-zinc-500 dark:text-zinc-400">
          Manage your short links and view analytics.
        </p>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <StatCard
          label="Total links"
          value={stats ? stats.totalLinks.toLocaleString() : "—"}
        />
        <StatCard
          label="Active links"
          value={stats ? stats.activeLinks.toLocaleString() : "—"}
        />
        <StatCard
          label="Clicks today"
          value={stats ? stats.clicksToday.toLocaleString() : "—"}
        />
      </div>

      <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 p-6">
        <h2 className="text-sm font-semibold text-zinc-700 dark:text-zinc-300 mb-3">
          Quick actions
        </h2>
        <div className="flex flex-wrap gap-3">
          <Link
            href="/app/links"
            className="rounded-lg border border-zinc-200 dark:border-zinc-700 px-4 py-2 text-sm
              font-medium hover:bg-zinc-100 dark:hover:bg-zinc-800 transition-colors"
          >
            View all links →
          </Link>
          <Link
            href="/"
            className="rounded-lg bg-zinc-900 dark:bg-white text-white dark:text-zinc-900
              px-4 py-2 text-sm font-medium hover:bg-zinc-700 dark:hover:bg-zinc-200 transition-colors"
          >
            Create a link
          </Link>
        </div>
      </div>
    </div>
  );
}

function StatCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 p-5">
      <p className="text-xs font-medium uppercase tracking-wider text-zinc-400 dark:text-zinc-600">
        {label}
      </p>
      <p className="mt-2 text-3xl font-semibold text-zinc-900 dark:text-white">{value}</p>
    </div>
  );
}

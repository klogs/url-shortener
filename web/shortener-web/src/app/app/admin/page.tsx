"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import Link from "next/link";
import { getSystemStats, type SystemStats } from "@/lib/api";

export default function AdminPage() {
  const { data: session, status } = useSession();
  const [stats, setStats] = useState<SystemStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (status === "loading") return;
    if (!session?.accessToken) return;

    getSystemStats(session.accessToken)
      .then(setStats)
      .catch((err: unknown) =>
        setError((err as { message?: string }).message ?? "Failed to load stats.")
      )
      .finally(() => setLoading(false));
  }, [session, status]);

  if (loading || status === "loading") {
    return (
      <div className="flex items-center justify-center py-24 text-sm text-zinc-400">
        Loading…
      </div>
    );
  }

  if (error) {
    return (
      <div className="max-w-3xl">
        <h1 className="text-2xl font-semibold tracking-tight mb-4">Admin</h1>
        <p className="text-sm text-red-600 dark:text-red-400">{error}</p>
      </div>
    );
  }

  return (
    <div className="max-w-3xl space-y-8">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Admin</h1>
        <p className="mt-1 text-sm text-zinc-500 dark:text-zinc-400">
          System-wide overview. Only visible to admins.
        </p>
      </div>

      {stats && (
        <div className="grid grid-cols-2 gap-4">
          <StatCard label="Total tenants" value={stats.totalTenants.toLocaleString()} />
          <StatCard label="Total links" value={stats.totalLinks.toLocaleString()} />
        </div>
      )}

      <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 divide-y divide-zinc-200 dark:divide-zinc-800">
        <AdminLink href="/app/admin/tenants" label="Manage tenants" description="View all tenants, usage, and update plans." />
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

function AdminLink({
  href,
  label,
  description,
}: {
  href: string;
  label: string;
  description: string;
}) {
  return (
    <Link
      href={href}
      className="flex items-center justify-between px-5 py-4 hover:bg-zinc-50 dark:hover:bg-zinc-900 transition-colors"
    >
      <div>
        <p className="text-sm font-medium text-zinc-800 dark:text-zinc-200">{label}</p>
        <p className="text-xs text-zinc-500 dark:text-zinc-400 mt-0.5">{description}</p>
      </div>
      <span className="text-zinc-400 dark:text-zinc-600 text-lg">→</span>
    </Link>
  );
}

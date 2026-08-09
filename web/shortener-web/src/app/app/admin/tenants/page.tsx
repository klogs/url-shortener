"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import Link from "next/link";
import { listAllTenants, type TenantSummary } from "@/lib/api";

export default function AdminTenantsPage() {
  const { data: session, status } = useSession();
  const [items, setItems] = useState<TenantSummary[]>([]);
  const [hasMore, setHasMore] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [cursor, setCursor] = useState<string | undefined>(undefined);

  const load = (after?: string) => {
    if (!session?.accessToken) return;
    setLoading(true);
    listAllTenants(session.accessToken, 25, after)
      .then((res) => {
        setItems((prev) => (after ? [...prev, ...res.items] : res.items));
        setHasMore(res.hasMore);
        if (res.items.length > 0) {
          setCursor(res.items[res.items.length - 1].id);
        }
      })
      .catch((err: unknown) =>
        setError((err as { message?: string }).message ?? "Failed to load tenants.")
      )
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    if (status === "loading") return;

    load(undefined);

    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [session, status]);

  return (
    <div className="max-w-4xl space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Tenants</h1>
        <p className="mt-1 text-sm text-zinc-500 dark:text-zinc-400">
          All registered tenants and their current plan.
        </p>
      </div>

      {error && (
        <p className="text-sm text-red-600 dark:text-red-400">{error}</p>
      )}

      <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-950">
              <th className="px-4 py-3 text-left font-medium text-zinc-500 dark:text-zinc-400">
                Name
              </th>
              <th className="px-4 py-3 text-left font-medium text-zinc-500 dark:text-zinc-400">
                Plan
              </th>
              <th className="px-4 py-3 text-left font-medium text-zinc-500 dark:text-zinc-400">
                Status
              </th>
              <th className="px-4 py-3 text-left font-medium text-zinc-500 dark:text-zinc-400">
                Created
              </th>
              <th className="px-4 py-3" />
            </tr>
          </thead>
          <tbody className="divide-y divide-zinc-100 dark:divide-zinc-800">
            {items.map((t) => (
              <tr key={t.id} className="hover:bg-zinc-50 dark:hover:bg-zinc-900/50">
                <td className="px-4 py-3 font-medium text-zinc-800 dark:text-zinc-200">
                  {t.name}
                </td>
                <td className="px-4 py-3">
                  <PlanBadge plan={t.plan} />
                </td>
                <td className="px-4 py-3">
                  {t.isActive ? (
                    <span className="text-xs font-medium text-green-600 dark:text-green-400">
                      Active
                    </span>
                  ) : (
                    <span className="text-xs font-medium text-zinc-400">Inactive</span>
                  )}
                </td>
                <td className="px-4 py-3 text-zinc-500 dark:text-zinc-400">
                  {new Date(t.createdAtUtc).toLocaleDateString()}
                </td>
                <td className="px-4 py-3 text-right">
                  <Link
                    href={`/app/admin/tenants/${t.id}`}
                    className="text-xs font-medium text-zinc-500 hover:text-zinc-900 dark:hover:text-zinc-100 transition-colors"
                  >
                    Manage →
                  </Link>
                </td>
              </tr>
            ))}
            {items.length === 0 && !loading && (
              <tr>
                <td colSpan={5} className="px-4 py-8 text-center text-zinc-400 text-sm">
                  No tenants found.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {hasMore && (
        <button
          onClick={() => load(cursor)}
          disabled={loading}
          className="text-sm text-zinc-500 hover:text-zinc-900 dark:hover:text-zinc-100 disabled:opacity-50 transition-colors"
        >
          {loading ? "Loading…" : "Load more"}
        </button>
      )}
    </div>
  );
}

function PlanBadge({ plan }: { plan: string }) {
  const key = plan.charAt(0).toUpperCase() + plan.slice(1).toLowerCase();
  const styles: Record<string, string> = {
    Free: "bg-zinc-100 text-zinc-600 dark:bg-zinc-800 dark:text-zinc-400",
    Pro: "bg-blue-100 text-blue-700 dark:bg-blue-950 dark:text-blue-400",
    Enterprise: "bg-purple-100 text-purple-700 dark:bg-purple-950 dark:text-purple-400",
  };
  return (
    <span
      className={`inline-block rounded-full px-2.5 py-0.5 text-xs font-semibold ${styles[key] ?? styles.Free}`}
    >
      {key}
    </span>
  );
}

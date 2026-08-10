"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { getTenantUsage, type TenantUsage } from "@/lib/api";

export default function UsagePage() {
  const { data: session, status } = useSession();
  const [usage, setUsage] = useState<TenantUsage | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (status === "loading") return;
    if (!session?.accessToken) return;

    getTenantUsage(session.accessToken)
      .then(setUsage)
      .catch((err: unknown) =>
        setError((err as { message?: string }).message ?? "Failed to load usage.")
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
      <div className="max-w-2xl">
        <h1 className="text-2xl font-semibold tracking-tight mb-4">Plan &amp; Usage</h1>
        <p className="text-sm text-red-600 dark:text-red-400">{error}</p>
      </div>
    );
  }

  return (
    <div className="max-w-2xl space-y-8">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Plan &amp; Usage</h1>
        <p className="mt-1 text-sm text-zinc-500 dark:text-zinc-400">
          Current quota consumption for your tenant.
        </p>
      </div>

      {usage && (
        <>
          <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 p-5 flex items-center justify-between">
            <div>
              <p className="text-xs font-medium uppercase tracking-wider text-zinc-400 dark:text-zinc-600">
                Current plan
              </p>
              <p className="mt-1 text-2xl font-semibold text-zinc-900 dark:text-white capitalize">
                {usage.plan}
              </p>
            </div>
            <PlanBadge plan={usage.plan} />
          </div>

          <div className="space-y-6">
            <QuotaBar
              label="Short links"
              used={usage.linkCount}
              max={usage.limits.maxLinks}
            />
            <QuotaBar
              label="Custom domains"
              used={usage.customDomainCount}
              max={usage.limits.maxCustomDomains}
            />
            <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 p-5">
              <p className="text-xs font-medium uppercase tracking-wider text-zinc-400 dark:text-zinc-600 mb-1">
                Analytics retention
              </p>
              <p className="text-2xl font-semibold text-zinc-900 dark:text-white">
                {usage.limits.analyticsDays} days
              </p>
            </div>
          </div>
        </>
      )}
    </div>
  );
}

function QuotaBar({ label, used, max }: { label: string; used: number; max: number }) {
  const unlimited = max === -1;
  const pct = unlimited ? 0 : Math.min(100, Math.round((used / max) * 100));
  const nearLimit = !unlimited && pct >= 80;
  const atLimit = !unlimited && used >= max;

  return (
    <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 p-5 space-y-3">
      <div className="flex items-center justify-between">
        <p className="text-sm font-medium text-zinc-700 dark:text-zinc-300">{label}</p>
        <p className="text-sm text-zinc-500 dark:text-zinc-400">
          {used.toLocaleString()}
          {unlimited ? " / unlimited" : ` / ${max.toLocaleString()}`}
        </p>
      </div>

      {!unlimited && (
        <div className="h-2 w-full rounded-full bg-zinc-100 dark:bg-zinc-800 overflow-hidden">
          <div
            className={`h-full rounded-full transition-all ${
              atLimit
                ? "bg-red-500"
                : nearLimit
                ? "bg-amber-400"
                : "bg-zinc-900 dark:bg-white"
            }`}
            style={{ width: `${pct}%` }}
          />
        </div>
      )}

      {atLimit && (
        <p className="text-xs text-red-600 dark:text-red-400 font-medium">
          Quota reached — upgrade your plan to add more.
        </p>
      )}
    </div>
  );
}

function PlanBadge({ plan }: { plan: number }) {
  const styles: Record<string, string> = {
    Free: "bg-zinc-100 text-zinc-600 dark:bg-zinc-800 dark:text-zinc-400",
    Pro: "bg-blue-100 text-blue-700 dark:bg-blue-950 dark:text-blue-400",
    Enterprise: "bg-purple-100 text-purple-700 dark:bg-purple-950 dark:text-purple-400",
  };

  const kv: Record<number, string> = {
    0: "Free",
    1: "Pro",
    2: "Enterprise",
  };

  const planName = kv[plan] ?? "Free";

  return (
    <span
      className={`inline-block rounded-full px-3 py-1 text-sm font-semibold ${styles[planName] ?? styles.Free}`}
    >
      {planName}
    </span>
  );
}

"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { use } from "react";
import {
  getAdminTenantUsage,
  changeTenantPlan,
  type TenantUsage,
} from "@/lib/api";

const PLANS = [
  { label: "Free", value: 0 },
  { label: "Pro", value: 1 },
  { label: "Enterprise", value: 2 },
];

export default function AdminTenantDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  const { data: session, status } = useSession();
  const [usage, setUsage] = useState<TenantUsage | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [saveError, setSaveError] = useState<string | null>(null);
  const [saveSuccess, setSaveSuccess] = useState(false);
  const [selectedPlan, setSelectedPlan] = useState<number>(0);

  useEffect(() => {
    if (status === "loading") return;
    if (!session?.accessToken) return;

    getAdminTenantUsage(session.accessToken, id)
      .then((u) => {
        setUsage(u);
        const current = PLANS.find(
          (p) => p.value == u.plan
        );
        setSelectedPlan(current?.value ?? 0);
      })
      .catch((err: unknown) =>
        setError((err as { message?: string }).message ?? "Failed to load tenant.")
      )
      .finally(() => setLoading(false));
  }, [id, session, status]);

  const handleSavePlan = async () => {
    if (!session?.accessToken) return;
    setSaving(true);
    setSaveError(null);
    setSaveSuccess(false);
    try {
      await changeTenantPlan(session.accessToken, id, selectedPlan);
      setSaveSuccess(true);
      const updated = await getAdminTenantUsage(session.accessToken, id);
      setUsage(updated);
    } catch (err: unknown) {
      setSaveError((err as { message?: string }).message ?? "Failed to save plan.");
    } finally {
      setSaving(false);
    }
  };

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
        <h1 className="text-2xl font-semibold tracking-tight mb-4">Tenant detail</h1>
        <p className="text-sm text-red-600 dark:text-red-400">{error}</p>
      </div>
    );
  }

  return (
    <div className="max-w-2xl space-y-8">
      <div>
        <p className="text-xs font-medium uppercase tracking-wider text-zinc-400 dark:text-zinc-600 mb-1">
          Tenant ID
        </p>
        <h1 className="text-2xl font-semibold tracking-tight font-mono text-zinc-700 dark:text-zinc-300 text-sm">
          {id}
        </h1>
      </div>

      {usage && (
        <>
          <div className="grid grid-cols-3 gap-4">
            <StatCard label="Links" value={String(usage.linkCount)} />
            <StatCard label="Custom domains" value={String(usage.customDomainCount)} />
            <StatCard label="Analytics retention" value={`${usage.limits.analyticsDays}d`} />
          </div>

          <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 p-5 space-y-4">
            <h2 className="text-base font-semibold text-zinc-800 dark:text-zinc-200">
              Change plan
            </h2>

            <div className="flex gap-3">
              {PLANS.map((p) => (
                <button
                  key={p.value}
                  onClick={() => setSelectedPlan(p.value)}
                  className={`flex-1 rounded-lg border px-3 py-2 text-sm font-medium transition-colors ${
                    selectedPlan === p.value
                      ? "border-zinc-900 dark:border-white bg-zinc-900 dark:bg-white text-white dark:text-zinc-900"
                      : "border-zinc-200 dark:border-zinc-700 text-zinc-600 dark:text-zinc-400 hover:border-zinc-400 dark:hover:border-zinc-500"
                  }`}
                >
                  {p.label}
                </button>
              ))}
            </div>

            {saveError && (
              <p className="text-sm text-red-600 dark:text-red-400">{saveError}</p>
            )}
            {saveSuccess && (
              <p className="text-sm text-green-600 dark:text-green-400">Plan updated.</p>
            )}

            <button
              onClick={handleSavePlan}
              disabled={saving}
              className="w-full rounded-lg bg-zinc-900 dark:bg-white text-white dark:text-zinc-900 px-4 py-2 text-sm font-medium hover:bg-zinc-700 dark:hover:bg-zinc-200 disabled:opacity-50 transition-colors"
            >
              {saving ? "Saving…" : "Save plan"}
            </button>
          </div>
        </>
      )}
    </div>
  );
}

function StatCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 p-4">
      <p className="text-xs font-medium uppercase tracking-wider text-zinc-400 dark:text-zinc-600">
        {label}
      </p>
      <p className="mt-1.5 text-2xl font-semibold text-zinc-900 dark:text-white">{value}</p>
    </div>
  );
}

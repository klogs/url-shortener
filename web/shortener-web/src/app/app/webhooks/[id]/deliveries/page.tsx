"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { listWebhookDeliveries, type WebhookDelivery } from "@/lib/api";

export default function WebhookDeliveriesPage() {
  const { data: session, status } = useSession();
  const params = useParams<{ id: string }>();
  const [deliveries, setDeliveries] = useState<WebhookDelivery[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (status === "loading") return;
    if (!session?.accessToken || !params.id) return;

    listWebhookDeliveries(session.accessToken, params.id)
      .then(setDeliveries)
      .catch((err: unknown) =>
        setError((err as { message?: string }).message ?? "Failed to load deliveries.")
      )
      .finally(() => setLoading(false));
  }, [session, status, params.id]);

  if (loading || status === "loading") {
    return (
      <div className="flex items-center justify-center py-24 text-sm text-zinc-400">
        Loading…
      </div>
    );
  }

  if (error) {
    return (
      <div className="max-w-4xl">
        <h1 className="text-2xl font-semibold tracking-tight mb-4">Delivery History</h1>
        <p className="text-sm text-red-600 dark:text-red-400">{error}</p>
      </div>
    );
  }

  return (
    <div className="max-w-5xl space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold tracking-tight">Delivery History</h1>
        <Link
          href="/app/webhooks"
          className="text-sm text-zinc-500 hover:text-zinc-900 dark:hover:text-white transition-colors"
        >
          ← Back to webhooks
        </Link>
      </div>

      {deliveries.length === 0 ? (
        <div className="rounded-xl border border-dashed border-zinc-300 dark:border-zinc-700 p-12 text-center">
          <p className="text-zinc-500 dark:text-zinc-400">No deliveries yet.</p>
        </div>
      ) : (
        <div className="overflow-x-auto rounded-xl border border-zinc-200 dark:border-zinc-800">
          <table className="min-w-full divide-y divide-zinc-200 dark:divide-zinc-800 text-sm">
            <thead className="bg-zinc-50 dark:bg-zinc-900">
              <tr>
                <th className="px-4 py-3 text-left font-medium text-zinc-500 dark:text-zinc-400">
                  Event
                </th>
                <th className="px-4 py-3 text-left font-medium text-zinc-500 dark:text-zinc-400">
                  Status
                </th>
                <th className="px-4 py-3 text-right font-medium text-zinc-500 dark:text-zinc-400">
                  HTTP
                </th>
                <th className="px-4 py-3 text-right font-medium text-zinc-500 dark:text-zinc-400">
                  Attempts
                </th>
                <th className="px-4 py-3 text-left font-medium text-zinc-500 dark:text-zinc-400">
                  Created
                </th>
                <th className="px-4 py-3 text-left font-medium text-zinc-500 dark:text-zinc-400">
                  Delivered
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-zinc-100 dark:divide-zinc-800 bg-white dark:bg-zinc-950">
              {deliveries.map((d) => (
                <tr key={d.id} className="hover:bg-zinc-50 dark:hover:bg-zinc-900 transition-colors">
                  <td className="px-4 py-3 font-mono text-xs text-zinc-700 dark:text-zinc-300">
                    {d.eventType}
                  </td>
                  <td className="px-4 py-3">
                    <DeliveryStatusBadge status={d.status} />
                  </td>
                  <td className="px-4 py-3 text-right text-zinc-600 dark:text-zinc-400">
                    {d.lastHttpStatus ?? "—"}
                  </td>
                  <td className="px-4 py-3 text-right text-zinc-600 dark:text-zinc-400">
                    {d.attemptCount}
                  </td>
                  <td className="px-4 py-3 text-zinc-500 whitespace-nowrap">
                    {new Date(d.createdAtUtc).toLocaleString()}
                  </td>
                  <td className="px-4 py-3 text-zinc-500 whitespace-nowrap">
                    {d.deliveredAt ? new Date(d.deliveredAt).toLocaleString() : "—"}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

function DeliveryStatusBadge({ status }: { status: string }) {
  const styles: Record<string, string> = {
    Pending:   "bg-zinc-100 text-zinc-600 dark:bg-zinc-800 dark:text-zinc-400",
    Delivered: "bg-green-100 text-green-700 dark:bg-green-950 dark:text-green-400",
    Failed:    "bg-red-100 text-red-700 dark:bg-red-950 dark:text-red-400",
  };
  return (
    <span className={`inline-block rounded-full px-2 py-0.5 text-xs font-medium ${styles[status] ?? styles.Pending}`}>
      {status}
    </span>
  );
}

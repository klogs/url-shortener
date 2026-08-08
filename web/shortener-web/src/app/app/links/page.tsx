"use client";

import { useEffect, useState } from "react";
import Link from "next/link";

interface LinkSummary {
  id: string;
  shortCode: string;
  destinationUrl: string;
  title: string | null;
  status: string;
  createdAtUtc: string;
  expiresAt: string | null;
  clickCountSnapshot: number;
}

interface LinksResult {
  items: LinkSummary[];
  nextCursor: string | null;
}

export default function LinksPage() {
  const [links, setLinks] = useState<LinkSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const apiUrl = process.env.NEXT_PUBLIC_API_URL ?? "";
    fetch(`${apiUrl}/api/v1/links`, {
      headers: { Accept: "application/json" },
    })
      .then(async (res) => {
        if (res.status === 401 || res.status === 403) {
          setError("sign-in-required");
          return;
        }
        if (!res.ok) {
          throw new Error(`Failed to load links (${res.status})`);
        }
        const data: LinksResult = await res.json();
        setLinks(data.items);
      })
      .catch((err: unknown) => {
        if ((err as { message?: string }).message !== "sign-in-required") {
          setError((err as { message?: string }).message ?? "Failed to load links.");
        }
      })
      .finally(() => setLoading(false));
  }, []);

  if (loading) {
    return (
      <div className="flex items-center justify-center py-24 text-sm text-zinc-400">
        Loading…
      </div>
    );
  }

  if (error === "sign-in-required") {
    return (
      <div className="max-w-3xl space-y-6">
        <h1 className="text-2xl font-semibold tracking-tight">Links</h1>
        <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 p-12 text-center">
          <p className="text-zinc-500 dark:text-zinc-400 mb-4">
            Sign in to view and manage your links.
          </p>
          <a
            href={`${process.env.NEXT_PUBLIC_API_URL ?? ""}/auth/login`}
            className="inline-block rounded-lg bg-zinc-900 dark:bg-white px-5 py-2 text-sm font-medium
              text-white dark:text-zinc-900 hover:bg-zinc-700 dark:hover:bg-zinc-200 transition-colors"
          >
            Sign in
          </a>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="max-w-3xl">
        <h1 className="text-2xl font-semibold tracking-tight mb-4">Links</h1>
        <p className="text-sm text-red-600 dark:text-red-400">{error}</p>
      </div>
    );
  }

  return (
    <div className="max-w-5xl space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold tracking-tight">Links</h1>
        <Link
          href="/"
          className="rounded-lg bg-zinc-900 dark:bg-white text-white dark:text-zinc-900
            px-4 py-2 text-sm font-medium hover:bg-zinc-700 dark:hover:bg-zinc-200 transition-colors"
        >
          + New link
        </Link>
      </div>

      {links.length === 0 ? (
        <div className="rounded-xl border border-dashed border-zinc-300 dark:border-zinc-700 p-12 text-center">
          <p className="text-zinc-500 dark:text-zinc-400 mb-4">No links yet.</p>
          <Link
            href="/"
            className="text-sm font-medium underline text-zinc-700 dark:text-zinc-300 hover:no-underline"
          >
            Create your first link
          </Link>
        </div>
      ) : (
        <div className="overflow-x-auto rounded-xl border border-zinc-200 dark:border-zinc-800">
          <table className="min-w-full divide-y divide-zinc-200 dark:divide-zinc-800 text-sm">
            <thead className="bg-zinc-50 dark:bg-zinc-900">
              <tr>
                <th className="px-4 py-3 text-left font-medium text-zinc-500 dark:text-zinc-400">Code</th>
                <th className="px-4 py-3 text-left font-medium text-zinc-500 dark:text-zinc-400">Destination</th>
                <th className="px-4 py-3 text-left font-medium text-zinc-500 dark:text-zinc-400">Status</th>
                <th className="px-4 py-3 text-right font-medium text-zinc-500 dark:text-zinc-400">Clicks</th>
                <th className="px-4 py-3 text-left font-medium text-zinc-500 dark:text-zinc-400">Created</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-zinc-100 dark:divide-zinc-800 bg-white dark:bg-zinc-950">
              {links.map((link) => (
                <tr key={link.id} className="hover:bg-zinc-50 dark:hover:bg-zinc-900 transition-colors">
                  <td className="px-4 py-3 font-mono font-medium text-zinc-900 dark:text-white">
                    {link.shortCode}
                  </td>
                  <td className="px-4 py-3 max-w-xs truncate text-zinc-600 dark:text-zinc-400">
                    <a
                      href={link.destinationUrl}
                      target="_blank"
                      rel="noopener noreferrer"
                      title={link.destinationUrl}
                      className="hover:underline"
                    >
                      {link.title ?? link.destinationUrl}
                    </a>
                  </td>
                  <td className="px-4 py-3">
                    <StatusBadge status={link.status} />
                  </td>
                  <td className="px-4 py-3 text-right text-zinc-700 dark:text-zinc-300">
                    {link.clickCountSnapshot.toLocaleString()}
                  </td>
                  <td className="px-4 py-3 text-zinc-500 dark:text-zinc-500 whitespace-nowrap">
                    {new Date(link.createdAtUtc).toLocaleDateString()}
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

function StatusBadge({ status }: { status: string }) {
  const styles: Record<string, string> = {
    Active: "bg-green-100 text-green-700 dark:bg-green-950 dark:text-green-400",
    Disabled: "bg-zinc-100 text-zinc-600 dark:bg-zinc-800 dark:text-zinc-400",
    Expired: "bg-amber-100 text-amber-700 dark:bg-amber-950 dark:text-amber-400",
    Deleted: "bg-red-100 text-red-700 dark:bg-red-950 dark:text-red-400",
  };
  return (
    <span className={`inline-block rounded-full px-2 py-0.5 text-xs font-medium ${styles[status] ?? styles.Disabled}`}>
      {status}
    </span>
  );
}

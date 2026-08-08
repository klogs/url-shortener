"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { listDomains, type DomainSummary } from "@/lib/api";

const BASE = process.env.NEXT_PUBLIC_API_URL ?? "";

export default function DomainsPage() {
  const { data: session, status } = useSession();
  const [domains, setDomains] = useState<DomainSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Add domain form
  const [addHost, setAddHost] = useState("");
  const [adding, setAdding] = useState(false);
  const [addError, setAddError] = useState<string | null>(null);

  // Verify state per domain
  const [verifying, setVerifying] = useState<Record<string, boolean>>({});
  const [verifyResult, setVerifyResult] = useState<Record<string, string>>({});

  // Remove state per domain
  const [removing, setRemoving] = useState<Record<string, boolean>>({});
  const [confirmRemove, setConfirmRemove] = useState<string | null>(null);

  const token = session?.accessToken;

  useEffect(() => {
    if (status === "loading" || !token) return;
    listDomains(token)
      .then(setDomains)
      .catch((err: unknown) => setError((err as { message?: string }).message ?? "Failed to load domains"))
      .finally(() => setLoading(false));
  }, [token, status]);

  async function handleAdd() {
    if (!token || !addHost.trim()) return;
    setAdding(true);
    setAddError(null);
    try {
      const res = await fetch(`${BASE}/api/v1/domains`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ host: addHost.trim() }),
      });
      if (!res.ok) {
        const body = await res.json().catch(() => ({})) as { detail?: string };
        throw new Error(body.detail ?? `Failed (${res.status})`);
      }
      const newDomain = await res.json() as DomainSummary;
      setDomains((prev) => [...prev, newDomain]);
      setAddHost("");
    } catch (err: unknown) {
      setAddError((err as { message?: string }).message ?? "Failed to add domain.");
    } finally {
      setAdding(false);
    }
  }

  async function handleVerify(domainId: string) {
    if (!token) return;
    setVerifying((v) => ({ ...v, [domainId]: true }));
    setVerifyResult((v) => ({ ...v, [domainId]: "" }));
    try {
      const res = await fetch(`${BASE}/api/v1/domains/${domainId}/verify`, {
        method: "POST",
        headers: { Authorization: `Bearer ${token}` },
      });
      if (res.ok) {
        setDomains((prev) =>
          prev.map((d) => (d.id === domainId ? { ...d, isVerified: true } : d))
        );
        setVerifyResult((v) => ({ ...v, [domainId]: "Verified!" }));
      } else {
        const body = await res.json().catch(() => ({})) as { detail?: string };
        setVerifyResult((v) => ({ ...v, [domainId]: body.detail ?? "Verification failed." }));
      }
    } catch {
      setVerifyResult((v) => ({ ...v, [domainId]: "Network error." }));
    } finally {
      setVerifying((v) => ({ ...v, [domainId]: false }));
    }
  }

  async function handleRemove(domainId: string) {
    if (!token) return;
    setRemoving((r) => ({ ...r, [domainId]: true }));
    try {
      const res = await fetch(`${BASE}/api/v1/domains/${domainId}`, {
        method: "DELETE",
        headers: { Authorization: `Bearer ${token}` },
      });
      if (res.ok || res.status === 404) {
        setDomains((prev) => prev.filter((d) => d.id !== domainId));
        setConfirmRemove(null);
      }
    } finally {
      setRemoving((r) => ({ ...r, [domainId]: false }));
    }
  }

  if (loading || status === "loading") {
    return <div className="py-16 text-center text-sm text-zinc-400">Loading…</div>;
  }

  return (
    <div className="max-w-2xl space-y-8">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Domains</h1>
        <p className="mt-1 text-sm text-zinc-500">
          Custom domains for your short links. Add a domain, then add a CNAME DNS record pointing to
          your shortener instance and click Verify.
        </p>
      </div>

      {error && <p className="text-sm text-red-600">{error}</p>}

      {/* Add domain form */}
      <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 p-6 space-y-3">
        <h2 className="text-sm font-semibold">Add domain</h2>
        <div className="flex gap-2">
          <input
            type="text"
            placeholder="go.example.com"
            value={addHost}
            onChange={(e) => setAddHost(e.target.value)}
            onKeyDown={(e) => e.key === "Enter" && handleAdd()}
            className={inputCls + " flex-1"}
          />
          <button
            onClick={handleAdd}
            disabled={adding || !addHost.trim()}
            className="rounded-lg bg-zinc-900 dark:bg-white px-4 py-2 text-xs font-medium
              text-white dark:text-zinc-900 hover:bg-zinc-700 dark:hover:bg-zinc-200
              disabled:opacity-40 transition-colors"
          >
            {adding ? "Adding…" : "Add"}
          </button>
        </div>
        {addError && <p className="text-xs text-red-600">{addError}</p>}
      </div>

      {/* Domain list */}
      {domains.length === 0 ? (
        <p className="text-sm text-zinc-400">No domains yet.</p>
      ) : (
        <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-900/50">
                <th className="px-4 py-3 text-left text-xs font-medium text-zinc-500">Host</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-zinc-500">Status</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-zinc-500">Added</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y divide-zinc-100 dark:divide-zinc-800">
              {domains.map((domain) => (
                <tr key={domain.id} className="hover:bg-zinc-50 dark:hover:bg-zinc-900/30 transition-colors">
                  <td className="px-4 py-3 font-mono font-medium">{domain.host}</td>
                  <td className="px-4 py-3">
                    {domain.isVerified ? (
                      <span className="inline-flex items-center gap-1 rounded-full bg-green-100 dark:bg-green-950 px-2 py-0.5 text-xs font-medium text-green-700 dark:text-green-400">
                        ✓ Verified
                      </span>
                    ) : (
                      <span className="inline-flex items-center gap-1 rounded-full bg-amber-100 dark:bg-amber-950 px-2 py-0.5 text-xs font-medium text-amber-700 dark:text-amber-400">
                        Pending
                      </span>
                    )}
                  </td>
                  <td className="px-4 py-3 text-xs text-zinc-400">
                    {new Date(domain.createdAtUtc).toLocaleDateString()}
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex justify-end items-center gap-2">
                      {verifyResult[domain.id] && (
                        <span className={`text-xs ${verifyResult[domain.id] === "Verified!" ? "text-green-600" : "text-red-600"}`}>
                          {verifyResult[domain.id]}
                        </span>
                      )}
                      {!domain.isVerified && (
                        <button
                          onClick={() => handleVerify(domain.id)}
                          disabled={verifying[domain.id]}
                          className="rounded-lg border border-zinc-200 dark:border-zinc-700 px-3 py-1 text-xs
                            font-medium hover:bg-zinc-100 dark:hover:bg-zinc-800 disabled:opacity-40 transition-colors"
                        >
                          {verifying[domain.id] ? "Checking…" : "Verify"}
                        </button>
                      )}
                      {confirmRemove === domain.id ? (
                        <div className="flex gap-1">
                          <button
                            onClick={() => handleRemove(domain.id)}
                            disabled={removing[domain.id]}
                            className="rounded-lg bg-red-600 px-3 py-1 text-xs font-medium text-white
                              hover:bg-red-700 disabled:opacity-50 transition-colors"
                          >
                            {removing[domain.id] ? "Removing…" : "Confirm"}
                          </button>
                          <button
                            onClick={() => setConfirmRemove(null)}
                            className="rounded-lg border border-zinc-200 dark:border-zinc-700 px-3 py-1
                              text-xs font-medium hover:bg-zinc-100 dark:hover:bg-zinc-800 transition-colors"
                          >
                            Cancel
                          </button>
                        </div>
                      ) : (
                        <button
                          onClick={() => setConfirmRemove(domain.id)}
                          className="rounded-lg border border-red-200 dark:border-red-900 px-3 py-1 text-xs
                            font-medium text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-950 transition-colors"
                        >
                          Remove
                        </button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 p-4 text-xs text-zinc-500 space-y-1">
        <p className="font-medium text-zinc-700 dark:text-zinc-300">DNS setup</p>
        <p>Add a CNAME record for your subdomain pointing to your shortener hostname, then click Verify.</p>
        <p className="font-mono bg-zinc-100 dark:bg-zinc-900 rounded px-2 py-1 inline-block mt-1">
          go.example.com. CNAME shortener.yourdomain.com.
        </p>
      </div>
    </div>
  );
}

const inputCls =
  "rounded-lg border border-zinc-200 dark:border-zinc-700 bg-white dark:bg-zinc-900 " +
  "px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-zinc-400 dark:focus:ring-zinc-500";

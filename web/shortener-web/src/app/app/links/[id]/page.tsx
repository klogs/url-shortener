"use client";

import { useEffect, useState, use } from "react";
import { useSession } from "next-auth/react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import {
  getLink,
  updateLink,
  deleteLink,
  listVariants,
  createVariant,
  deleteVariant,
  listGeoRoutes,
  createGeoRoute,
  deleteGeoRoute,
  getLinkAnalytics,
  type LinkDetail,
  type VariantDto,
  type GeoRouteDto,
  type LinkAnalytics,
} from "@/lib/api";

const BASE = process.env.NEXT_PUBLIC_API_URL ?? "";

interface PageParams {
  id: string;
}

export default function LinkDetailPage({ params }: { params: Promise<PageParams> }) {
  const { id } = use(params);
  const { data: session, status } = useSession();
  const router = useRouter();

  const [link, setLink] = useState<LinkDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [notFound, setNotFound] = useState(false);

  // Edit form state
  const [editing, setEditing] = useState(false);
  const [destUrl, setDestUrl] = useState("");
  const [title, setTitle] = useState("");
  const [expiresAt, setExpiresAt] = useState("");
  const [saving, setSaving] = useState(false);
  const [saveError, setSaveError] = useState<string | null>(null);

  // Delete state
  const [confirmDelete, setConfirmDelete] = useState(false);
  const [deleting, setDeleting] = useState(false);

  // QR state
  const [qrUrl, setQrUrl] = useState<string | null>(null);
  const [qrLoading, setQrLoading] = useState(false);

  // Variants state
  const [variants, setVariants] = useState<VariantDto[]>([]);
  const [newVariantLabel, setNewVariantLabel] = useState("");
  const [newVariantUrl, setNewVariantUrl] = useState("");
  const [newVariantWeight, setNewVariantWeight] = useState(1);
  const [variantError, setVariantError] = useState<string | null>(null);

  // Geo routes state
  const [geoRoutes, setGeoRoutes] = useState<GeoRouteDto[]>([]);
  const [newGeoCode, setNewGeoCode] = useState("");
  const [newGeoUrl, setNewGeoUrl] = useState("");
  const [geoError, setGeoError] = useState<string | null>(null);

  // Analytics state
  const [analytics, setAnalytics] = useState<LinkAnalytics | null>(null);

  const token = session?.accessToken;

  useEffect(() => {
    if (status === "loading" || !token) return;

    Promise.all([
      getLink(token, id),
      listVariants(token, id).catch(() => [] as VariantDto[]),
      listGeoRoutes(token, id).catch(() => [] as GeoRouteDto[]),
      getLinkAnalytics(token, id, 30).catch(() => null),
    ]).then(([linkData, variantData, geoData, analyticsData]) => {
      if (!linkData) {
        setNotFound(true);
      } else {
        setLink(linkData);
        setDestUrl(linkData.destinationUrl);
        setTitle(linkData.title ?? "");
        setExpiresAt(
          linkData.expiresAt
            ? new Date(linkData.expiresAt).toISOString().slice(0, 16)
            : ""
        );
        setVariants(variantData);
        setGeoRoutes(geoData);
        setAnalytics(analyticsData);
      }
    }).finally(() => setLoading(false));
  }, [id, token, status]);

  // Load QR code on demand
  async function loadQr() {
    if (!token || qrUrl) return;
    setQrLoading(true);
    try {
      const res = await fetch(`${BASE}/api/v1/links/${id}/qr?format=svg&size=8`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (res.ok) {
        const blob = await res.blob();
        setQrUrl(URL.createObjectURL(blob));
      }
    } finally {
      setQrLoading(false);
    }
  }

  async function handleSave() {
    if (!token || !link) return;
    setSaving(true);
    setSaveError(null);
    try {
      await updateLink(token, id, {
        destinationUrl: destUrl,
        title: title || undefined,
        expiresAt: expiresAt ? new Date(expiresAt).toISOString() : null,
        redirectType: link.redirectType,
      });
      setLink({ ...link, destinationUrl: destUrl, title: title || null, expiresAt: expiresAt ? expiresAt : null });
      setEditing(false);
    } catch (err: unknown) {
      setSaveError((err as { message?: string }).message ?? "Save failed.");
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete() {
    if (!token) return;
    setDeleting(true);
    try {
      await deleteLink(token, id);
      router.push("/app/links");
    } catch {
      setDeleting(false);
      setConfirmDelete(false);
    }
  }

  async function handleAddVariant() {
    if (!token || !newVariantLabel || !newVariantUrl) return;
    setVariantError(null);
    try {
      const v = await createVariant(token, id, {
        label: newVariantLabel,
        destinationUrl: newVariantUrl,
        weight: newVariantWeight,
      });
      setVariants([...variants, v]);
      setNewVariantLabel("");
      setNewVariantUrl("");
      setNewVariantWeight(1);
    } catch (err: unknown) {
      setVariantError((err as { message?: string }).message ?? "Failed to add variant.");
    }
  }

  async function handleDeleteVariant(variantId: string) {
    if (!token) return;
    await deleteVariant(token, id, variantId).catch(() => null);
    setVariants(variants.filter((v) => v.id !== variantId));
  }

  async function handleAddGeoRoute() {
    if (!token || !newGeoCode || !newGeoUrl) return;
    setGeoError(null);
    try {
      const r = await createGeoRoute(token, id, {
        countryCode: newGeoCode.toUpperCase(),
        destinationUrl: newGeoUrl,
      });
      setGeoRoutes([...geoRoutes, r]);
      setNewGeoCode("");
      setNewGeoUrl("");
    } catch (err: unknown) {
      setGeoError((err as { message?: string }).message ?? "Failed to add geo route.");
    }
  }

  async function handleDeleteGeoRoute(routeId: string) {
    if (!token) return;
    await deleteGeoRoute(token, id, routeId).catch(() => null);
    setGeoRoutes(geoRoutes.filter((r) => r.id !== routeId));
  }

  if (loading || status === "loading") {
    return <div className="py-16 text-center text-sm text-zinc-400">Loading…</div>;
  }

  if (notFound || !link) {
    return (
      <div className="max-w-2xl space-y-4">
        <h1 className="text-2xl font-semibold">Link not found</h1>
        <Link href="/app/links" className="text-sm text-zinc-500 hover:underline">
          ← Back to links
        </Link>
      </div>
    );
  }

  return (
    <div className="max-w-2xl space-y-8">
      {/* Header */}
      <div className="flex items-start justify-between gap-4">
        <div>
          <Link href="/app/links" className="text-xs text-zinc-500 hover:underline">
            ← Links
          </Link>
          <h1 className="mt-1 text-2xl font-semibold tracking-tight font-mono">
            {link.shortCode}
          </h1>
          <div className="mt-1 flex items-center gap-2">
            <StatusBadge status={link.status} />
            {link.isAbTest && <Chip color="purple">A/B</Chip>}
            {link.hasGeoRoutes && <Chip color="blue">Geo</Chip>}
            <span className="text-xs text-zinc-400">
              {link.clickCountSnapshot.toLocaleString()} clicks
            </span>
          </div>
        </div>

        <div className="flex gap-2 shrink-0">
          <button
            onClick={() => { loadQr(); }}
            className="rounded-lg border border-zinc-200 dark:border-zinc-700 px-3 py-1.5 text-xs
              font-medium hover:bg-zinc-50 dark:hover:bg-zinc-800 transition-colors"
          >
            QR
          </button>
          {!confirmDelete ? (
            <button
              onClick={() => setConfirmDelete(true)}
              className="rounded-lg border border-red-200 dark:border-red-900 px-3 py-1.5 text-xs
                font-medium text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-950 transition-colors"
            >
              Delete
            </button>
          ) : (
            <div className="flex gap-1">
              <button
                onClick={handleDelete}
                disabled={deleting}
                className="rounded-lg bg-red-600 px-3 py-1.5 text-xs font-medium text-white
                  hover:bg-red-700 disabled:opacity-50 transition-colors"
              >
                {deleting ? "Deleting…" : "Confirm"}
              </button>
              <button
                onClick={() => setConfirmDelete(false)}
                className="rounded-lg border border-zinc-200 dark:border-zinc-700 px-3 py-1.5 text-xs
                  font-medium hover:bg-zinc-50 dark:hover:bg-zinc-800 transition-colors"
              >
                Cancel
              </button>
            </div>
          )}
        </div>
      </div>

      {/* QR code */}
      {(qrUrl || qrLoading) && (
        <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 p-6">
          <h2 className="text-sm font-semibold mb-4">QR Code</h2>
          {qrLoading ? (
            <p className="text-sm text-zinc-400">Generating…</p>
          ) : qrUrl ? (
            <div className="flex items-center gap-6">
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img src={qrUrl} alt="QR code" className="h-32 w-32 dark:invert" />
              <a
                href={qrUrl}
                download={`qr-${link.shortCode}.svg`}
                className="text-xs font-medium underline text-zinc-500 hover:text-zinc-900 dark:hover:text-white"
              >
                Download SVG
              </a>
            </div>
          ) : null}
        </div>
      )}

      {/* Edit form */}
      <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 p-6 space-y-4">
        <div className="flex items-center justify-between">
          <h2 className="text-sm font-semibold">Link settings</h2>
          {!editing && (
            <button
              onClick={() => setEditing(true)}
              className="text-xs font-medium text-zinc-500 hover:text-zinc-900 dark:hover:text-white transition-colors"
            >
              Edit
            </button>
          )}
        </div>

        {editing ? (
          <div className="space-y-3">
            <Field label="Destination URL">
              <input
                type="url"
                value={destUrl}
                onChange={(e) => setDestUrl(e.target.value)}
                className={inputCls}
              />
            </Field>
            <Field label="Title">
              <input
                type="text"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                placeholder="Optional"
                className={inputCls}
              />
            </Field>
            <Field label="Expires at">
              <input
                type="datetime-local"
                value={expiresAt}
                onChange={(e) => setExpiresAt(e.target.value)}
                className={inputCls}
              />
            </Field>
            {saveError && <p className="text-xs text-red-600">{saveError}</p>}
            <div className="flex gap-2 pt-1">
              <button
                onClick={handleSave}
                disabled={saving}
                className="rounded-lg bg-zinc-900 dark:bg-white px-4 py-2 text-xs font-medium
                  text-white dark:text-zinc-900 hover:bg-zinc-700 dark:hover:bg-zinc-200
                  disabled:opacity-50 transition-colors"
              >
                {saving ? "Saving…" : "Save"}
              </button>
              <button
                onClick={() => { setEditing(false); setSaveError(null); }}
                className="rounded-lg border border-zinc-200 dark:border-zinc-700 px-4 py-2
                  text-xs font-medium hover:bg-zinc-50 dark:hover:bg-zinc-800 transition-colors"
              >
                Cancel
              </button>
            </div>
          </div>
        ) : (
          <dl className="space-y-2 text-sm">
            <Row label="Destination" value={link.destinationUrl} mono={false} href={link.destinationUrl} />
            {link.title && <Row label="Title" value={link.title} />}
            <Row label="Redirect type" value={String(link.redirectType)} />
            <Row
              label="Expires"
              value={link.expiresAt ? new Date(link.expiresAt).toLocaleString() : "Never"}
            />
            <Row label="Created" value={new Date(link.createdAtUtc).toLocaleString()} />
          </dl>
        )}
      </div>

      {/* Analytics sparkline */}
      {analytics && (
        <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 p-6 space-y-4">
          <div className="flex items-center justify-between">
            <h2 className="text-sm font-semibold">Clicks (last {analytics.days} days)</h2>
            <span className="text-sm font-semibold tabular-nums">
              {analytics.total.toLocaleString()}
            </span>
          </div>
          <Sparkline series={analytics.series} />
        </div>
      )}

      {/* A/B variants */}
      <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 p-6 space-y-4">
        <h2 className="text-sm font-semibold">
          A/B Variants{" "}
          <span className="font-normal text-zinc-400">({variants.length})</span>
        </h2>

        {variants.length > 0 && (
          <table className="w-full text-xs">
            <thead>
              <tr className="text-left text-zinc-500">
                <th className="pb-2 font-medium">Label</th>
                <th className="pb-2 font-medium">URL</th>
                <th className="pb-2 font-medium text-right">Weight</th>
                <th className="pb-2" />
              </tr>
            </thead>
            <tbody className="divide-y divide-zinc-100 dark:divide-zinc-800">
              {variants.map((v) => (
                <tr key={v.id}>
                  <td className="py-2 font-medium">{v.label}</td>
                  <td className="py-2 max-w-xs truncate text-zinc-500">{v.destinationUrl}</td>
                  <td className="py-2 text-right">{v.weight}</td>
                  <td className="py-2 text-right">
                    <button
                      onClick={() => handleDeleteVariant(v.id)}
                      className="text-red-500 hover:text-red-700 text-xs"
                    >
                      Remove
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}

        <div className="space-y-2 pt-2 border-t border-zinc-100 dark:border-zinc-800">
          <p className="text-xs font-medium text-zinc-500">Add variant</p>
          <div className="grid grid-cols-3 gap-2">
            <input
              placeholder="Label"
              value={newVariantLabel}
              onChange={(e) => setNewVariantLabel(e.target.value)}
              className={inputCls}
            />
            <input
              placeholder="Destination URL"
              value={newVariantUrl}
              onChange={(e) => setNewVariantUrl(e.target.value)}
              className={inputCls}
            />
            <div className="flex gap-2">
              <input
                type="number"
                min={1}
                max={100}
                placeholder="Weight"
                value={newVariantWeight}
                onChange={(e) => setNewVariantWeight(Number(e.target.value))}
                className={`${inputCls} w-20`}
              />
              <button
                onClick={handleAddVariant}
                disabled={!newVariantLabel || !newVariantUrl}
                className="rounded-lg bg-zinc-900 dark:bg-white px-3 py-1.5 text-xs font-medium
                  text-white dark:text-zinc-900 disabled:opacity-40 hover:bg-zinc-700 dark:hover:bg-zinc-200 transition-colors"
              >
                Add
              </button>
            </div>
          </div>
          {variantError && <p className="text-xs text-red-600">{variantError}</p>}
        </div>
      </div>

      {/* Geo routes */}
      <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 p-6 space-y-4">
        <h2 className="text-sm font-semibold">
          Geo Routes{" "}
          <span className="font-normal text-zinc-400">({geoRoutes.length})</span>
        </h2>

        {geoRoutes.length > 0 && (
          <table className="w-full text-xs">
            <thead>
              <tr className="text-left text-zinc-500">
                <th className="pb-2 font-medium">Country</th>
                <th className="pb-2 font-medium">URL</th>
                <th className="pb-2" />
              </tr>
            </thead>
            <tbody className="divide-y divide-zinc-100 dark:divide-zinc-800">
              {geoRoutes.map((r) => (
                <tr key={r.id}>
                  <td className="py-2 font-mono font-medium">{r.countryCode}</td>
                  <td className="py-2 max-w-xs truncate text-zinc-500">{r.destinationUrl}</td>
                  <td className="py-2 text-right">
                    <button
                      onClick={() => handleDeleteGeoRoute(r.id)}
                      className="text-red-500 hover:text-red-700 text-xs"
                    >
                      Remove
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}

        <div className="space-y-2 pt-2 border-t border-zinc-100 dark:border-zinc-800">
          <p className="text-xs font-medium text-zinc-500">Add geo route</p>
          <div className="flex gap-2">
            <input
              placeholder="US"
              maxLength={2}
              value={newGeoCode}
              onChange={(e) => setNewGeoCode(e.target.value.toUpperCase())}
              className={`${inputCls} w-16 uppercase font-mono`}
            />
            <input
              placeholder="Destination URL"
              value={newGeoUrl}
              onChange={(e) => setNewGeoUrl(e.target.value)}
              className={`${inputCls} flex-1`}
            />
            <button
              onClick={handleAddGeoRoute}
              disabled={newGeoCode.length !== 2 || !newGeoUrl}
              className="rounded-lg bg-zinc-900 dark:bg-white px-3 py-1.5 text-xs font-medium
                text-white dark:text-zinc-900 disabled:opacity-40 hover:bg-zinc-700 dark:hover:bg-zinc-200 transition-colors"
            >
              Add
            </button>
          </div>
          {geoError && <p className="text-xs text-red-600">{geoError}</p>}
        </div>
      </div>
    </div>
  );
}

// ── Helpers ───────────────────────────────────────────────────────────────────

const inputCls =
  "rounded-lg border border-zinc-200 dark:border-zinc-700 bg-white dark:bg-zinc-900 " +
  "px-3 py-1.5 text-sm w-full focus:outline-none focus:ring-2 focus:ring-zinc-400 dark:focus:ring-zinc-500";

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div>
      <label className="mb-1 block text-xs font-medium text-zinc-500">{label}</label>
      {children}
    </div>
  );
}

function Row({
  label,
  value,
  mono = true,
  href,
}: {
  label: string;
  value: string;
  mono?: boolean;
  href?: string;
}) {
  return (
    <div className="flex gap-4">
      <dt className="w-28 shrink-0 text-zinc-400">{label}</dt>
      <dd className={`min-w-0 truncate ${mono ? "font-mono" : ""}`}>
        {href ? (
          <a
            href={href}
            target="_blank"
            rel="noopener noreferrer"
            className="hover:underline text-zinc-700 dark:text-zinc-300"
          >
            {value}
          </a>
        ) : (
          <span className="text-zinc-700 dark:text-zinc-300">{value}</span>
        )}
      </dd>
    </div>
  );
}

function StatusBadge({ status }: { status: string }) {
  const styles: Record<string, string> = {
    Active: "bg-green-100 text-green-700 dark:bg-green-950 dark:text-green-400",
    Disabled: "bg-zinc-100 text-zinc-600 dark:bg-zinc-800 dark:text-zinc-400",
    Expired: "bg-amber-100 text-amber-700 dark:bg-amber-950 dark:text-amber-400",
    Blocked: "bg-red-100 text-red-700 dark:bg-red-950 dark:text-red-400",
    Deleted: "bg-zinc-100 text-zinc-400 dark:bg-zinc-800 dark:text-zinc-600",
  };
  return (
    <span
      className={`inline-block rounded-full px-2 py-0.5 text-xs font-medium ${styles[status] ?? styles.Disabled}`}
    >
      {status}
    </span>
  );
}

function Chip({ color, children }: { color: "purple" | "blue"; children: React.ReactNode }) {
  const cls =
    color === "purple"
      ? "bg-purple-100 text-purple-700 dark:bg-purple-950 dark:text-purple-400"
      : "bg-blue-100 text-blue-700 dark:bg-blue-950 dark:text-blue-400";
  return (
    <span className={`inline-block rounded-full px-2 py-0.5 text-xs font-medium ${cls}`}>
      {children}
    </span>
  );
}

function Sparkline({ series }: { series: { date: string; count: number }[] }) {
  const W = 480;
  const H = 60;
  const PAD = 4;

  if (series.length === 0) {
    return <p className="text-xs text-zinc-400">No click data yet.</p>;
  }

  const max = Math.max(...series.map((p) => p.count), 1);
  const xs = series.map((_, i) => PAD + (i / Math.max(series.length - 1, 1)) * (W - PAD * 2));
  const ys = series.map((p) => PAD + (1 - p.count / max) * (H - PAD * 2));

  const line = xs.map((x, i) => `${i === 0 ? "M" : "L"}${x.toFixed(1)},${ys[i].toFixed(1)}`).join(" ");
  const area =
    `${line} L${xs[xs.length - 1].toFixed(1)},${(H - PAD).toFixed(1)} L${xs[0].toFixed(1)},${(H - PAD).toFixed(1)} Z`;

  const labelEvery = Math.ceil(series.length / 5);
  const labels = series
    .map((p, i) => ({ x: xs[i], label: p.date.slice(5), i }))
    .filter((_, i) => i % labelEvery === 0 || i === series.length - 1);

  return (
    <svg
      viewBox={`0 0 ${W} ${H + 16}`}
      className="w-full"
      aria-label="Clicks sparkline"
    >
      <defs>
        <linearGradient id="spark-fill" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stopColor="currentColor" stopOpacity="0.15" />
          <stop offset="100%" stopColor="currentColor" stopOpacity="0" />
        </linearGradient>
      </defs>
      <path d={area} fill="url(#spark-fill)" className="text-zinc-700 dark:text-zinc-300" />
      <path
        d={line}
        fill="none"
        stroke="currentColor"
        strokeWidth="1.5"
        strokeLinejoin="round"
        strokeLinecap="round"
        className="text-zinc-700 dark:text-zinc-300"
      />
      {labels.map(({ x, label }) => (
        <text
          key={label}
          x={x}
          y={H + 12}
          textAnchor="middle"
          fontSize="9"
          className="fill-zinc-400"
        >
          {label}
        </text>
      ))}
      {series.map((p, i) => (
        <title key={i}>{`${p.date}: ${p.count}`}</title>
      ))}
    </svg>
  );
}

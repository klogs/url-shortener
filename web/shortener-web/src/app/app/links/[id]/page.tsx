"use client";

import { useEffect, useRef, useState, use } from "react";
import { useSession } from "next-auth/react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import * as echarts from "echarts/core";
import { LineChart, BarChart, PieChart } from "echarts/charts";
import {
  GridComponent, TooltipComponent, TitleComponent, LegendComponent,
} from "echarts/components";
import { CanvasRenderer } from "echarts/renderers";
import { useTheme } from "@/components/ThemeProvider";
import { BrowserIcon, BROWSER_COLORS } from "@/components/BrowserIcon";
import { WorldMapChart, CountryList, useCountryNames } from "@/components/charts/WorldMapChart";
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

echarts.use([LineChart, BarChart, PieChart, GridComponent, TooltipComponent, TitleComponent, LegendComponent, CanvasRenderer]);

const BASE = process.env.BACKEND_API_URL ?? "";

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

      {/* Analytics */}
      {analytics && (
        <div className="space-y-4">
          <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 p-6 space-y-4">
            <div className="flex items-center justify-between">
              <h2 className="text-sm font-semibold">Clicks — last {analytics.days} days</h2>
              <span className="text-sm font-semibold tabular-nums">
                {analytics.total.toLocaleString()} total
              </span>
            </div>
            <ClicksLineChart series={analytics.series} />
          </div>

          {analytics.countries.length > 0 && (
            <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 p-6 space-y-4">
              <h3 className="text-xs font-semibold text-zinc-600 dark:text-zinc-400 uppercase tracking-wide">
                Countries
              </h3>
              <CountriesCard items={analytics.countries} />
            </div>
          )}

          {analytics.browsers.length > 0 && (
            <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 p-6">
              <h3 className="text-xs font-semibold mb-3 text-zinc-600 dark:text-zinc-400 uppercase tracking-wide">
                Browsers
              </h3>
              <BrowserBreakdown items={analytics.browsers} />
            </div>
          )}
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
          <table className="w-full text-xs table-fixed">
            <colgroup>
              <col className="w-14" />
              <col />
              <col className="w-16" />
            </colgroup>
            <thead>
              <tr className="text-left text-zinc-500">
                <th className="pb-2 font-medium">CC</th>
                <th className="pb-2 font-medium">URL</th>
                <th className="pb-2" />
              </tr>
            </thead>
            <tbody className="divide-y divide-zinc-100 dark:divide-zinc-800">
              {geoRoutes.map((r) => (
                <tr key={r.id}>
                  <td className="py-2 font-mono font-medium">{r.countryCode}</td>
                  <td className="py-2 truncate text-zinc-500">{r.destinationUrl}</td>
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
              className="w-12 shrink-0 rounded-lg border border-zinc-200 dark:border-zinc-700
                bg-white dark:bg-zinc-900 px-2 py-1.5 text-sm uppercase font-mono text-center
                focus:outline-none focus:ring-2 focus:ring-zinc-400 dark:focus:ring-zinc-500"
            />
            <input
              placeholder="https://example.com/destination"
              value={newGeoUrl}
              onChange={(e) => setNewGeoUrl(e.target.value)}
              className="flex-1 min-w-0 rounded-lg border border-zinc-200 dark:border-zinc-700
                bg-white dark:bg-zinc-900 px-3 py-1.5 text-sm
                focus:outline-none focus:ring-2 focus:ring-zinc-400 dark:focus:ring-zinc-500"
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

function ClicksLineChart({ series }: { series: { date: string; count: number }[] }) {
  const ref = useRef<HTMLDivElement>(null);
  const { theme } = useTheme();

  useEffect(() => {
    if (!ref.current || series.length === 0) { return; }
    const dark = theme === "dark";
    const axis = dark ? "#9a9aa8" : "#63636e";
    const grid = dark ? "rgba(255,255,255,0.07)" : "#eceef4";

    const chart = echarts.init(ref.current, null, { renderer: "canvas" });
    chart.setOption({
      backgroundColor: "transparent",
      grid: { top: 10, right: 10, bottom: 24, left: 36 },
      xAxis: {
        type: "category",
        data: series.map((p) => p.date.slice(5)),
        axisLine: { show: false },
        axisTick: { show: false },
        axisLabel: { fontSize: 10, color: axis },
      },
      yAxis: {
        type: "value",
        minInterval: 1,
        splitLine: { lineStyle: { color: grid } },
        axisLabel: { fontSize: 10, color: axis },
      },
      tooltip: {
        trigger: "axis",
        backgroundColor: dark ? "#1b1b2b" : "#ffffff",
        borderColor: dark ? "rgba(255,255,255,0.12)" : "rgba(16,16,28,0.12)",
        textStyle: { color: dark ? "#f0f0f8" : "#10101c", fontSize: 12 },
        formatter: (p: unknown) => {
          const params = p as { name: string; value: number }[];
          return `${params[0].name}: <b>${params[0].value}</b>`;
        },
      },
      series: [{
        type: "line",
        data: series.map((p) => p.count),
        smooth: true,
        lineStyle: { width: 2, color: "#0d4dff" },
        itemStyle: { color: "#0d4dff" },
        areaStyle: { color: { type: "linear", x: 0, y: 0, x2: 0, y2: 1,
          colorStops: [{ offset: 0, color: "rgba(13,77,255,0.28)" }, { offset: 1, color: "rgba(32,210,255,0)" }] } },
        symbol: "circle",
        symbolSize: series.length > 20 ? 0 : 5,
      }],
    });
    const ro = new ResizeObserver(() => chart.resize());
    ro.observe(ref.current);
    return () => { chart.dispose(); ro.disconnect(); };
  }, [series, theme]);

  if (series.length === 0) {
    return <p className="text-xs text-muted">No click data yet.</p>;
  }
  return <div ref={ref} style={{ width: "100%", height: 160 }} />;
}

/** World choropleth plus a ranked list, so exact figures stay readable. */
function CountriesCard({ items }: { items: { label: string; count: number }[] }) {
  const names = useCountryNames();
  return (
    <div className="space-y-4">
      <WorldMapChart items={items} height={280} />
      <div className="border-t border-zinc-100 dark:border-zinc-800 pt-3">
        <CountryList items={items} names={names ?? undefined} />
      </div>
    </div>
  );
}

function BrowserBreakdown({ items }: { items: { label: string; count: number }[] }) {
  const total = items.reduce((s, i) => s + i.count, 0);
  return (
    <ul className="space-y-2.5">
      {items.slice(0, 8).map((item) => {
        const pct = total > 0 ? Math.round((item.count / total) * 100) : 0;
        const color = BROWSER_COLORS[item.label] ?? "#0d4dff";
        return (
          <li key={item.label} className="space-y-1">
            <div className="flex items-center justify-between text-xs">
              <span className="flex items-center gap-2 font-medium">
                <BrowserIcon name={item.label} className="h-4 w-4" />
                {item.label}
              </span>
              <span className="tabular-nums text-muted">
                {item.count.toLocaleString()} ({pct}%)
              </span>
            </div>
            <div className="h-1.5 w-full rounded-full bg-zinc-100 dark:bg-white/8 overflow-hidden">
              <div
                className="h-full rounded-full transition-[width] duration-500"
                style={{ width: `${pct}%`, backgroundColor: color }}
              />
            </div>
          </li>
        );
      })}
    </ul>
  );
}

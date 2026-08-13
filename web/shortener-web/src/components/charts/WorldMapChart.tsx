"use client";

import { useEffect, useRef, useState } from "react";
import * as echarts from "echarts/core";
import { MapChart } from "echarts/charts";
import { TooltipComponent, VisualMapComponent } from "echarts/components";
import { CanvasRenderer } from "echarts/renderers";
import { useTheme } from "@/components/ThemeProvider";

echarts.use([MapChart, TooltipComponent, VisualMapComponent, CanvasRenderer]);

const MAP_NAME = "world";

interface WorldFeature {
  properties: { iso2: string; name: string };
}

/** iso2 → English country name, built from the GeoJSON we register. */
type NameLookup = Record<string, string>;

let mapPromise: Promise<NameLookup> | null = null;

/**
 * Registers the world map with ECharts exactly once per page load.
 * The GeoJSON is Natural Earth 110m, slimmed to `iso2` + `name` (see
 * public/maps/world.json) so region names are ISO-3166 alpha-2 codes —
 * exactly what the analytics API returns.
 */
function loadWorldMap(): Promise<NameLookup> {
  mapPromise ??= fetch("/maps/world.json")
    .then((res) => {
      if (!res.ok) throw new Error(`Failed to load world map (${res.status})`);
      return res.json() as Promise<{ features: WorldFeature[] }>;
    })
    .then((geo) => {
      echarts.registerMap(MAP_NAME, geo as never);
      const lookup: NameLookup = {};
      for (const f of geo.features) {
        lookup[f.properties.iso2] = f.properties.name;
      }
      return lookup;
    })
    .catch((err) => {
      mapPromise = null; // let a remount retry
      throw err;
    });
  return mapPromise;
}

export interface CountryDatum {
  label: string; // ISO alpha-2, or "Unknown"
  count: number;
}

/** iso2 → country name, for labelling outside the chart. Null until loaded. */
export function useCountryNames(): NameLookup | null {
  const [names, setNames] = useState<NameLookup | null>(null);
  useEffect(() => {
    let alive = true;
    loadWorldMap()
      .then((lookup) => alive && setNames(lookup))
      .catch(() => undefined);
    return () => {
      alive = false;
    };
  }, []);
  return names;
}

export function WorldMapChart({ items, height = 260 }: { items: CountryDatum[]; height?: number }) {
  const ref = useRef<HTMLDivElement>(null);
  const { theme } = useTheme();
  const [names, setNames] = useState<NameLookup | null>(null);
  const [failed, setFailed] = useState(false);

  useEffect(() => {
    let alive = true;
    loadWorldMap()
      .then((lookup) => alive && setNames(lookup))
      .catch(() => alive && setFailed(true));
    return () => {
      alive = false;
    };
  }, []);

  useEffect(() => {
    if (!ref.current || !names) return;

    const dark = theme === "dark";
    // "Unknown" has no shape on the map; it is surfaced in the list beside it.
    const mapped = items.filter((i) => i.label !== "Unknown" && names[i.label]);
    const max = Math.max(1, ...mapped.map((i) => i.count));

    const chart = echarts.init(ref.current, null, { renderer: "canvas" });
    chart.setOption({
      backgroundColor: "transparent",
      tooltip: {
        trigger: "item",
        backgroundColor: dark ? "#1b1b2b" : "#ffffff",
        borderColor: dark ? "rgba(255,255,255,0.12)" : "rgba(16,16,28,0.12)",
        textStyle: { color: dark ? "#f0f0f8" : "#10101c", fontSize: 12 },
        formatter: (p: unknown) => {
          const { name, value } = p as { name: string; value?: number };
          const label = names[name] ?? name;
          return Number.isFinite(value)
            ? `${label}<br/><b>${(value as number).toLocaleString()}</b> clicks`
            : `${label}<br/><span style="opacity:.6">No clicks</span>`;
        },
      },
      visualMap: {
        min: 0,
        max,
        left: 0,
        bottom: 0,
        orient: "horizontal",
        itemWidth: 10,
        itemHeight: 60,
        calculable: false,
        text: [max.toLocaleString(), "0"],
        textStyle: { color: dark ? "#9a9aa8" : "#63636e", fontSize: 10 },
        inRange: {
          color: dark ? ["#1d2b4a", "#0d4dff", "#20d2ff"] : ["#dbe6ff", "#3e6eff", "#0d4dff"],
        },
      },
      series: [
        {
          type: "map",
          map: MAP_NAME,
          // Our GeoJSON keys regions by ISO alpha-2 instead of the default `name`.
          nameProperty: "iso2",
          roam: false,
          // Crop Antarctica so the populated world fills the card.
          boundingCoords: [
            [-180, 83],
            [180, -56],
          ],
          itemStyle: {
            areaColor: dark ? "#191926" : "#eceef4",
            borderColor: dark ? "#10101c" : "#ffffff",
            borderWidth: 0.5,
          },
          emphasis: {
            label: { show: false },
            itemStyle: { areaColor: "#20d2ff" },
          },
          select: { disabled: true },
          data: mapped.map((i) => ({ name: i.label, value: i.count })),
        },
      ],
    });

    const ro = new ResizeObserver(() => chart.resize());
    ro.observe(ref.current);
    return () => {
      chart.dispose();
      ro.disconnect();
    };
  }, [items, names, theme]);

  if (failed) {
    return <p className="text-xs text-muted">Map data unavailable.</p>;
  }
  if (!names) {
    return (
      <div className="animate-pulse rounded-lg bg-surface" style={{ width: "100%", height }} />
    );
  }
  return <div ref={ref} style={{ width: "100%", height }} />;
}

/** Compact ranked list shown beneath the map, including "Unknown". */
export function CountryList({ items, names }: { items: CountryDatum[]; names?: NameLookup }) {
  const total = items.reduce((s, i) => s + i.count, 0);
  return (
    <ul className="space-y-1.5">
      {items.slice(0, 6).map((item) => (
        <li key={item.label} className="flex items-center justify-between gap-2 text-xs">
          <span className="flex min-w-0 items-center gap-2">
            <span className="font-mono text-[10px] font-semibold text-muted w-6 shrink-0">
              {item.label === "Unknown" ? "??" : item.label}
            </span>
            <span className="truncate">{names?.[item.label] ?? item.label}</span>
          </span>
          <span className="shrink-0 tabular-nums text-muted">
            {item.count.toLocaleString()}
            {total > 0 && ` (${Math.round((item.count / total) * 100)}%)`}
          </span>
        </li>
      ))}
    </ul>
  );
}

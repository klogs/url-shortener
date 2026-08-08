"use client";

import { useState } from "react";
import { createPublicLink, type CreateLinkResult } from "@/lib/api";

export default function ShortenForm() {
  const [url, setUrl] = useState("");
  const [result, setResult] = useState<CreateLinkResult | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [copied, setCopied] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setResult(null);
    setLoading(true);

    try {
      const data = await createPublicLink(url.trim());
      setResult(data);
      setUrl("");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Something went wrong.");
    } finally {
      setLoading(false);
    }
  }

  async function handleCopy() {
    if (!result) return;
    await navigator.clipboard.writeText(result.shortUrl);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  }

  return (
    <div className="rounded-xl border border-zinc-200 bg-white p-6 shadow-sm dark:border-zinc-800 dark:bg-zinc-900">
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label
            htmlFor="url"
            className="block text-sm font-medium text-zinc-700 dark:text-zinc-300 mb-1"
          >
            Destination URL
          </label>
          <input
            id="url"
            type="url"
            required
            value={url}
            onChange={(e) => setUrl(e.target.value)}
            placeholder="https://example.com/very/long/url"
            className="w-full rounded-lg border border-zinc-300 bg-white px-3 py-2 text-sm shadow-sm
              focus:outline-none focus:ring-2 focus:ring-zinc-500
              dark:border-zinc-700 dark:bg-zinc-800 dark:text-white dark:placeholder-zinc-500"
          />
        </div>

        <button
          type="submit"
          disabled={loading}
          className="w-full rounded-lg bg-zinc-900 px-4 py-2 text-sm font-medium text-white
            hover:bg-zinc-700 disabled:opacity-50 transition-colors
            dark:bg-white dark:text-zinc-900 dark:hover:bg-zinc-200"
        >
          {loading ? "Shortening…" : "Shorten"}
        </button>
      </form>

      {error && (
        <p className="mt-4 rounded-lg bg-red-50 px-4 py-3 text-sm text-red-700 dark:bg-red-950 dark:text-red-300">
          {error}
        </p>
      )}

      {result && (
        <div className="mt-4 rounded-lg border border-green-200 bg-green-50 px-4 py-3 dark:border-green-900 dark:bg-green-950">
          <p className="text-xs font-medium text-green-700 dark:text-green-400 mb-1">
            Your short link
          </p>
          <div className="flex items-center gap-2">
            <a
              href={result.shortUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="flex-1 truncate text-sm font-mono text-green-800 dark:text-green-300 hover:underline"
            >
              {result.shortUrl}
            </a>
            <button
              onClick={handleCopy}
              className="shrink-0 rounded px-2 py-1 text-xs font-medium bg-green-100 text-green-700
                hover:bg-green-200 transition-colors dark:bg-green-900 dark:text-green-300 dark:hover:bg-green-800"
            >
              {copied ? "Copied!" : "Copy"}
            </button>
          </div>
          {result.expiresAt && (
            <p className="mt-1 text-xs text-green-600 dark:text-green-500">
              Expires {new Date(result.expiresAt).toLocaleDateString()}
            </p>
          )}
        </div>
      )}
    </div>
  );
}

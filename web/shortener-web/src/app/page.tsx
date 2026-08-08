// Public landing page — anonymous URL shortening
// Phase 1: wire ShortenForm to POST /api/v1/public/links

export default function HomePage() {
  return (
    <main className="flex flex-1 flex-col items-center justify-center px-4">
      <div className="w-full max-w-lg space-y-6">
        <h1 className="text-3xl font-semibold tracking-tight text-center">
          Shorten your link
        </h1>

        {/* ShortenForm component goes here in Phase 1 */}
        <div className="rounded-xl border border-zinc-200 bg-white p-6 shadow-sm dark:border-zinc-800 dark:bg-zinc-900">
          <p className="text-sm text-zinc-500 text-center">
            URL shortener coming soon — Phase 1
          </p>
        </div>
      </div>
    </main>
  );
}

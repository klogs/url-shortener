import ShortenForm from "@/components/ShortenForm";
import { HomeNav } from "@/components/HomeNav";

export default function HomePage() {
  return (
    <>
      <HomeNav />
      <main className="flex flex-1 flex-col items-center justify-center px-4 py-20">
        <div className="w-full max-w-xl space-y-8">
          {/* Badge */}
          <div className="flex justify-center">
            <span className="inline-flex items-center gap-2 rounded-full border border-brand-600/30 bg-brand-600/10
              px-4 py-1.5 text-xs font-medium text-brand-600 dark:text-brand-300">
              <span className="h-1.5 w-1.5 rounded-full bg-brand-300" />
              Fast &amp; reliable link shortening
            </span>
          </div>

          {/* Hero */}
          <div className="text-center space-y-4">
            <h1 className="text-5xl font-bold tracking-tight leading-tight">
              Shorten your links
              <br />
              <span className="bg-gradient-to-r from-brand-600 via-brand-500 to-brand-300 bg-clip-text text-transparent">
                in seconds
              </span>
            </h1>
            <p className="text-base text-muted max-w-sm mx-auto">
              Paste any URL below to get a short link instantly — no sign-up needed.
            </p>
          </div>

          <ShortenForm />

          <p className="text-center text-xs text-muted">
            Free links expire after 7 days.{" "}
            <a href="/app" className="text-brand-500 hover:text-brand-300 transition-colors underline underline-offset-2">
              Sign in
            </a>{" "}
            for permanent links, analytics, and custom domains.
          </p>
        </div>
      </main>
    </>
  );
}

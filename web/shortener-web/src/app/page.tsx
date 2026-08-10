import ShortenForm from "@/components/ShortenForm";
import { HomeNav } from "@/components/HomeNav";

export default function HomePage() {
  return (
    <>
      <HomeNav />
      <main className="flex flex-1 flex-col items-center justify-center px-4">
        <div className="w-full max-w-lg space-y-6">
          <div className="text-center space-y-2">
            <h1 className="text-3xl font-semibold tracking-tight">
              Shorten your link
            </h1>
            <p className="text-sm text-zinc-500 dark:text-zinc-400">
              Paste a URL below to get a short link — no sign-up needed.
            </p>
          </div>

          <ShortenForm />

          <p className="text-center text-xs text-zinc-400 dark:text-zinc-600">
            Links expire after 7 days.{" "}
            <a href="/app" className="underline hover:text-zinc-600 dark:hover:text-zinc-400">
              Sign in
            </a>{" "}
            for permanent links and analytics.
          </p>
        </div>
      </main>
    </>
  );
}

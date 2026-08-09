interface Props {
  searchParams: Promise<{ callbackUrl?: string }>;
}

export default async function LoginPage({ searchParams }: Props) {
  const { callbackUrl } = await searchParams;
  const signinHref = `/api/auth/signin-klogs${callbackUrl ? `?callbackUrl=${encodeURIComponent(callbackUrl)}` : ""}`;

  return (
    <div className="min-h-screen flex items-center justify-center bg-zinc-50 dark:bg-zinc-950">
      <div className="w-full max-w-sm rounded-2xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 p-8 shadow-sm">
        <h1 className="text-xl font-semibold tracking-tight text-zinc-900 dark:text-white mb-1">
          Sign in
        </h1>
        <p className="text-sm text-zinc-500 dark:text-zinc-400 mb-8">
          Use your Klogs account to access the dashboard.
        </p>

        <a href={signinHref}>
          <button
            type="button"
            className="w-full rounded-lg bg-zinc-900 dark:bg-white px-4 py-2.5 text-sm font-medium
              text-white dark:text-zinc-900 hover:bg-zinc-700 dark:hover:bg-zinc-200 transition-colors"
          >
            Continue with Klogs
          </button>
        </a>

        <p className="mt-6 text-center text-xs text-zinc-400 dark:text-zinc-600">
          You will be redirected to your identity provider.
        </p>
      </div>
    </div>
  );
}

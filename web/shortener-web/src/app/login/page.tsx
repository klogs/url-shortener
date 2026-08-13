import { LoginBrand } from "@/components/LoginBrand";

interface Props {
  searchParams: Promise<{ callbackUrl?: string }>;
}

export default async function LoginPage({ searchParams }: Props) {
  const { callbackUrl } = await searchParams;
  const signinHref = `/api/auth/signin-klogs${callbackUrl ? `?callbackUrl=${encodeURIComponent(callbackUrl)}` : ""}`;

  return (
    <div className="min-h-screen flex items-center justify-center bg-background px-4">
      <div className="w-full max-w-sm rounded-2xl border border-edge bg-raised p-8 shadow-sm">
        <LoginBrand />

        <h1 className="text-xl font-semibold tracking-tight mb-1">Sign in</h1>
        <p className="text-sm text-muted mb-8">
          Use your Klogs account to access the dashboard.
        </p>

        <a href={signinHref}>
          <button
            type="button"
            className="w-full rounded-lg bg-brand-600 hover:bg-brand-500 px-4 py-2.5 text-sm
              font-semibold text-white transition-colors"
          >
            Continue with Klogs
          </button>
        </a>

        <p className="mt-6 text-center text-xs text-muted">
          You will be redirected to your identity provider.
        </p>
      </div>
    </div>
  );
}

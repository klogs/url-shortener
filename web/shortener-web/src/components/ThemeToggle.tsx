"use client";

import { useTheme } from "@/components/ThemeProvider";

/**
 * Sliding sun/moon switch. The knob tracks the active theme; both icons stay
 * visible so the control reads as a switch rather than a single-state button.
 */
export function ThemeToggle({ className = "" }: { className?: string }) {
  const { theme, toggleTheme } = useTheme();
  const isDark = theme === "dark";

  return (
    <button
      type="button"
      role="switch"
      aria-checked={isDark}
      aria-label={isDark ? "Switch to light theme" : "Switch to dark theme"}
      title={isDark ? "Switch to light theme" : "Switch to dark theme"}
      onClick={toggleTheme}
      className={`relative inline-flex h-7 w-13 shrink-0 items-center rounded-full border border-edge
        bg-surface transition-colors hover:border-brand-500/50
        focus:outline-none focus-visible:ring-2 focus-visible:ring-brand-600 ${className}`}
    >
      {/* Sliding knob */}
      <span
        className={`absolute top-0.5 left-0.5 h-5.5 w-5.5 rounded-full bg-gradient-to-br
          from-brand-600 to-brand-300 shadow-sm transition-transform duration-200 ease-out
          ${isDark ? "translate-x-6" : "translate-x-0"}`}
      />
      {/* Sun */}
      <span
        className={`pointer-events-none relative z-10 flex h-6 w-6 items-center justify-center transition-colors
          ${isDark ? "text-muted" : "text-white"}`}
      >
        <svg className="h-3.5 w-3.5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round">
          <circle cx="12" cy="12" r="4" />
          <path d="M12 2v2M12 20v2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M2 12h2M20 12h2M4.9 19.1l1.4-1.4M17.7 6.3l1.4-1.4" />
        </svg>
      </span>
      {/* Moon */}
      <span
        className={`pointer-events-none relative z-10 flex h-6 w-6 items-center justify-center transition-colors
          ${isDark ? "text-white" : "text-muted"}`}
      >
        <svg className="h-3.5 w-3.5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
          <path d="M21 12.8A9 9 0 1 1 11.2 3a7 7 0 0 0 9.8 9.8z" />
        </svg>
      </span>
    </button>
  );
}

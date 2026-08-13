"use client";

import Link from "next/link";
import { KlogsLogo } from "@/components/KlogsLogo";
import { ThemeToggle } from "@/components/ThemeToggle";
import { useTheme } from "@/components/ThemeProvider";

/** Sidebar header: wordmark tinted for the active theme, plus the theme switch. */
export function SidebarBrand() {
  const { theme } = useTheme();
  return (
    <div className="flex items-center justify-between gap-2 px-4 py-4 border-b border-edge">
      <Link href="/" className="flex items-center hover:opacity-80 transition-opacity">
        <KlogsLogo height={19} textFill={theme === "dark" ? "#ffffff" : "#10101C"} />
      </Link>
      <ThemeToggle />
    </div>
  );
}

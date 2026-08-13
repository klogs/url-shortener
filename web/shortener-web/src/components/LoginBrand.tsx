"use client";

import { KlogsLogo } from "@/components/KlogsLogo";
import { ThemeToggle } from "@/components/ThemeToggle";
import { useTheme } from "@/components/ThemeProvider";

export function LoginBrand() {
  const { theme } = useTheme();
  return (
    <div className="mb-8 flex items-center justify-between">
      <KlogsLogo height={22} textFill={theme === "dark" ? "#ffffff" : "#10101C"} />
      <ThemeToggle />
    </div>
  );
}

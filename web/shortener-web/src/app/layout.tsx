import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "Shortener",
  description: "High-performance, multi-tenant URL shortener",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="en" className="h-full">
      <body className="min-h-full flex flex-col">{children}</body>
    </html>
  );
}

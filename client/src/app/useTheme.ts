import { useEffect, useState } from "react";

export type Theme = "dark" | "light";

const STORAGE_KEY = "autonest_theme";

function systemTheme(): Theme {
  return typeof window.matchMedia === "function" &&
    window.matchMedia("(prefers-color-scheme: dark)").matches
    ? "dark"
    : "light";
}

function savedTheme(): Theme | null {
  const saved = localStorage.getItem(STORAGE_KEY);
  return saved === "dark" || saved === "light" ? saved : null;
}

export function useTheme() {
  const [theme, setTheme] = useState<Theme>(() => savedTheme() ?? systemTheme());

  useEffect(() => {
    document.documentElement.dataset.theme = theme;
    document.documentElement.style.colorScheme = theme;
    document
      .querySelector('meta[name="theme-color"]')
      ?.setAttribute("content", theme === "dark" ? "#090b0f" : "#f5f7fa");
  }, [theme]);

  useEffect(() => {
    if (typeof window.matchMedia !== "function") return;
    const media = window.matchMedia("(prefers-color-scheme: dark)");
    const followSystem = (event: MediaQueryListEvent) => {
      if (!savedTheme()) setTheme(event.matches ? "dark" : "light");
    };
    const syncStoredTheme = (event: StorageEvent) => {
      if (event.key === STORAGE_KEY) setTheme(savedTheme() ?? systemTheme());
    };

    media.addEventListener("change", followSystem);
    window.addEventListener("storage", syncStoredTheme);
    return () => {
      media.removeEventListener("change", followSystem);
      window.removeEventListener("storage", syncStoredTheme);
    };
  }, []);

  const toggleTheme = () => {
    setTheme((current) => {
      const next = current === "dark" ? "light" : "dark";
      localStorage.setItem(STORAGE_KEY, next);
      return next;
    });
  };

  return { theme, toggleTheme };
}

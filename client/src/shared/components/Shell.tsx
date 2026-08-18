import { Heart, LayoutDashboard, LogIn, Menu, Moon, Sun, X } from "lucide-react";
import { useEffect, useState } from "react";
import { Link, NavLink, Outlet, useLocation } from "react-router-dom";
import { useAuth } from "../../app/AuthContext";
import { useTheme } from "../../app/useTheme";
import type { Role } from "../types";

type NavItem = readonly [to: string, label: string];

const PUBLIC_LINKS: NavItem[] = [
  ["/cars", "Browse cars"],
  ["/privacy", "Privacy"],
];

const ROLE_LINKS: Record<Role, NavItem[]> = {
  Customer: [
    ["/cars", "Browse cars"],
    ["/favorites", "Favorites"],
    ["/requests", "My requests"],
    ["/profile", "Profile"],
    ["/privacy", "Privacy"],
  ],
  Company: [
    ["/dashboard", "Dashboard"],
    ["/company/cars", "Vehicles"],
    ["/company/requests", "Requests"],
    ["/privacy", "Privacy"],
  ],
  Admin: [
    ["/dashboard", "Dashboard"],
    ["/admin/users", "Users"],
    ["/admin/plans", "Plans"],
    ["/admin/points", "Points"],
    ["/privacy", "Privacy"],
  ],
};

export function Shell() {
  const [open, setOpen] = useState(false);
  const { session, signOut } = useAuth();
  const { theme, toggleTheme } = useTheme();
  const { pathname } = useLocation();

  useEffect(() => {
    setOpen(false);
  }, [pathname]);

  const links = session ? ROLE_LINKS[session.role] : PUBLIC_LINKS;

  return (
    <div className="app-shell">
      <header className="topbar">
        <div className="topbar-inner">
          <Link to="/" className="brand" aria-label="AutoNest home">
            <img className="brand-logo" src="/logo.png" alt="" />
            <span>
              AUTO<b>NEST</b>
            </span>
          </Link>
          <div className="header-actions">
            <nav
              id="primary-navigation"
              className={open ? "nav open" : "nav"}
              aria-label="Primary navigation"
            >
              {links.map(([to, label]) => (
                <NavLink key={to} to={to}>
                  {label}
                </NavLink>
              ))}
              {session ? (
                <button
                  className="nav-action"
                  onClick={() => {
                    setOpen(false);
                    signOut();
                  }}
                >
                  Sign out
                </button>
              ) : (
                <NavLink className="sign-in-link" to="/auth">
                  <LogIn size={17} /> Sign in
                </NavLink>
              )}
            </nav>
            <button
              className="icon-button theme-toggle"
              onClick={toggleTheme}
              aria-label={`Switch to ${theme === "dark" ? "light" : "dark"} theme`}
              title={`Switch to ${theme === "dark" ? "light" : "dark"} theme`}
            >
              {theme === "dark" ? <Sun size={19} /> : <Moon size={19} />}
            </button>
            <button
              className="icon-button mobile-menu"
              onClick={() => setOpen((current) => !current)}
              aria-label={open ? "Close navigation menu" : "Open navigation menu"}
              aria-expanded={open}
              aria-controls="primary-navigation"
            >
              {open ? <X /> : <Menu />}
            </button>
          </div>
        </div>
      </header>
      <main>
        <Outlet />
      </main>
      <footer>
        <div className="brand">
          <img className="brand-logo" src="/logo.png" alt="" />
          <span>AUTONEST</span>
        </div>
        <p>Exceptional cars. Clear decisions. One trusted destination.</p>
        <span>© {new Date().getFullYear()} AutoNest</span>
      </footer>
    </div>
  );
}

export const IconFor = ({ name }: { name: string }) =>
  name.includes("Dashboard") ? <LayoutDashboard /> : <Heart />;

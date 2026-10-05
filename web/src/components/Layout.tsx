/*
 * File: Layout.tsx
 * Purpose: Shell for signed-in pages - a sticky, dark, gridded navbar (icons + labels, live
 *          pending-bookings badge, account menu, mobile menu, scanner light along its base)
 *          and the animated page container.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

import { useEffect, useState } from "react";
import type { IconType } from "react-icons";
import { NavLink, Outlet, useLocation } from "react-router-dom";
import { dashboardApi } from "../api/endpoints";
import type { Role } from "../api/types";
import { useAuth } from "../auth/AuthContext";
import { UserMenu } from "./UserMenu";
import { CiBatteryCharging, CiCalendarDate, CiCircleRemove, CiClock2, CiGrid41, CiHome, CiMenuBurger, CiUser } from "react-icons/ci";

interface NavItem {
  to: string;
  label: string;
  icon: IconType;
  roles: Role[];
  showPendingBadge?: boolean;
}

const NAV: NavItem[] = [
  { to: "/dashboard", label: "Dashboard", icon: CiGrid41, roles: ["Backoffice", "GridOperator"] },
  { to: "/bookings", label: "Bookings", icon: CiCalendarDate, roles: ["Backoffice", "GridOperator"], showPendingBadge: true },
  { to: "/nodes", label: "Nodes", icon: CiBatteryCharging, roles: ["Backoffice", "GridOperator"] },
  { to: "/slots", label: "Slots", icon: CiClock2, roles: ["Backoffice", "GridOperator"] },
  { to: "/prosumers", label: "Prosumers", icon: CiHome, roles: ["Backoffice"] },
  { to: "/users", label: "Staff", icon: CiUser, roles: ["Backoffice"] },
];

export function Layout() {
  const { session } = useAuth();
  const location = useLocation();
  const [menuOpen, setMenuOpen] = useState(false);
  const [pendingCount, setPendingCount] = useState<number | null>(null);

  // Close the mobile menu whenever the page changes.
  useEffect(() => setMenuOpen(false), [location.pathname]);

  // Live count of bookings waiting for approval, refreshed on every navigation.
  useEffect(() => {
    let cancelled = false;
    dashboardApi
      .get()
      .then((counts) => !cancelled && setPendingCount(counts.pendingCount))
      .catch(() => !cancelled && setPendingCount(null));
    return () => {
      cancelled = true;
    };
  }, [location.pathname, location.search]);

  if (!session) return null;
  const links = NAV.filter((n) => n.roles.includes(session.role));

  return (
    <>
      <a className="sg-skip-link" href="#sg-main">
        Skip to content
      </a>

      <nav className="navbar navbar-expand-lg navbar-dark sg-navbar sticky-top" aria-label="Main">
        <div className="container">
          <NavLink className="sg-brand me-lg-4" to="/dashboard" aria-label="SolarGrid dashboard">
            <span className="sg-brand-logo" aria-hidden="true">
              S
            </span>
            <span>SolarGrid</span>
          </NavLink>

          <button
            className="sg-menu-toggle d-lg-none"
            type="button"
            aria-controls="sgNav"
            aria-expanded={menuOpen}
            aria-label={menuOpen ? "Close menu" : "Open menu"}
            onClick={() => setMenuOpen((open) => !open)}
          >
            {menuOpen ? <CiCircleRemove /> : <CiMenuBurger />}
          </button>

          <div className={`collapse navbar-collapse${menuOpen ? " show" : ""}`} id="sgNav">
            <ul className="navbar-nav sg-nav me-auto">
              {links.map((n) => (
                <li className="nav-item" key={n.to}>
                  <NavLink className="nav-link sg-nav-link" to={n.to} onClick={() => setMenuOpen(false)}>
                    <n.icon />
                    <span>{n.label}</span>
                    {n.showPendingBadge && pendingCount !== null && pendingCount > 0 && (
                      <span className="sg-nav-count" aria-label={`${pendingCount} pending`}>
                        {pendingCount > 99 ? "99+" : pendingCount}
                      </span>
                    )}
                  </NavLink>
                </li>
              ))}
            </ul>
            <UserMenu />
          </div>
        </div>
        <span className="sg-scanline" aria-hidden="true" />
      </nav>

      <main className="container pt-4 pb-5" id="sg-main" tabIndex={-1}>
        {/* Keyed by route so each page plays its entrance animation. */}
        <div className="sg-page" key={location.pathname}>
          <Outlet />
        </div>
      </main>
    </>
  );
}

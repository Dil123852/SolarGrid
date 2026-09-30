/*
 * File: Layout.tsx
 * Purpose: Shell for signed-in pages - the dark, gridded, role-aware navbar (with the sign-in
 *          page's scanner light along its base) and the animated page container.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

import { useState } from "react";
import { NavLink, Outlet, useLocation } from "react-router-dom";
import type { Role } from "../api/types";
import { useAuth } from "../auth/AuthContext";
import { roleLabel } from "../utils/format";

const NAV: { to: string; label: string; roles: Role[] }[] = [
  { to: "/dashboard", label: "Dashboard", roles: ["Backoffice", "GridOperator"] },
  { to: "/bookings", label: "Bookings", roles: ["Backoffice", "GridOperator"] },
  { to: "/nodes", label: "Nodes", roles: ["Backoffice", "GridOperator"] },
  { to: "/prosumers", label: "Prosumers", roles: ["Backoffice"] },
  { to: "/users", label: "Staff", roles: ["Backoffice"] },
];

export function Layout() {
  const { session, logout } = useAuth();
  const location = useLocation();
  const [menuOpen, setMenuOpen] = useState(false);
  if (!session) return null;

  const links = NAV.filter((n) => n.roles.includes(session.role));

  return (
    <>
      <nav className="navbar navbar-expand-lg navbar-dark sg-navbar mb-4">
        <div className="container">
          <NavLink className="sg-brand me-4" to="/dashboard">
            <span className="sg-brand-logo" aria-hidden="true">
              S
            </span>
            SolarGrid
          </NavLink>
          <button
            className="navbar-toggler"
            type="button"
            aria-controls="sgNav"
            aria-expanded={menuOpen}
            aria-label="Toggle navigation"
            onClick={() => setMenuOpen((open) => !open)}
          >
            <span className="navbar-toggler-icon" />
          </button>
          <div className={`collapse navbar-collapse${menuOpen ? " show" : ""}`} id="sgNav">
            <ul className="navbar-nav me-auto">
              {links.map((n) => (
                <li className="nav-item" key={n.to}>
                  <NavLink className="nav-link" to={n.to} onClick={() => setMenuOpen(false)}>
                    {n.label}
                  </NavLink>
                </li>
              ))}
            </ul>
            <span className="sg-user me-3">
              {session.displayName}
              <span className="sg-role">{roleLabel(session.role)}</span>
            </span>
            <button className="btn btn-sm btn-signout" onClick={() => logout()}>
              Sign out
            </button>
          </div>
        </div>
        <span className="sg-scanline" aria-hidden="true" />
      </nav>
      <main className="container pb-5">
        {/* Keyed by route so each page plays its entrance animation. */}
        <div className="sg-page" key={location.pathname}>
          <Outlet />
        </div>
      </main>
    </>
  );
}

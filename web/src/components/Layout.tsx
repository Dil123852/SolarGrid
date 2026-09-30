/*
 * File: Layout.tsx
 * Purpose: Shell for signed-in pages - the role-aware navbar and the page container.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

import { useState } from "react";
import { NavLink, Outlet } from "react-router-dom";
import type { Role } from "../api/types";
import { useAuth } from "../auth/AuthContext";
import { roleLabel } from "../utils/format";

const NAV: { to: string; label: string; icon: string; roles: Role[] }[] = [
  { to: "/dashboard", label: "Dashboard", icon: "bi-speedometer2", roles: ["Backoffice", "GridOperator"] },
  { to: "/bookings", label: "Bookings", icon: "bi-calendar-check", roles: ["Backoffice", "GridOperator"] },
  { to: "/nodes", label: "Microgrid Nodes", icon: "bi-lightning-charge", roles: ["Backoffice", "GridOperator"] },
  { to: "/prosumers", label: "Prosumers", icon: "bi-house-gear", roles: ["Backoffice"] },
  { to: "/users", label: "Staff Users", icon: "bi-people", roles: ["Backoffice"] },
];

export function Layout() {
  const { session, logout } = useAuth();
  const [menuOpen, setMenuOpen] = useState(false);
  if (!session) return null;

  const links = NAV.filter((n) => n.roles.includes(session.role));

  return (
    <>
      <nav className="navbar navbar-expand-lg navbar-dark sg-navbar mb-4">
        <div className="container">
          <NavLink className="navbar-brand fw-semibold" to="/dashboard">
            <i className="bi bi-sun-fill text-warning me-2" />
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
                    <i className={`bi ${n.icon} me-1`} />
                    {n.label}
                  </NavLink>
                </li>
              ))}
            </ul>
            <span className="navbar-text me-3 small">
              <i className="bi bi-person-circle me-1" />
              {session.displayName}
              <span className="badge rounded-pill text-bg-light ms-1">{roleLabel(session.role)}</span>
            </span>
            <button className="btn btn-outline-light btn-sm" onClick={() => logout()}>
              <i className="bi bi-box-arrow-right me-1" />
              Sign out
            </button>
          </div>
        </div>
      </nav>
      <main className="container pb-5">
        <Outlet />
      </main>
    </>
  );
}

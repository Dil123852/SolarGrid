/*
 * File: UserMenu.tsx
 * Purpose: Account menu in the navbar - initials avatar that opens a small panel with the
 *          signed-in user's name, role, session expiry and Sign out. Closes on outside click,
 *          Escape, or after choosing an action.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

import { useEffect, useRef, useState } from "react";
import { useAuth } from "../auth/AuthContext";
import { roleLabel } from "../utils/format";
import { CiCircleChevDown, CiClock2, CiLogout } from "react-icons/ci";

function initials(name: string): string {
  const parts = name.split(/[\s._-]+/).filter(Boolean);
  const letters = parts.length > 1 ? parts[0][0] + parts[1][0] : name.slice(0, 2);
  return letters.toUpperCase();
}

export function UserMenu() {
  const { session, logout } = useAuth();
  const [open, setOpen] = useState(false);
  const rootRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) return;
    const onPointer = (e: MouseEvent) => {
      if (rootRef.current && !rootRef.current.contains(e.target as Node)) setOpen(false);
    };
    const onKey = (e: KeyboardEvent) => e.key === "Escape" && setOpen(false);
    document.addEventListener("mousedown", onPointer);
    document.addEventListener("keydown", onKey);
    return () => {
      document.removeEventListener("mousedown", onPointer);
      document.removeEventListener("keydown", onKey);
    };
  }, [open]);

  if (!session) return null;
  const expires = new Date(session.expiresAt).toLocaleTimeString(undefined, { hour: "2-digit", minute: "2-digit" });

  return (
    <div className="sg-user-menu" ref={rootRef}>
      <button
        type="button"
        className="sg-user-trigger"
        aria-haspopup="menu"
        aria-expanded={open}
        onClick={() => setOpen((o) => !o)}
      >
        <span className="sg-avatar" aria-hidden="true">
          {initials(session.displayName)}
        </span>
        <span className="sg-user-text">
          <span className="sg-user-name">{session.displayName}</span>
          <span className="sg-user-role">{roleLabel(session.role)}</span>
        </span>
        <CiCircleChevDown className={`sg-chevron${open ? " open" : ""}`} />
      </button>

      {open && (
        <div className="sg-user-panel" role="menu">
          <div className="sg-user-panel-head">
            <span className="sg-avatar sg-avatar-lg" aria-hidden="true">
              {initials(session.displayName)}
            </span>
            <div>
              <div className="sg-user-panel-name">{session.displayName}</div>
              <div className="sg-user-panel-role">{roleLabel(session.role)}</div>
            </div>
          </div>
          <div className="sg-user-panel-meta">
            <CiClock2 className="me-2" />
            Session active until {expires}
          </div>
          <button type="button" className="sg-user-panel-action" role="menuitem" onClick={() => logout()}>
            <CiLogout className="me-2" />
            Sign out
          </button>
        </div>
      )}
    </div>
  );
}

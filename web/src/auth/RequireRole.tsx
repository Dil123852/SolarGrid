/*
 * File: RequireRole.tsx
 * Purpose: Route guard - signed-out users go to the login page; signed-in users without one
 *          of the allowed roles go back to the dashboard. The API enforces the same rules.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

import type { ReactNode } from "react";
import { Navigate, useLocation } from "react-router-dom";
import type { Role } from "../api/types";
import { useAuth } from "./AuthContext";

export function RequireRole({ roles, children }: { roles: Role[]; children: ReactNode }) {
  const { session } = useAuth();
  const location = useLocation();

  if (!session) return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  if (!roles.includes(session.role)) return <Navigate to="/dashboard" replace />;
  return <>{children}</>;
}

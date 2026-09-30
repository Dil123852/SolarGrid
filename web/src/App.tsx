/*
 * File: App.tsx
 * Purpose: Routes. HashRouter keeps URLs like /#/bookings, so static hosting on IIS needs no
 *          rewrite rules. Each route declares which staff roles may open it.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

import type { ReactNode } from "react";
import { HashRouter, Navigate, Route, Routes } from "react-router-dom";
import type { Role } from "./api/types";
import { AuthProvider } from "./auth/AuthContext";
import { RequireRole } from "./auth/RequireRole";
import { Layout } from "./components/Layout";
import { ToastProvider } from "./components/Toasts";
import { BookingsPage } from "./pages/BookingsPage";
import { DashboardPage } from "./pages/DashboardPage";
import { LoginPage } from "./pages/LoginPage";
import { NodesPage } from "./pages/NodesPage";
import { ProsumersPage } from "./pages/ProsumersPage";
import { UsersPage } from "./pages/UsersPage";

const STAFF: Role[] = ["Backoffice", "GridOperator"];
const BACKOFFICE: Role[] = ["Backoffice"];

const guard = (roles: Role[], page: ReactNode) => <RequireRole roles={roles}>{page}</RequireRole>;

export function App() {
  return (
    <HashRouter>
      <AuthProvider>
        <ToastProvider>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route element={guard(STAFF, <Layout />)}>
              <Route path="/dashboard" element={<DashboardPage />} />
              <Route path="/bookings" element={<BookingsPage />} />
              <Route path="/nodes" element={<NodesPage />} />
              <Route path="/prosumers" element={guard(BACKOFFICE, <ProsumersPage />)} />
              <Route path="/users" element={guard(BACKOFFICE, <UsersPage />)} />
            </Route>
            <Route path="*" element={<Navigate to="/dashboard" replace />} />
          </Routes>
        </ToastProvider>
      </AuthProvider>
    </HashRouter>
  );
}

/*
 * File: Badges.tsx
 * Purpose: Small presentational pieces shared by the tables - status/active badges and empty rows.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

import type { ReservationStatus } from "../api/types";

const STATUS_COLOR: Record<ReservationStatus, string> = {
  Pending: "warning",
  Approved: "success",
  Cancelled: "secondary",
  Completed: "primary",
};

export function StatusBadge({ status }: { status: ReservationStatus }) {
  return <span className={`badge text-bg-${STATUS_COLOR[status] ?? "light"}`}>{status}</span>;
}

export function ActiveBadge({ active }: { active: boolean }) {
  return active ? (
    <span className="badge text-bg-success">Active</span>
  ) : (
    <span className="badge text-bg-secondary">Inactive</span>
  );
}

export function EmptyRow({ colSpan, text }: { colSpan: number; text: string }) {
  return (
    <tr>
      <td colSpan={colSpan} className="text-center text-body-secondary py-4">
        {text}
      </td>
    </tr>
  );
}

export function LoadingRow({ colSpan }: { colSpan: number }) {
  return (
    <tr>
      <td colSpan={colSpan} className="text-center text-body-secondary py-4">
        <span className="spinner-border spinner-border-sm me-2" aria-hidden="true" />
        Loading…
      </td>
    </tr>
  );
}

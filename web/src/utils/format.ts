/*
 * File: format.ts
 * Purpose: Date and label formatting shared by the pages. The API speaks UTC ISO strings;
 *          the UI shows and edits local time.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

import type { Role } from "../api/types";

export function formatDateTime(iso: string | null | undefined): string {
  if (!iso) return "";
  return new Date(iso).toLocaleString(undefined, { dateStyle: "medium", timeStyle: "short" });
}

// Value for <input type="datetime-local"> in the browser's local time.
export function toLocalInput(value: Date | string): string {
  const d = new Date(value);
  d.setMinutes(d.getMinutes() - d.getTimezoneOffset());
  return d.toISOString().slice(0, 16);
}

// datetime-local value -> UTC ISO string for the API.
export function fromLocalInput(value: string): string {
  return new Date(value).toISOString();
}

// yyyy-mm-dd from a date input -> UTC ISO at the start / end of that local day.
export function dayStartIso(day: string): string {
  return day ? new Date(`${day}T00:00`).toISOString() : "";
}

export function dayEndIso(day: string): string {
  return day ? new Date(`${day}T23:59:59`).toISOString() : "";
}

export function roleLabel(role: Role): string {
  return role === "GridOperator" ? "Grid Operator" : role;
}

export function plural(count: number, word: string): string {
  return `${count} ${word}${count === 1 ? "" : "s"}`;
}

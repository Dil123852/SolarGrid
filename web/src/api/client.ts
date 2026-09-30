/*
 * File: client.ts
 * Purpose: The only place the web app talks HTTP. Adds the JWT bearer header, sends/parses JSON,
 *          reports expired sessions, and turns the API's { message } error bodies into ApiErrors
 *          so pages can show the server's business-rule message verbatim.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

import { apiBaseUrl } from "../config";

export class ApiError extends Error {
  readonly status: number;

  constructor(status: number, message: string) {
    super(message);
    this.name = "ApiError";
    this.status = status;
  }
}

type Query = Record<string, string | number | boolean | null | undefined>;

let tokenProvider: () => string | null = () => null;
let onUnauthorized: () => void = () => {};

// Wired once by the AuthProvider so the client never imports React state directly.
export function configureClient(options: { getToken: () => string | null; onUnauthorized: () => void }): void {
  tokenProvider = options.getToken;
  onUnauthorized = options.onUnauthorized;
}

function buildQuery(params?: Query): string {
  if (!params) return "";
  const search = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== null && value !== "") search.append(key, String(value));
  }
  const text = search.toString();
  return text ? `?${text}` : "";
}

async function request<T>(method: string, path: string, body?: unknown, params?: Query): Promise<T> {
  const token = tokenProvider();
  const headers: Record<string, string> = { Accept: "application/json" };
  if (token) headers.Authorization = `Bearer ${token}`;
  if (body !== undefined) headers["Content-Type"] = "application/json";

  let response: Response;
  try {
    response = await fetch(`${apiBaseUrl}${path}${buildQuery(params)}`, {
      method,
      headers,
      body: body !== undefined ? JSON.stringify(body) : undefined,
    });
  } catch {
    throw new ApiError(0, `Cannot reach the SolarGrid API at ${apiBaseUrl}.`);
  }

  // A protected call rejected our token: the session has expired.
  if (response.status === 401 && token) {
    onUnauthorized();
    throw new ApiError(401, "Your session has expired. Please sign in again.");
  }

  const text = await response.text();
  let data: unknown = null;
  if (text) {
    try {
      data = JSON.parse(text);
    } catch {
      data = null;
    }
  }

  if (!response.ok) {
    const serverMessage = (data as { message?: string } | null)?.message;
    const fallback =
      response.status === 403 ? "You do not have permission to do that." : `Request failed (${response.status}).`;
    throw new ApiError(response.status, serverMessage || fallback);
  }
  return data as T;
}

export const http = {
  get: <T>(path: string, params?: Query) => request<T>("GET", path, undefined, params),
  post: <T>(path: string, body: unknown = {}) => request<T>("POST", path, body),
  put: <T>(path: string, body: unknown = {}) => request<T>("PUT", path, body),
  del: <T>(path: string) => request<T>("DELETE", path),
};

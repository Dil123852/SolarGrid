/*
 * File: config.ts
 * Purpose: Base URL of the IIS-hosted SolarGrid API. Set VITE_API_BASE_URL at build time, or
 *          override in the browser: localStorage.setItem('sg.apiBaseUrl', 'http://host:port').
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

const DEFAULT_API_BASE_URL = "http://localhost:8081";

function readOverride(): string | null {
  try {
    return localStorage.getItem("sg.apiBaseUrl");
  } catch {
    return null;
  }
}

export const apiBaseUrl: string = (
  readOverride() ||
  (import.meta.env.VITE_API_BASE_URL as string | undefined) ||
  DEFAULT_API_BASE_URL
).replace(/\/+$/, "");

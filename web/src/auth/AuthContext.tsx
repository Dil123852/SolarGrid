/*
 * File: AuthContext.tsx
 * Purpose: Staff session state - the JWT, role and display name returned by /api/auth/login,
 *          mirrored to sessionStorage so a refresh keeps the user signed in. Expired sessions
 *          are dropped, and a flash message explains why on the login page.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

import { createContext, useCallback, useContext, useMemo, useRef, useState, type ReactNode } from "react";
import { configureClient } from "../api/client";
import { authApi } from "../api/endpoints";
import type { AuthResponse, Role } from "../api/types";

const SESSION_KEY = "sg.session";
const FLASH_KEY = "sg.flash";

interface AuthState {
  session: AuthResponse | null;
  login: (username: string, password: string) => Promise<AuthResponse>;
  logout: (flashMessage?: string) => void;
  hasRole: (...roles: Role[]) => boolean;
  takeFlash: () => string | null;
}

const AuthContext = createContext<AuthState | null>(null);

function readSession(): AuthResponse | null {
  try {
    const stored = JSON.parse(sessionStorage.getItem(SESSION_KEY) ?? "null") as AuthResponse | null;
    if (stored && new Date(stored.expiresAt) > new Date()) return stored;
  } catch {
    /* corrupt or unavailable storage: treat as signed out */
  }
  return null;
}

function writeStorage(key: string, value: string | null): void {
  try {
    if (value === null) sessionStorage.removeItem(key);
    else sessionStorage.setItem(key, value);
  } catch {
    /* private mode: memory only */
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<AuthResponse | null>(readSession);
  const sessionRef = useRef(session);
  sessionRef.current = session;

  const logout = useCallback((flashMessage?: string) => {
    writeStorage(SESSION_KEY, null);
    if (flashMessage) writeStorage(FLASH_KEY, flashMessage);
    setSession(null);
  }, []);

  // The HTTP client reads the token lazily and reports expired sessions back here. Configured
  // during render (not in an effect) because child pages' effects run first and fetch immediately.
  configureClient({
    getToken: () => sessionRef.current?.token ?? null,
    onUnauthorized: () => logout("Your session has expired. Please sign in again."),
  });

  const login = useCallback(async (username: string, password: string) => {
    const result = await authApi.login(username, password);
    writeStorage(SESSION_KEY, JSON.stringify(result));
    sessionRef.current = result;
    setSession(result);
    return result;
  }, []);

  const value = useMemo<AuthState>(
    () => ({
      session,
      login,
      logout,
      hasRole: (...roles) => !!session && roles.includes(session.role),
      takeFlash: () => {
        let message: string | null = null;
        try {
          message = sessionStorage.getItem(FLASH_KEY);
        } catch {
          message = null;
        }
        writeStorage(FLASH_KEY, null);
        return message;
      },
    }),
    [session, login, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthState {
  const context = useContext(AuthContext);
  if (!context) throw new Error("useAuth must be used inside <AuthProvider>.");
  return context;
}

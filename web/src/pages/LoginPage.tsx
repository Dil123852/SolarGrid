/*
 * File: LoginPage.tsx
 * Purpose: Split-screen staff sign-in (Backoffice / Grid Operator) via /api/auth/login.
 *          Signed-in users are sent straight to the dashboard.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

import { useEffect, useState, type FormEvent } from "react";
import { Navigate, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import "../styles/login.css";

export function LoginPage() {
  const { session, login, takeFlash } = useAuth();
  const navigate = useNavigate();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [flash] = useState(takeFlash);

  // The login page has its own full-bleed background.
  useEffect(() => {
    document.body.classList.add("login-page");
    return () => document.body.classList.remove("login-page");
  }, []);

  if (session) return <Navigate to="/dashboard" replace />;

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    if (!username.trim() || !password) {
      setError("Enter your username and password.");
      return;
    }
    setBusy(true);
    try {
      await login(username.trim(), password);
      navigate("/dashboard", { replace: true });
    } catch (err) {
      setError(err instanceof Error ? err.message : String(err));
      setBusy(false);
    }
  };

  return (
    <main className="login">
      <section className="login-brand" aria-label="SolarGrid">
        <header className="brand-head">
          <div className="brand-mark">
            <span className="brand-logo" aria-hidden="true">
              S
            </span>
            <span className="brand-name">SolarGrid</span>
          </div>
          <p className="eyebrow eyebrow-dark">Staff portal</p>
        </header>

        <div className="brand-body">
          <span className="brand-rule" aria-hidden="true" />
          <h1 className="brand-title">Smart Solar Microgrid Trading System</h1>
          <p className="brand-lead">
            A precision control surface for the people who keep the grid balanced — backoffice officers and grid
            operators.
          </p>
        </div>

        <footer className="brand-roles" aria-label="Roles">
          <span>Backoffice</span>
          <span className="role-dash" aria-hidden="true" />
          <span>Grid operators</span>
          <span className="role-dash" aria-hidden="true" />
          <span>Prosumers</span>
        </footer>
      </section>

      <section className="login-panel">
        <div className="login-form-wrap">
          <p className="eyebrow">Sign in</p>
          <h2 className="login-title">Staff access</h2>

          {flash && (
            <div className="notice notice-warning" role="alert">
              {flash}
            </div>
          )}

          <form onSubmit={submit} noValidate>
            <label htmlFor="username" className="field-label">
              Username
            </label>
            <input
              id="username"
              className="field-input"
              placeholder="operator.id"
              autoComplete="username"
              autoFocus
              value={username}
              onChange={(e) => setUsername(e.target.value)}
            />

            <label htmlFor="password" className="field-label">
              Password
            </label>
            <input
              id="password"
              type="password"
              className="field-input"
              placeholder="••••••••"
              autoComplete="current-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />

            {error && (
              <div className="notice notice-error" role="alert">
                {error}
              </div>
            )}

            <button type="submit" className="btn-signin" disabled={busy}>
              {busy && <span className="spinner-border" aria-hidden="true" />}
              Sign in
            </button>
          </form>

          <hr className="login-divider" />
          <p className="login-note">For Backoffice officers and Grid Operators. Prosumers use the SolarGrid mobile app.</p>
        </div>
      </section>
    </main>
  );
}

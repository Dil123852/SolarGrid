/*
 * File: Toasts.tsx
 * Purpose: App-wide toast notifications (Bootstrap styling). useToast().success / .error from
 *          any page; errors show the API's own message.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from "react";
import { CiCircleRemove } from "react-icons/ci";

type ToastKind = "success" | "danger" | "warning" | "info";

interface Toast {
  id: number;
  kind: ToastKind;
  message: string;
}

interface ToastApi {
  success: (message: string) => void;
  error: (error: unknown) => void;
}

const ToastContext = createContext<ToastApi | null>(null);
const TOAST_LIFETIME_MS = 4500;
let nextId = 1;

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([]);

  const dismiss = useCallback((id: number) => setToasts((all) => all.filter((t) => t.id !== id)), []);

  const push = useCallback(
    (kind: ToastKind, message: string) => {
      const id = nextId++;
      setToasts((all) => [...all, { id, kind, message }]);
      window.setTimeout(() => dismiss(id), TOAST_LIFETIME_MS);
    },
    [dismiss],
  );

  const api = useMemo<ToastApi>(
    () => ({
      success: (message) => push("success", message),
      error: (error) => push("danger", error instanceof Error ? error.message : String(error)),
    }),
    [push],
  );

  return (
    <ToastContext.Provider value={api}>
      {children}
      <div className="toast-container position-fixed bottom-0 end-0 p-3">
        {toasts.map((t) => (
          <div key={t.id} className={`toast show align-items-center border-0 text-bg-${t.kind}`} role="alert">
            <div className="d-flex">
              <div className="toast-body">{t.message}</div>
              <button
                type="button"
                className="sg-icon-btn sg-icon-btn-light me-2 m-auto"
                aria-label="Close"
                onClick={() => dismiss(t.id)}
              >
                <CiCircleRemove />
              </button>
            </div>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
}

export function useToast(): ToastApi {
  const context = useContext(ToastContext);
  if (!context) throw new Error("useToast must be used inside <ToastProvider>.");
  return context;
}

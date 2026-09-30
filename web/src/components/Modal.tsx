/*
 * File: Modal.tsx
 * Purpose: Controlled Bootstrap-styled modal dialog (no Bootstrap JS). Closes on Escape or
 *          backdrop click; the body is a form so Enter submits.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

import { useEffect, useId, type FormEvent, type ReactNode } from "react";

interface ModalProps {
  show: boolean;
  title: string;
  onClose: () => void;
  onSubmit?: (event: FormEvent<HTMLFormElement>) => void;
  footer?: ReactNode;
  size?: "sm" | "lg";
  centered?: boolean;
  children: ReactNode;
}

export function Modal({ show, title, onClose, onSubmit, footer, size, centered, children }: ModalProps) {
  const titleId = useId();

  useEffect(() => {
    if (!show) return;
    const onKey = (e: KeyboardEvent) => e.key === "Escape" && onClose();
    document.addEventListener("keydown", onKey);
    document.body.classList.add("modal-open");
    return () => {
      document.removeEventListener("keydown", onKey);
      document.body.classList.remove("modal-open");
    };
  }, [show, onClose]);

  if (!show) return null;

  const dialogClass = ["modal-dialog", size ? `modal-${size}` : "", centered ? "modal-dialog-centered" : ""].join(" ");

  return (
    <>
      <div
        className="modal fade show d-block"
        tabIndex={-1}
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        onMouseDown={(e) => e.target === e.currentTarget && onClose()}
      >
        <div className={dialogClass}>
          <form
            className="modal-content"
            noValidate
            onSubmit={(e) => {
              e.preventDefault();
              onSubmit?.(e);
            }}
          >
            <div className="modal-header">
              <h2 className="modal-title h5" id={titleId}>
                {title}
              </h2>
              <button type="button" className="btn-close" aria-label="Close" onClick={onClose} />
            </div>
            <div className="modal-body">{children}</div>
            {footer && <div className="modal-footer">{footer}</div>}
          </form>
        </div>
      </div>
      <div className="modal-backdrop fade show" />
    </>
  );
}

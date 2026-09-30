/*
 * File: BusyButton.tsx
 * Purpose: Button that disables itself and shows a spinner while its async action runs.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

import { useState, type ButtonHTMLAttributes, type ReactNode } from "react";

interface BusyButtonProps extends Omit<ButtonHTMLAttributes<HTMLButtonElement>, "onClick"> {
  onClick?: () => Promise<unknown> | void;
  busy?: boolean;
  children: ReactNode;
}

export function BusyButton({ onClick, busy: externalBusy, disabled, children, ...rest }: BusyButtonProps) {
  const [internalBusy, setInternalBusy] = useState(false);
  const busy = externalBusy ?? internalBusy;

  const handleClick = async () => {
    if (!onClick) return;
    setInternalBusy(true);
    try {
      await onClick();
    } finally {
      setInternalBusy(false);
    }
  };

  return (
    <button {...rest} disabled={disabled || busy} onClick={onClick ? handleClick : undefined}>
      {busy && <span className="spinner-border spinner-border-sm me-1" aria-hidden="true" />}
      {children}
    </button>
  );
}

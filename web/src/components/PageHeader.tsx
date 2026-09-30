/*
 * File: PageHeader.tsx
 * Purpose: Consistent page heading - uppercase section label, title, subtitle and actions -
 *          matching the sign-in page's typography.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

import type { ReactNode } from "react";

interface PageHeaderProps {
  title: string;
  eyebrow?: string;
  subtitle?: ReactNode;
  actions?: ReactNode;
}

export function PageHeader({ title, eyebrow = "Staff portal", subtitle, actions }: PageHeaderProps) {
  return (
    <div className="sg-page-head d-flex flex-wrap justify-content-between align-items-end gap-3">
      <div>
        <p className="sg-eyebrow mb-0">{eyebrow}</p>
        <h1 className="sg-page-title">{title}</h1>
        {subtitle && <p className="sg-page-subtitle">{subtitle}</p>}
      </div>
      {actions && <div className="d-flex gap-2 align-items-center">{actions}</div>}
    </div>
  );
}

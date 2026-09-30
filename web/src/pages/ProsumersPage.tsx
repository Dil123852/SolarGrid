/*
 * File: ProsumersPage.tsx
 * Purpose: Prosumer accounts - the Pending Activation list (deactivated accounts) with
 *          Backoffice reactivation, and the full prosumer list.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

import { useState } from "react";
import { prosumersApi } from "../api/endpoints";
import type { Prosumer } from "../api/types";
import { ActiveBadge, EmptyRow, LoadingRow } from "../components/Badges";
import { BusyButton } from "../components/BusyButton";
import { PageHeader } from "../components/PageHeader";
import { useToast } from "../components/Toasts";
import { useApiData } from "../hooks/useApiData";
import { formatDateTime } from "../utils/format";
import { CiCircleCheck, CiTimer } from "react-icons/ci";

type Tab = "pending" | "all";

export function ProsumersPage() {
  const toast = useToast();
  const [tab, setTab] = useState<Tab>("pending");
  const { data, loading, error, reload } = useApiData(async () => {
    const [list, pending] = await Promise.all([
      prosumersApi.list(tab === "pending" ? false : undefined),
      tab === "pending" ? Promise.resolve(null) : prosumersApi.list(false),
    ]);
    return { list, pendingCount: (pending ?? list).length };
  }, [tab]);

  const toggle = async (p: Prosumer) => {
    try {
      const result = p.isActive ? await prosumersApi.deactivate(p.nic) : await prosumersApi.reactivate(p.nic);
      toast.success(result.message);
      await reload();
    } catch (err) {
      toast.error(err);
    }
  };

  return (
    <>
      <PageHeader
        eyebrow="Accounts"
        title="Prosumers"
        subtitle="Solar property owners registered through the mobile app. Only Backoffice can reactivate an account."
      />

      <ul className="nav nav-pills mb-3">
        <li className="nav-item">
          <button className={`nav-link${tab === "pending" ? " active" : ""}`} onClick={() => setTab("pending")}>
            <CiTimer className="me-1" />
            Pending activation <span className="badge text-bg-warning ms-1">{data?.pendingCount ?? 0}</span>
          </button>
        </li>
        <li className="nav-item">
          <button className={`nav-link${tab === "all" ? " active" : ""}`} onClick={() => setTab("all")}>
            All prosumers
          </button>
        </li>
      </ul>

      {error && <div className="alert alert-danger">{error}</div>}

      <div className="card sg-card">
        <div className="table-responsive">
          <table className="table table-hover mb-0">
            <thead className="table-light">
              <tr>
                <th>NIC</th>
                <th>Name</th>
                <th>Email</th>
                <th>Phone</th>
                <th>Status</th>
                <th>Registered</th>
                <th className="text-end">Actions</th>
              </tr>
            </thead>
            <tbody>
              {loading && !data ? (
                <LoadingRow colSpan={7} />
              ) : !data?.list.length ? (
                <EmptyRow
                  colSpan={7}
                  text={tab === "pending" ? "No accounts are waiting for activation." : "No prosumers registered yet."}
                />
              ) : (
                data.list.map((p) => (
                  <tr key={p.nic}>
                    <td className="fw-medium">{p.nic}</td>
                    <td>{p.name}</td>
                    <td>{p.email}</td>
                    <td>{p.phone}</td>
                    <td>
                      <ActiveBadge active={p.isActive} />
                    </td>
                    <td className="small text-body-secondary">{formatDateTime(p.createdAt)}</td>
                    <td className="text-end">
                      <BusyButton
                        className={`btn btn-sm ${p.isActive ? "btn-outline-danger" : "btn-success"}`}
                        onClick={() => toggle(p)}
                      >
                        {!p.isActive && <CiCircleCheck className="me-1" />}
                        {p.isActive ? "Deactivate" : "Reactivate"}
                      </BusyButton>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </>
  );
}

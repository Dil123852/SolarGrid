/*
 * File: DashboardPage.tsx
 * Purpose: Live counts from /api/dashboard and the next bookings awaiting approval.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

import type { IconType } from "react-icons";
import { Link } from "react-router-dom";
import { dashboardApi, reservationsApi } from "../api/endpoints";
import { useAuth } from "../auth/AuthContext";
import { EmptyRow, LoadingRow, StatusBadge } from "../components/Badges";
import { BusyButton } from "../components/BusyButton";
import { PageHeader } from "../components/PageHeader";
import { useApiData } from "../hooks/useApiData";
import { formatDateTime, roleLabel } from "../utils/format";
import { CiBatteryFull, CiCalendarDate, CiRepeat, CiTimer } from "react-icons/ci";

const NEXT_PENDING_LIMIT = 5;

function StatCard({ value, label, icon: Icon, color }: { value: number | undefined; label: string; icon: IconType; color: string }) {
  return (
    <div className="col-md-4">
      <div className="card sg-card h-100">
        <div className="card-body d-flex align-items-center gap-3">
          <div className={`sg-stat-icon text-bg-${color}`}>
            <Icon />
          </div>
          <div>
            <div className="sg-stat">{value ?? "–"}</div>
            <div className="text-body-secondary small">{label}</div>
          </div>
        </div>
      </div>
    </div>
  );
}

export function DashboardPage() {
  const { session } = useAuth();
  const { data, loading, error, reload } = useApiData(async () => {
    const [counts, pending] = await Promise.all([
      dashboardApi.get(),
      reservationsApi.list({ status: "Pending", from: new Date().toISOString() }),
    ]);
    // The API returns the newest slot first; show the soonest few.
    return { counts, next: [...pending].reverse().slice(0, NEXT_PENDING_LIMIT) };
  }, []);

  return (
    <>
      <PageHeader
        eyebrow="Overview"
        title="Dashboard"
        subtitle={session ? `Signed in as ${session.displayName} (${roleLabel(session.role)})` : "Live reservation overview"}
        actions={
          <BusyButton className="btn btn-outline-secondary btn-sm" onClick={reload}>
            <CiRepeat className="me-1" />
            Refresh
          </BusyButton>
        }
      />

      {error && <div className="alert alert-danger">{error}</div>}

      <div className="row g-3 mb-4">
        <StatCard value={data?.counts.pendingCount} label="Pending approval" icon={CiTimer} color="warning" />
        <StatCard value={data?.counts.approvedFutureCount} label="Approved upcoming" icon={CiCalendarDate} color="success" />
        <StatCard value={data?.counts.completedCount} label="Transfers completed" icon={CiBatteryFull} color="primary" />
      </div>

      <div className="card sg-card">
        <div className="card-header bg-white d-flex justify-content-between align-items-center">
          <span className="fw-semibold">Next reservations awaiting approval</span>
          <Link to="/bookings?status=Pending" className="small">
            View all
          </Link>
        </div>
        <div className="table-responsive">
          <table className="table table-hover mb-0">
            <thead className="table-light">
              <tr>
                <th>Slot</th>
                <th>Node</th>
                <th>Prosumer NIC</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {loading && !data ? (
                <LoadingRow colSpan={4} />
              ) : !data?.next.length ? (
                <EmptyRow colSpan={4} text="No upcoming reservations are waiting for approval." />
              ) : (
                data.next.map((r) => (
                  <tr key={r.id}>
                    <td>{formatDateTime(r.slotTime)}</td>
                    <td>{r.nodeName}</td>
                    <td>{r.prosumerNic}</td>
                    <td>
                      <StatusBadge status={r.status} />
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

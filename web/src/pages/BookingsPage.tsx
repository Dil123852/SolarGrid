/*
 * File: BookingsPage.tsx
 * Purpose: Slot booking management - filtered booking list for staff; Backoffice can create,
 *          reschedule, cancel and approve bookings and show the approved QR token.
 *          All window/notice/capacity rules are enforced by the API; errors are shown as returned.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

import { useEffect, useMemo, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { QRCodeSVG } from "qrcode.react";
import { nodesApi, reservationsApi } from "../api/endpoints";
import type { Reservation, ReservationStatus } from "../api/types";
import { useAuth } from "../auth/AuthContext";
import { EmptyRow, LoadingRow, StatusBadge } from "../components/Badges";
import { BusyButton } from "../components/BusyButton";
import { Modal } from "../components/Modal";
import { PageHeader } from "../components/PageHeader";
import { useToast } from "../components/Toasts";
import { useApiData } from "../hooks/useApiData";
import { dayEndIso, dayStartIso, formatDateTime, fromLocalInput, plural, toLocalInput } from "../utils/format";
import { CiBarcode, CiCircleCheck, CiCirclePlus, CiCircleRemove, CiEdit } from "react-icons/ci";

const STATUSES: ReservationStatus[] = ["Pending", "Approved", "Completed", "Cancelled"];
const BOOKING_WINDOW_MS = 7 * 24 * 60 * 60 * 1000;

interface Filters {
  status: ReservationStatus | "";
  nodeId: string;
  nic: string;
  from: string;
  to: string;
}

interface BookingForm {
  id: string | null;
  prosumerNic: string;
  nodeId: string;
  slot: string;
}

export function BookingsPage() {
  const { hasRole } = useAuth();
  const canEdit = hasRole("Backoffice");
  const toast = useToast();
  const [searchParams] = useSearchParams();

  const initialStatus = searchParams.get("status");
  const [filters, setFilters] = useState<Filters>({
    status: STATUSES.includes(initialStatus as ReservationStatus) ? (initialStatus as ReservationStatus) : "",
    nodeId: "",
    nic: "",
    from: "",
    to: "",
  });

  // The NIC box is typed into, so it is debounced before hitting the API.
  const [nicQuery, setNicQuery] = useState("");
  useEffect(() => {
    const timer = window.setTimeout(() => setNicQuery(filters.nic.trim()), 400);
    return () => window.clearTimeout(timer);
  }, [filters.nic]);

  const { data: nodes } = useApiData(() => nodesApi.list(), []);
  const { data: bookings, loading, error, reload } = useApiData(
    () =>
      reservationsApi.list({
        status: filters.status,
        nodeId: filters.nodeId,
        nic: nicQuery,
        from: dayStartIso(filters.from),
        to: dayEndIso(filters.to),
      }),
    [filters.status, filters.nodeId, nicQuery, filters.from, filters.to],
  );

  const [form, setForm] = useState<BookingForm | null>(null);
  const [validated, setValidated] = useState(false);
  const [saving, setSaving] = useState(false);
  const [qrBooking, setQrBooking] = useState<Reservation | null>(null);

  const activeNodes = useMemo(
    () => (nodes ?? []).filter((n) => n.isActive || n.id === form?.nodeId),
    [nodes, form?.nodeId],
  );

  const setFilter = <K extends keyof Filters>(key: K, value: Filters[K]) => setFilters((f) => ({ ...f, [key]: value }));
  const clearFilters = () => setFilters({ status: "", nodeId: "", nic: "", from: "", to: "" });

  const openForm = (booking?: Reservation) => {
    setValidated(false);
    setForm(
      booking
        ? { id: booking.id, prosumerNic: booking.prosumerNic, nodeId: booking.nodeId, slot: toLocalInput(booking.slotTime) }
        : { id: null, prosumerNic: "", nodeId: nodes?.find((n) => n.isActive)?.id ?? "", slot: "" },
    );
  };

  const save = async () => {
    if (!form) return;
    setValidated(true);
    if ((!form.id && !form.prosumerNic.trim()) || !form.nodeId || !form.slot) return;
    setSaving(true);
    try {
      const slotTime = fromLocalInput(form.slot);
      const saved = form.id
        ? await reservationsApi.update(form.id, { slotTime, nodeId: form.nodeId })
        : await reservationsApi.create({ nodeId: form.nodeId, slotTime, prosumerNic: form.prosumerNic.trim() });
      toast.success(
        `${form.id ? "Booking rescheduled" : "Booking created"} for ${saved.nodeName} on ${formatDateTime(saved.slotTime)}.`,
      );
      setForm(null);
      await reload();
    } catch (err) {
      toast.error(err);
    } finally {
      setSaving(false);
    }
  };

  const cancel = async (booking: Reservation) => {
    if (!window.confirm(`Cancel the booking at ${booking.nodeName} on ${formatDateTime(booking.slotTime)}?`)) return;
    try {
      await reservationsApi.cancel(booking.id);
      toast.success("Booking cancelled.");
      await reload();
    } catch (err) {
      toast.error(err);
    }
  };

  const approve = async (booking: Reservation) => {
    try {
      const approved = await reservationsApi.approve(booking.id);
      toast.success("Booking approved - QR token issued.");
      await reload();
      setQrBooking(approved);
    } catch (err) {
      toast.error(err);
    }
  };

  const now = new Date();
  const invalid = (bad: boolean) => (validated && bad ? " is-invalid" : "");

  return (
    <>
      <PageHeader
        eyebrow="Operations"
        title="Energy Slot Bookings"
        subtitle="Bookings can be made up to 7 days ahead; changes need 12 hours' notice."
        actions={
          canEdit && (
            <button className="btn btn-primary" onClick={() => openForm()}>
              <CiCirclePlus className="me-1" />
              New booking
            </button>
          )
        }
      />

      <div className="card sg-card mb-3">
        <div className="card-body row g-2 align-items-end">
          <div className="col-6 col-md-2">
            <label className="form-label small mb-1" htmlFor="fStatus">
              Status
            </label>
            <select
              id="fStatus"
              className="form-select form-select-sm"
              value={filters.status}
              onChange={(e) => setFilter("status", e.target.value as Filters["status"])}
            >
              <option value="">Any</option>
              {STATUSES.map((s) => (
                <option key={s}>{s}</option>
              ))}
            </select>
          </div>
          <div className="col-6 col-md-3">
            <label className="form-label small mb-1" htmlFor="fNode">
              Node
            </label>
            <select
              id="fNode"
              className="form-select form-select-sm"
              value={filters.nodeId}
              onChange={(e) => setFilter("nodeId", e.target.value)}
            >
              <option value="">Any node</option>
              {(nodes ?? []).map((n) => (
                <option key={n.id} value={n.id}>
                  {n.name}
                  {n.isActive ? "" : " (inactive)"}
                </option>
              ))}
            </select>
          </div>
          <div className="col-6 col-md-2">
            <label className="form-label small mb-1" htmlFor="fNic">
              Prosumer NIC
            </label>
            <input
              id="fNic"
              className="form-control form-control-sm"
              value={filters.nic}
              onChange={(e) => setFilter("nic", e.target.value)}
            />
          </div>
          <div className="col-6 col-md-2">
            <label className="form-label small mb-1" htmlFor="fFrom">
              From
            </label>
            <input
              id="fFrom"
              type="date"
              className="form-control form-control-sm"
              value={filters.from}
              onChange={(e) => setFilter("from", e.target.value)}
            />
          </div>
          <div className="col-6 col-md-2">
            <label className="form-label small mb-1" htmlFor="fTo">
              To
            </label>
            <input
              id="fTo"
              type="date"
              className="form-control form-control-sm"
              value={filters.to}
              onChange={(e) => setFilter("to", e.target.value)}
            />
          </div>
          <div className="col-6 col-md-1 d-grid">
            <button className="btn btn-sm btn-outline-secondary" title="Clear filters" onClick={clearFilters}>
              <CiCircleRemove />
            </button>
          </div>
        </div>
      </div>

      {error && <div className="alert alert-danger">{error}</div>}

      <div className="card sg-card">
        <div className="table-responsive">
          <table className="table table-hover mb-0">
            <thead className="table-light">
              <tr>
                <th>Slot</th>
                <th>Node</th>
                <th>Prosumer NIC</th>
                <th>Status</th>
                <th>Booked</th>
                <th className="text-end">Actions</th>
              </tr>
            </thead>
            <tbody>
              {loading && !bookings ? (
                <LoadingRow colSpan={6} />
              ) : !bookings?.length ? (
                <EmptyRow colSpan={6} text="No bookings match these filters." />
              ) : (
                bookings.map((r) => {
                  const live = r.status === "Pending" || r.status === "Approved";
                  return (
                    <tr key={r.id}>
                      <td className="fw-medium text-nowrap">{formatDateTime(r.slotTime)}</td>
                      <td>{r.nodeName}</td>
                      <td>{r.prosumerNic}</td>
                      <td>
                        <StatusBadge status={r.status} />
                      </td>
                      <td className="small text-body-secondary">{formatDateTime(r.createdAt)}</td>
                      <td className="text-end text-nowrap">
                        <div className="d-inline-flex gap-1">
                          {r.qrToken && (
                            <button className="btn btn-sm btn-outline-secondary" onClick={() => setQrBooking(r)}>
                              <CiBarcode /> QR
                            </button>
                          )}
                          {canEdit && r.status === "Pending" && (
                            <BusyButton className="btn btn-sm btn-outline-success" onClick={() => approve(r)}>
                              <CiCircleCheck /> Approve
                            </BusyButton>
                          )}
                          {canEdit && live && (
                            <>
                              <button className="btn btn-sm btn-outline-primary" onClick={() => openForm(r)}>
                                <CiEdit /> Reschedule
                              </button>
                              <BusyButton className="btn btn-sm btn-outline-danger" onClick={() => cancel(r)}>
                                <CiCircleRemove /> Cancel
                              </BusyButton>
                            </>
                          )}
                        </div>
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
        <div className="card-footer bg-white small text-body-secondary">{plural(bookings?.length ?? 0, "booking")}</div>
      </div>

      <Modal
        show={form !== null}
        title={form?.id ? "Reschedule booking" : "New booking"}
        onClose={() => setForm(null)}
        onSubmit={save}
        footer={
          <>
            <button type="button" className="btn btn-outline-secondary" onClick={() => setForm(null)}>
              Close
            </button>
            <BusyButton type="submit" className="btn btn-primary" busy={saving}>
              Save booking
            </BusyButton>
          </>
        }
      >
        {form && (
          <>
            {!form.id && (
              <div className="mb-3">
                <label className="form-label" htmlFor="bNic">
                  Prosumer NIC
                </label>
                <input
                  id="bNic"
                  className={`form-control${invalid(!form.prosumerNic.trim())}`}
                  value={form.prosumerNic}
                  onChange={(e) => setForm({ ...form, prosumerNic: e.target.value })}
                  autoFocus
                />
              </div>
            )}
            <div className="mb-3">
              <label className="form-label" htmlFor="bNode">
                Microgrid node
              </label>
              <select
                id="bNode"
                className={`form-select${invalid(!form.nodeId)}`}
                value={form.nodeId}
                onChange={(e) => setForm({ ...form, nodeId: e.target.value })}
              >
                {activeNodes.length === 0 && <option value="">No active nodes</option>}
                {activeNodes.map((n) => (
                  <option key={n.id} value={n.id}>
                    {n.name} · {plural(n.batterySlots, "slot")}
                  </option>
                ))}
              </select>
            </div>
            <div className="mb-1">
              <label className="form-label" htmlFor="bSlot">
                Slot date &amp; time
              </label>
              <input
                id="bSlot"
                type="datetime-local"
                className={`form-control${invalid(!form.slot)}`}
                min={toLocalInput(now)}
                max={toLocalInput(new Date(now.getTime() + BOOKING_WINDOW_MS))}
                value={form.slot}
                onChange={(e) => setForm({ ...form, slot: e.target.value })}
              />
              <div className="form-text">Within the next 7 days.</div>
            </div>
          </>
        )}
      </Modal>

      <Modal show={qrBooking !== null} title="Transaction QR" size="sm" centered onClose={() => setQrBooking(null)}>
        {qrBooking?.qrToken && (
          <div className="text-center">
            <div className="sg-qr mb-2">
              <QRCodeSVG value={qrBooking.qrToken} size={200} />
            </div>
            <p className="small mb-1">
              {qrBooking.nodeName} · {formatDateTime(qrBooking.slotTime)}
            </p>
            <code className="sg-token small">{qrBooking.qrToken}</code>
            <p className="small text-body-secondary mt-2 mb-0">The Grid Operator scans this in the mobile app's Operator Mode.</p>
          </div>
        )}
      </Modal>
    </>
  );
}

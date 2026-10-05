/*
 * File: SlotsPage.tsx
 * Purpose: Energy booking slot management for Backoffice and Grid Operators - publish, edit and
 *          delete the time windows each solar station takes bookings in, with live booked and
 *          available counts. All rules (overlap, capacity, opening hours) are enforced by the API.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

import { useMemo, useState } from "react";
import { CiCirclePlus, CiCircleRemove, CiEdit } from "react-icons/ci";
import { nodesApi, slotsApi } from "../api/endpoints";
import type { BookingSlot } from "../api/types";
import { EmptyRow, LoadingRow } from "../components/Badges";
import { BusyButton } from "../components/BusyButton";
import { Modal } from "../components/Modal";
import { PageHeader } from "../components/PageHeader";
import { useToast } from "../components/Toasts";
import { useApiData } from "../hooks/useApiData";
import { dayEndIso, dayStartIso } from "../utils/format";

// Form values are local date + "HH:mm" times, converted to UTC ISO strings for the API.
interface SlotForm {
  id: string | null;
  nodeId: string;
  date: string;
  start: string;
  end: string;
  capacity: string;
}

// yyyy-mm-dd for a Date in local time.
function localDate(d: Date): string {
  const copy = new Date(d);
  copy.setMinutes(copy.getMinutes() - copy.getTimezoneOffset());
  return copy.toISOString().slice(0, 10);
}

// HH:mm for an ISO time in local time.
function localTime(iso: string): string {
  return new Date(iso).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit", hour12: false });
}

// Combines a local date and "HH:mm" into a UTC ISO string.
function toIso(date: string, time: string): string {
  return new Date(`${date}T${time}`).toISOString();
}

export function SlotsPage() {
  const toast = useToast();
  const today = localDate(new Date());
  const [nodeFilter, setNodeFilter] = useState("");
  const [fromDay, setFromDay] = useState(today);
  const [toDay, setToDay] = useState("");

  const { data: nodes } = useApiData(() => nodesApi.list(), []);
  const { data: slots, loading, error, reload } = useApiData(
    () => slotsApi.list({ nodeId: nodeFilter, from: dayStartIso(fromDay), to: dayEndIso(toDay) }),
    [nodeFilter, fromDay, toDay],
  );

  const activeNodes = useMemo(() => (nodes ?? []).filter((n) => n.isActive), [nodes]);
  const [form, setForm] = useState<SlotForm | null>(null);
  const [validated, setValidated] = useState(false);
  const [saving, setSaving] = useState(false);

  const selectedNode = (nodes ?? []).find((n) => n.id === form?.nodeId);
  const maxCapacity = selectedNode?.batterySlots ?? 1;

  // Opens the form empty (new slot) or filled from an existing slot.
  const openForm = (slot?: BookingSlot) => {
    setValidated(false);
    setForm(
      slot
        ? {
            id: slot.id,
            nodeId: slot.nodeId,
            date: localDate(new Date(slot.startTime)),
            start: localTime(slot.startTime),
            end: localTime(slot.endTime),
            capacity: String(slot.capacity),
          }
        : { id: null, nodeId: nodeFilter || activeNodes[0]?.id || "", date: today, start: "08:00", end: "10:00", capacity: "1" },
    );
  };

  const setField = (key: keyof SlotForm, value: string) => setForm((f) => (f ? { ...f, [key]: value } : f));

  const errors = form && {
    nodeId: !form.nodeId,
    date: !form.date,
    window: !form.start || !form.end || form.end <= form.start,
    capacity: !(Number.isInteger(Number(form.capacity)) && Number(form.capacity) >= 1),
  };

  // Publishes a new slot or saves changes to an existing one.
  const save = async () => {
    if (!form || !errors) return;
    setValidated(true);
    if (Object.values(errors).some(Boolean)) return;
    const window = {
      startTime: toIso(form.date, form.start),
      endTime: toIso(form.date, form.end),
      capacity: Number(form.capacity),
    };
    setSaving(true);
    try {
      if (form.id) await slotsApi.update(form.id, window);
      else await slotsApi.create({ nodeId: form.nodeId, ...window });
      toast.success(form.id ? "Booking slot updated." : "Booking slot published.");
      setForm(null);
      await reload();
    } catch (err) {
      toast.error(err);
    } finally {
      setSaving(false);
    }
  };

  // Deletes a slot after confirmation (the API refuses while live bookings are in it).
  const remove = async (slot: BookingSlot) => {
    const label = `${slot.nodeName}, ${new Date(slot.startTime).toLocaleDateString()} ${localTime(slot.startTime)}-${localTime(slot.endTime)}`;
    if (!window.confirm(`Delete the slot at ${label}?`)) return;
    try {
      const result = await slotsApi.remove(slot.id);
      toast.success(result.message);
      await reload();
    } catch (err) {
      toast.error(err);
    }
  };

  const invalid = (bad: boolean | undefined) => (validated && bad ? " is-invalid" : "");

  return (
    <>
      <PageHeader
        eyebrow="Operations"
        title="Energy Booking Slots"
        subtitle="Time windows each solar station takes bookings in. Stations with slots only accept bookings inside them."
        actions={
          <button className="btn btn-primary" onClick={() => openForm()} disabled={activeNodes.length === 0}>
            <CiCirclePlus className="me-1" />
            New slot
          </button>
        }
      />

      <div className="card sg-card mb-3">
        <div className="card-body row g-2 align-items-end">
          <div className="col-12 col-md-5">
            <label className="form-label small mb-1" htmlFor="sNode">
              Station
            </label>
            <select id="sNode" className="form-select form-select-sm" value={nodeFilter} onChange={(e) => setNodeFilter(e.target.value)}>
              <option value="">All stations</option>
              {(nodes ?? []).map((n) => (
                <option key={n.id} value={n.id}>
                  {n.name}
                  {n.isActive ? "" : " (inactive)"}
                </option>
              ))}
            </select>
          </div>
          <div className="col-6 col-md-3">
            <label className="form-label small mb-1" htmlFor="sFrom">
              From
            </label>
            <input id="sFrom" type="date" className="form-control form-control-sm" value={fromDay} onChange={(e) => setFromDay(e.target.value)} />
          </div>
          <div className="col-6 col-md-3">
            <label className="form-label small mb-1" htmlFor="sTo">
              To
            </label>
            <input id="sTo" type="date" className="form-control form-control-sm" value={toDay} onChange={(e) => setToDay(e.target.value)} />
          </div>
          <div className="col-12 col-md-1 d-grid">
            <button
              className="btn btn-sm btn-outline-secondary"
              title="Reset filters"
              onClick={() => {
                setNodeFilter("");
                setFromDay(today);
                setToDay("");
              }}
            >
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
                <th>Station</th>
                <th>Date</th>
                <th>Window</th>
                <th className="text-end">Capacity</th>
                <th className="text-end">Booked</th>
                <th className="text-end">Available</th>
                <th className="text-end">Actions</th>
              </tr>
            </thead>
            <tbody>
              {loading && !slots ? (
                <LoadingRow colSpan={7} />
              ) : !slots?.length ? (
                <EmptyRow colSpan={7} text="No booking slots in this range. Stations without slots take bookings at any time within their hours." />
              ) : (
                slots.map((s) => (
                  <tr key={s.id}>
                    <td className="fw-medium">{s.nodeName}</td>
                    <td className="text-nowrap">{new Date(s.startTime).toLocaleDateString(undefined, { dateStyle: "medium" })}</td>
                    <td className="text-nowrap">
                      {localTime(s.startTime)} - {localTime(s.endTime)}
                    </td>
                    <td className="text-end">{s.capacity}</td>
                    <td className="text-end">{s.booked}</td>
                    <td className="text-end">
                      <span className={`badge ${s.available > 0 ? "text-bg-success" : "text-bg-secondary"}`}>
                        {s.available > 0 ? `${s.available} free` : "Full"}
                      </span>
                    </td>
                    <td className="text-end text-nowrap">
                      <div className="d-inline-flex gap-1">
                        <button className="btn btn-sm btn-outline-primary" onClick={() => openForm(s)}>
                          <CiEdit /> Edit
                        </button>
                        <BusyButton className="btn btn-sm btn-outline-danger" onClick={() => remove(s)}>
                          <CiCircleRemove /> Delete
                        </BusyButton>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
        <div className="card-footer bg-white small text-body-secondary">
          {slots?.length ?? 0} slot{slots?.length === 1 ? "" : "s"}
        </div>
      </div>

      <Modal
        show={form !== null}
        title={form?.id ? "Edit booking slot" : "New booking slot"}
        onClose={() => setForm(null)}
        onSubmit={save}
        footer={
          <>
            <button type="button" className="btn btn-outline-secondary" onClick={() => setForm(null)}>
              Cancel
            </button>
            <BusyButton type="submit" className="btn btn-primary" busy={saving}>
              {form?.id ? "Save slot" : "Publish slot"}
            </BusyButton>
          </>
        }
      >
        {form && (
          <>
            <div className="mb-3">
              <label className="form-label" htmlFor="fNodeSel">
                Solar station
              </label>
              <select
                id="fNodeSel"
                className={`form-select${invalid(errors?.nodeId)}`}
                value={form.nodeId}
                disabled={!!form.id}
                onChange={(e) => setField("nodeId", e.target.value)}
              >
                {!form.nodeId && <option value="">Choose a station</option>}
                {(form.id ? nodes ?? [] : activeNodes).map((n) => (
                  <option key={n.id} value={n.id}>
                    {n.name} · {n.batterySlots} battery slots
                    {n.openTime && n.closeTime ? ` · ${n.openTime}-${n.closeTime}` : ""}
                  </option>
                ))}
              </select>
            </div>
            <div className="row g-3 mb-3">
              <div className="col-md-4">
                <label className="form-label" htmlFor="fDate">
                  Date
                </label>
                <input
                  id="fDate"
                  type="date"
                  className={`form-control${invalid(errors?.date)}`}
                  value={form.date}
                  min={today}
                  onChange={(e) => setField("date", e.target.value)}
                />
              </div>
              <div className="col-6 col-md-4">
                <label className="form-label" htmlFor="fStart">
                  Starts
                </label>
                <input
                  id="fStart"
                  type="time"
                  className={`form-control${invalid(errors?.window)}`}
                  value={form.start}
                  onChange={(e) => setField("start", e.target.value)}
                />
              </div>
              <div className="col-6 col-md-4">
                <label className="form-label" htmlFor="fEnd">
                  Ends
                </label>
                <input
                  id="fEnd"
                  type="time"
                  className={`form-control${invalid(errors?.window)}`}
                  value={form.end}
                  onChange={(e) => setField("end", e.target.value)}
                />
              </div>
            </div>
            <div className="mb-1">
              <label className="form-label" htmlFor="fCapacity">
                Capacity (bookings)
              </label>
              <input
                id="fCapacity"
                type="number"
                min="1"
                max={maxCapacity}
                step="1"
                className={`form-control${invalid(errors?.capacity)}`}
                value={form.capacity}
                onChange={(e) => setField("capacity", e.target.value)}
              />
              <div className="form-text">
                Up to the station's {maxCapacity} battery slots. Windows last 15 minutes to 24 hours, must fit the
                station's opening hours and may not overlap another slot.
              </div>
            </div>
          </>
        )}
      </Modal>
    </>
  );
}

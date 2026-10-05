/*
 * File: NodesPage.tsx
 * Purpose: Microgrid node management - list/filter nodes with their opening hours; Backoffice can
 *          create, edit (incl. schedule), activate and deactivate (the API refuses deactivation while
 *          bookings are live); Grid Operators keep battery-slot availability current.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

import { useState } from "react";
import { nodesApi } from "../api/endpoints";
import type { MicrogridNode } from "../api/types";
import { useAuth } from "../auth/AuthContext";
import { ActiveBadge, EmptyRow, LoadingRow } from "../components/Badges";
import { BusyButton } from "../components/BusyButton";
import { Modal } from "../components/Modal";
import { PageHeader } from "../components/PageHeader";
import { useToast } from "../components/Toasts";
import { useApiData } from "../hooks/useApiData";
import { CiBatteryCharging, CiCirclePlus, CiEdit, CiMapPin } from "react-icons/ci";

type ActiveFilter = "" | "true" | "false";

// Form fields are kept as strings so inputs can be empty while typing.
interface NodeForm {
  id: string | null;
  name: string;
  latitude: string;
  longitude: string;
  capacityKWh: string;
  batterySlots: string;
  openTime: string;
  closeTime: string;
}

const EMPTY_FORM: NodeForm = {
  id: null,
  name: "",
  latitude: "",
  longitude: "",
  capacityKWh: "",
  batterySlots: "",
  openTime: "",
  closeTime: "",
};

// "06:00-18:00", or "24 hours" when the node has no schedule.
function hoursLabel(node: MicrogridNode): string {
  return node.openTime && node.closeTime ? `${node.openTime}-${node.closeTime}` : "24 hours";
}

function toForm(node: MicrogridNode): NodeForm {
  return {
    id: node.id,
    name: node.name,
    latitude: String(node.latitude),
    longitude: String(node.longitude),
    capacityKWh: String(node.capacityKWh),
    batterySlots: String(node.batterySlots),
    openTime: node.openTime ?? "",
    closeTime: node.closeTime ?? "",
  };
}

export function NodesPage() {
  const { hasRole } = useAuth();
  const canEdit = hasRole("Backoffice");
  // Grid Operators keep battery-slot availability current but cannot change other node details.
  const canUpdateSlots = hasRole("Backoffice", "GridOperator");
  const toast = useToast();
  const [filter, setFilter] = useState<ActiveFilter>("");
  const { data: nodes, loading, error, reload } = useApiData(
    () => nodesApi.list(filter === "" ? undefined : filter === "true"),
    [filter],
  );
  const [form, setForm] = useState<NodeForm | null>(null);
  const [validated, setValidated] = useState(false);
  const [saving, setSaving] = useState(false);
  const [slotsNode, setSlotsNode] = useState<MicrogridNode | null>(null);
  const [slotsValue, setSlotsValue] = useState("");

  const openForm = (node?: MicrogridNode) => {
    setValidated(false);
    setForm(node ? toForm(node) : EMPTY_FORM);
  };

  const setField = (key: keyof NodeForm, value: string) => setForm((f) => (f ? { ...f, [key]: value } : f));

  const errors = form && {
    name: !form.name.trim(),
    latitude: form.latitude === "" || Math.abs(Number(form.latitude)) > 90,
    longitude: form.longitude === "" || Math.abs(Number(form.longitude)) > 180,
    capacityKWh: !(Number(form.capacityKWh) > 0),
    batterySlots: !(Number.isInteger(Number(form.batterySlots)) && Number(form.batterySlots) >= 1),
    // Both hours or neither; opening before closing ("HH:mm" strings compare correctly).
    schedule: !!form.openTime !== !!form.closeTime || (!!form.openTime && form.openTime >= form.closeTime),
  };

  const save = async () => {
    if (!form || !errors) return;
    setValidated(true);
    if (Object.values(errors).some(Boolean)) return;
    const body = {
      name: form.name.trim(),
      latitude: Number(form.latitude),
      longitude: Number(form.longitude),
      capacityKWh: Number(form.capacityKWh),
      batterySlots: Number(form.batterySlots),
      openTime: form.openTime || null,
      closeTime: form.closeTime || null,
    };
    setSaving(true);
    try {
      if (form.id) await nodesApi.update(form.id, body);
      else await nodesApi.create(body);
      toast.success(form.id ? "Node updated." : "Node created.");
      setForm(null);
      await reload();
    } catch (err) {
      toast.error(err);
    } finally {
      setSaving(false);
    }
  };

  const toggle = async (node: MicrogridNode) => {
    try {
      const result = node.isActive ? await nodesApi.deactivate(node.id) : await nodesApi.activate(node.id);
      toast.success(result.message);
      await reload();
    } catch (err) {
      toast.error(err);
    }
  };

  const openSlots = (node: MicrogridNode) => {
    setSlotsNode(node);
    setSlotsValue(String(node.batterySlots));
  };

  const saveSlots = async () => {
    if (!slotsNode) return;
    setSaving(true);
    try {
      await nodesApi.updateSlots(slotsNode.id, Number(slotsValue));
      toast.success(`Battery slots for ${slotsNode.name} updated.`);
      setSlotsNode(null);
      await reload();
    } catch (err) {
      toast.error(err);
    } finally {
      setSaving(false);
    }
  };

  const invalid = (bad: boolean | undefined) => (validated && bad ? " is-invalid" : "");
  const columns = canUpdateSlots ? 7 : 6;

  return (
    <>
      <PageHeader
        eyebrow="Infrastructure"
        title="Microgrid Nodes"
        subtitle="Solar hubs with their GPS location, capacity and battery storage slots."
        actions={
          <>
            <select
              className="form-select form-select-sm w-auto"
              aria-label="Filter nodes by status"
              value={filter}
              onChange={(e) => setFilter(e.target.value as ActiveFilter)}
            >
              <option value="">All nodes</option>
              <option value="true">Active only</option>
              <option value="false">Inactive only</option>
            </select>
            {canEdit && (
              <button className="btn btn-primary" onClick={() => openForm()}>
                <CiCirclePlus className="me-1" />
                New node
              </button>
            )}
          </>
        }
      />

      {error && <div className="alert alert-danger">{error}</div>}

      <div className="card sg-card">
        <div className="table-responsive">
          <table className="table table-hover mb-0">
            <thead className="table-light">
              <tr>
                <th>Name</th>
                <th>Location (lat, lng)</th>
                <th className="text-end">Capacity (kWh)</th>
                <th className="text-end">Battery slots</th>
                <th>Hours</th>
                <th>Status</th>
                {canUpdateSlots && <th className="text-end">Actions</th>}
              </tr>
            </thead>
            <tbody>
              {loading && !nodes ? (
                <LoadingRow colSpan={columns} />
              ) : !nodes?.length ? (
                <EmptyRow colSpan={columns} text="No nodes found." />
              ) : (
                nodes.map((n) => (
                  <tr key={n.id}>
                    <td className="fw-medium">{n.name}</td>
                    <td>
                      <a target="_blank" rel="noopener noreferrer" href={`https://www.google.com/maps?q=${n.latitude},${n.longitude}`}>
                        {n.latitude.toFixed(5)}, {n.longitude.toFixed(5)} <CiMapPin />
                      </a>
                    </td>
                    <td className="text-end">{n.capacityKWh}</td>
                    <td className="text-end">{n.batterySlots}</td>
                    <td className="text-nowrap">{hoursLabel(n)}</td>
                    <td>
                      <ActiveBadge active={n.isActive} />
                    </td>
                    {canUpdateSlots && (
                      <td className="text-end text-nowrap">
                        <div className="d-inline-flex gap-1">
                          <button className="btn btn-sm btn-outline-secondary" onClick={() => openSlots(n)}>
                            <CiBatteryCharging /> Slots
                          </button>
                          {canEdit && (
                            <>
                              <button className="btn btn-sm btn-outline-primary" onClick={() => openForm(n)}>
                                <CiEdit /> Edit
                              </button>
                              <BusyButton
                                className={`btn btn-sm ${n.isActive ? "btn-outline-danger" : "btn-outline-success"}`}
                                onClick={() => toggle(n)}
                              >
                                {n.isActive ? "Deactivate" : "Activate"}
                              </BusyButton>
                            </>
                          )}
                        </div>
                      </td>
                    )}
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      <Modal
        show={form !== null}
        title={form?.id ? "Edit node" : "New node"}
        onClose={() => setForm(null)}
        onSubmit={save}
        footer={
          <>
            <button type="button" className="btn btn-outline-secondary" onClick={() => setForm(null)}>
              Cancel
            </button>
            <BusyButton type="submit" className="btn btn-primary" busy={saving}>
              Save node
            </BusyButton>
          </>
        }
      >
        {form && (
          <>
            <div className="mb-3">
              <label className="form-label" htmlFor="nName">
                Name
              </label>
              <input
                id="nName"
                className={`form-control${invalid(errors?.name)}`}
                value={form.name}
                onChange={(e) => setField("name", e.target.value)}
                autoFocus
              />
            </div>
            <div className="row g-3 mb-3">
              <div className="col-6">
                <label className="form-label" htmlFor="nLat">
                  Latitude
                </label>
                <input
                  id="nLat"
                  type="number"
                  step="any"
                  className={`form-control${invalid(errors?.latitude)}`}
                  value={form.latitude}
                  onChange={(e) => setField("latitude", e.target.value)}
                />
              </div>
              <div className="col-6">
                <label className="form-label" htmlFor="nLng">
                  Longitude
                </label>
                <input
                  id="nLng"
                  type="number"
                  step="any"
                  className={`form-control${invalid(errors?.longitude)}`}
                  value={form.longitude}
                  onChange={(e) => setField("longitude", e.target.value)}
                />
              </div>
            </div>
            <div className="row g-3">
              <div className="col-6">
                <label className="form-label" htmlFor="nCapacity">
                  Capacity (kWh)
                </label>
                <input
                  id="nCapacity"
                  type="number"
                  step="any"
                  min="0.1"
                  className={`form-control${invalid(errors?.capacityKWh)}`}
                  value={form.capacityKWh}
                  onChange={(e) => setField("capacityKWh", e.target.value)}
                />
              </div>
              <div className="col-6">
                <label className="form-label" htmlFor="nSlots">
                  Battery slots
                </label>
                <input
                  id="nSlots"
                  type="number"
                  step="1"
                  min="1"
                  className={`form-control${invalid(errors?.batterySlots)}`}
                  value={form.batterySlots}
                  onChange={(e) => setField("batterySlots", e.target.value)}
                />
              </div>
            </div>
            <div className="row g-3 mt-0">
              <div className="col-6">
                <label className="form-label" htmlFor="nOpen">
                  Opens
                </label>
                <input
                  id="nOpen"
                  type="time"
                  className={`form-control${invalid(errors?.schedule)}`}
                  value={form.openTime}
                  onChange={(e) => setField("openTime", e.target.value)}
                />
              </div>
              <div className="col-6">
                <label className="form-label" htmlFor="nClose">
                  Closes
                </label>
                <input
                  id="nClose"
                  type="time"
                  className={`form-control${invalid(errors?.schedule)}`}
                  value={form.closeTime}
                  onChange={(e) => setField("closeTime", e.target.value)}
                />
              </div>
              <div className="col-12 form-text mt-1">
                Operating schedule in Sri Lanka time. Leave both empty for a 24-hour node; bookings outside these
                hours are refused.
              </div>
            </div>
          </>
        )}
      </Modal>

      <Modal
        show={slotsNode !== null}
        title="Update battery slots"
        size="sm"
        onClose={() => setSlotsNode(null)}
        onSubmit={saveSlots}
        footer={
          <>
            <button type="button" className="btn btn-outline-secondary" onClick={() => setSlotsNode(null)}>
              Cancel
            </button>
            <BusyButton type="submit" className="btn btn-primary" busy={saving}>
              Save
            </BusyButton>
          </>
        }
      >
        {slotsNode && (
          <>
            <p className="small text-body-secondary mb-3">{slotsNode.name}</p>
            <label className="form-label" htmlFor="sSlots">
              Available battery slots
            </label>
            <input
              id="sSlots"
              type="number"
              min="1"
              step="1"
              className="form-control"
              value={slotsValue}
              onChange={(e) => setSlotsValue(e.target.value)}
              autoFocus
            />
          </>
        )}
      </Modal>
    </>
  );
}

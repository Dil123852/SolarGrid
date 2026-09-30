/*
 * File: NodesPage.tsx
 * Purpose: Microgrid node management - list/filter nodes; Backoffice can create, edit, activate
 *          and deactivate (the API refuses deactivation while bookings are live).
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

type ActiveFilter = "" | "true" | "false";

// Form fields are kept as strings so inputs can be empty while typing.
interface NodeForm {
  id: string | null;
  name: string;
  latitude: string;
  longitude: string;
  capacityKWh: string;
  batterySlots: string;
}

const EMPTY_FORM: NodeForm = { id: null, name: "", latitude: "", longitude: "", capacityKWh: "", batterySlots: "" };

function toForm(node: MicrogridNode): NodeForm {
  return {
    id: node.id,
    name: node.name,
    latitude: String(node.latitude),
    longitude: String(node.longitude),
    capacityKWh: String(node.capacityKWh),
    batterySlots: String(node.batterySlots),
  };
}

export function NodesPage() {
  const { hasRole } = useAuth();
  const canEdit = hasRole("Backoffice");
  const toast = useToast();
  const [filter, setFilter] = useState<ActiveFilter>("");
  const { data: nodes, loading, error, reload } = useApiData(
    () => nodesApi.list(filter === "" ? undefined : filter === "true"),
    [filter],
  );
  const [form, setForm] = useState<NodeForm | null>(null);
  const [validated, setValidated] = useState(false);
  const [saving, setSaving] = useState(false);

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

  const invalid = (bad: boolean | undefined) => (validated && bad ? " is-invalid" : "");
  const columns = canEdit ? 6 : 5;

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
                <i className="bi bi-plus-lg me-1" />
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
                <th>Status</th>
                {canEdit && <th className="text-end">Actions</th>}
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
                        {n.latitude.toFixed(5)}, {n.longitude.toFixed(5)} <i className="bi bi-box-arrow-up-right small" />
                      </a>
                    </td>
                    <td className="text-end">{n.capacityKWh}</td>
                    <td className="text-end">{n.batterySlots}</td>
                    <td>
                      <ActiveBadge active={n.isActive} />
                    </td>
                    {canEdit && (
                      <td className="text-end text-nowrap">
                        <button className="btn btn-sm btn-outline-primary me-1" onClick={() => openForm(n)}>
                          <i className="bi bi-pencil" /> Edit
                        </button>
                        <BusyButton
                          className={`btn btn-sm ${n.isActive ? "btn-outline-danger" : "btn-outline-success"}`}
                          onClick={() => toggle(n)}
                        >
                          {n.isActive ? "Deactivate" : "Activate"}
                        </BusyButton>
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
          </>
        )}
      </Modal>
    </>
  );
}

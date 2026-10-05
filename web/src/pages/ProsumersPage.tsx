/*
 * File: ProsumersPage.tsx
 * Purpose: Prosumer management for Backoffice - create and edit prosumer profiles (NIC is the
 *          key), deactivate them, and reactivate accounts from the Pending Activation list.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

import { useState } from "react";
import { CiCircleCheck, CiCirclePlus, CiEdit, CiTimer } from "react-icons/ci";
import { prosumersApi } from "../api/endpoints";
import type { Prosumer } from "../api/types";
import { ActiveBadge, EmptyRow, LoadingRow } from "../components/Badges";
import { BusyButton } from "../components/BusyButton";
import { Modal } from "../components/Modal";
import { PageHeader } from "../components/PageHeader";
import { useToast } from "../components/Toasts";
import { useApiData } from "../hooks/useApiData";
import { formatDateTime } from "../utils/format";

type Tab = "pending" | "all";

// One form for both create (NIC + password required) and edit (NIC fixed, no password).
interface ProsumerForm {
  editingNic: string | null;
  nic: string;
  name: string;
  email: string;
  phone: string;
  address: string;
  password: string;
}

const EMPTY_FORM: ProsumerForm = { editingNic: null, nic: "", name: "", email: "", phone: "", address: "", password: "" };

// Sri Lankan NIC: 9 digits + V/X, or 12 digits (the API applies the same rule).
const NIC_PATTERN = /^(\d{9}[VvXx]|\d{12})$/;

export function ProsumersPage() {
  const toast = useToast();
  const [tab, setTab] = useState<Tab>("all");
  const { data, loading, error, reload } = useApiData(async () => {
    const [list, pending] = await Promise.all([
      prosumersApi.list(tab === "pending" ? false : undefined),
      tab === "pending" ? Promise.resolve(null) : prosumersApi.list(false),
    ]);
    return { list, pendingCount: (pending ?? list).length };
  }, [tab]);

  const [form, setForm] = useState<ProsumerForm | null>(null);
  const [validated, setValidated] = useState(false);
  const [saving, setSaving] = useState(false);

  const openForm = (p?: Prosumer) => {
    setValidated(false);
    setForm(
      p
        ? { editingNic: p.nic, nic: p.nic, name: p.name, email: p.email, phone: p.phone, address: p.address, password: "" }
        : EMPTY_FORM,
    );
  };

  const setField = (key: keyof ProsumerForm, value: string) => setForm((f) => (f ? { ...f, [key]: value } : f));

  const isNew = form !== null && form.editingNic === null;
  const errors = form && {
    nic: isNew && !NIC_PATTERN.test(form.nic.trim()),
    name: !form.name.trim(),
    email: !/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(form.email.trim()),
    phone: !form.phone.trim(),
    password: isNew && form.password.length < 6,
  };

  const save = async () => {
    if (!form || !errors) return;
    setValidated(true);
    if (Object.values(errors).some(Boolean)) return;
    const profile = {
      name: form.name.trim(),
      email: form.email.trim(),
      phone: form.phone.trim(),
      address: form.address.trim(),
    };
    setSaving(true);
    try {
      if (form.editingNic) {
        await prosumersApi.update(form.editingNic, profile);
        toast.success(`Profile for ${form.editingNic} updated.`);
      } else {
        await prosumersApi.create({ ...profile, nic: form.nic.trim(), password: form.password });
        toast.success(`Prosumer ${form.nic.trim().toUpperCase()} created.`);
      }
      setForm(null);
      await reload();
    } catch (err) {
      toast.error(err);
    } finally {
      setSaving(false);
    }
  };

  const toggle = async (p: Prosumer) => {
    try {
      const result = p.isActive ? await prosumersApi.deactivate(p.nic) : await prosumersApi.reactivate(p.nic);
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
        eyebrow="Accounts"
        title="Prosumers"
        subtitle="Solar property owners, keyed by NIC. Deactivated accounts can only be reactivated by Backoffice."
        actions={
          <button className="btn btn-primary" onClick={() => openForm()}>
            <CiCirclePlus className="me-1" />
            New prosumer
          </button>
        }
      />

      <ul className="nav nav-pills mb-3">
        <li className="nav-item">
          <button className={`nav-link${tab === "all" ? " active" : ""}`} onClick={() => setTab("all")}>
            All prosumers
          </button>
        </li>
        <li className="nav-item">
          <button className={`nav-link${tab === "pending" ? " active" : ""}`} onClick={() => setTab("pending")}>
            <CiTimer className="me-1" />
            Pending activation <span className="badge text-bg-warning ms-1">{data?.pendingCount ?? 0}</span>
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
                    <td className="text-end text-nowrap">
                      <div className="d-inline-flex gap-1">
                        <button className="btn btn-sm btn-outline-primary" onClick={() => openForm(p)}>
                          <CiEdit /> Edit
                        </button>
                        <BusyButton
                          className={`btn btn-sm ${p.isActive ? "btn-outline-danger" : "btn-success"}`}
                          onClick={() => toggle(p)}
                        >
                          {!p.isActive && <CiCircleCheck className="me-1" />}
                          {p.isActive ? "Deactivate" : "Reactivate"}
                        </BusyButton>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      <Modal
        show={form !== null}
        title={isNew ? "New prosumer" : "Edit prosumer"}
        onClose={() => setForm(null)}
        onSubmit={save}
        footer={
          <>
            <button type="button" className="btn btn-outline-secondary" onClick={() => setForm(null)}>
              Cancel
            </button>
            <BusyButton type="submit" className="btn btn-primary" busy={saving}>
              {isNew ? "Create prosumer" : "Save changes"}
            </BusyButton>
          </>
        }
      >
        {form && (
          <>
            <div className="mb-3">
              <label className="form-label" htmlFor="pNic">
                NIC (account ID)
              </label>
              <input
                id="pNic"
                name="new-prosumer-nic"
                autoComplete="off"
                className={`form-control${invalid(errors?.nic)}`}
                value={form.nic}
                onChange={(e) => setField("nic", e.target.value)}
                disabled={!isNew}
                placeholder="200012345678 or 991234567V"
                autoFocus={isNew}
              />
              {isNew && <div className="form-text">9 digits + V/X, or 12 digits. It cannot be changed later.</div>}
            </div>
            <div className="mb-3">
              <label className="form-label" htmlFor="pName">
                Full name
              </label>
              <input
                id="pName"
                autoComplete="off"
                className={`form-control${invalid(errors?.name)}`}
                value={form.name}
                onChange={(e) => setField("name", e.target.value)}
                autoFocus={!isNew}
              />
            </div>
            <div className="row g-3 mb-3">
              <div className="col-md-6">
                <label className="form-label" htmlFor="pEmail">
                  Email
                </label>
                <input
                  id="pEmail"
                  type="email"
                  autoComplete="off"
                  className={`form-control${invalid(errors?.email)}`}
                  value={form.email}
                  onChange={(e) => setField("email", e.target.value)}
                />
              </div>
              <div className="col-md-6">
                <label className="form-label" htmlFor="pPhone">
                  Phone
                </label>
                <input
                  id="pPhone"
                  autoComplete="off"
                  className={`form-control${invalid(errors?.phone)}`}
                  value={form.phone}
                  onChange={(e) => setField("phone", e.target.value)}
                />
              </div>
            </div>
            <div className="mb-3">
              <label className="form-label" htmlFor="pAddress">
                Address
              </label>
              <input
                id="pAddress"
                autoComplete="off"
                className="form-control"
                value={form.address}
                onChange={(e) => setField("address", e.target.value)}
              />
            </div>
            {isNew && (
              <div className="mb-1">
                <label className="form-label" htmlFor="pPassword">
                  Initial password
                </label>
                <input
                  id="pPassword"
                  name="new-prosumer-password"
                  type="password"
                  autoComplete="new-password"
                  className={`form-control${invalid(errors?.password)}`}
                  value={form.password}
                  onChange={(e) => setField("password", e.target.value)}
                />
                <div className="form-text">At least 6 characters. The prosumer signs in to the mobile app with their NIC.</div>
              </div>
            )}
          </>
        )}
      </Modal>
    </>
  );
}

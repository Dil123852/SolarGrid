/*
 * File: UsersPage.tsx
 * Purpose: Backoffice staff user management - list, create, enable and disable staff accounts.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

import { useState } from "react";
import { usersApi } from "../api/endpoints";
import type { RegisterStaffRequest, StaffRole, StaffUser } from "../api/types";
import { useAuth } from "../auth/AuthContext";
import { ActiveBadge, EmptyRow, LoadingRow } from "../components/Badges";
import { BusyButton } from "../components/BusyButton";
import { Modal } from "../components/Modal";
import { PageHeader } from "../components/PageHeader";
import { useToast } from "../components/Toasts";
import { useApiData } from "../hooks/useApiData";
import { formatDateTime, roleLabel } from "../utils/format";
import { CiCirclePlus } from "react-icons/ci";

const EMPTY_FORM: RegisterStaffRequest = { username: "", email: "", password: "", role: "GridOperator" };

export function UsersPage() {
  const { session } = useAuth();
  const toast = useToast();
  const { data: users, loading, error, reload } = useApiData(() => usersApi.list(), []);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState(EMPTY_FORM);
  const [validated, setValidated] = useState(false);
  const [saving, setSaving] = useState(false);

  const setField = <K extends keyof RegisterStaffRequest>(key: K, value: RegisterStaffRequest[K]) =>
    setForm((f) => ({ ...f, [key]: value }));

  const closeForm = () => {
    setShowForm(false);
    setForm(EMPTY_FORM);
    setValidated(false);
  };

  const create = async () => {
    setValidated(true);
    if (!form.username.trim() || !form.email.trim() || form.password.length < 6) return;
    setSaving(true);
    try {
      await usersApi.create({ ...form, username: form.username.trim(), email: form.email.trim() });
      toast.success(`User "${form.username.trim()}" created.`);
      closeForm();
      await reload();
    } catch (err) {
      toast.error(err);
    } finally {
      setSaving(false);
    }
  };

  const toggle = async (user: StaffUser) => {
    try {
      const result = user.isActive ? await usersApi.deactivate(user.id) : await usersApi.activate(user.id);
      toast.success(result.message);
      await reload();
    } catch (err) {
      toast.error(err);
    }
  };

  const invalid = (bad: boolean) => (validated && bad ? " is-invalid" : "");

  return (
    <>
      <PageHeader
        eyebrow="Administration"
        title="Staff Users"
        subtitle="Backoffice officers and Grid Operators who can use this portal and operator mode."
        actions={
          <button className="btn btn-primary" onClick={() => setShowForm(true)}>
            <CiCirclePlus className="me-1" />
            New user
          </button>
        }
      />

      {error && <div className="alert alert-danger">{error}</div>}

      <div className="card sg-card">
        <div className="table-responsive">
          <table className="table table-hover mb-0">
            <thead className="table-light">
              <tr>
                <th>Username</th>
                <th>Email</th>
                <th>Role</th>
                <th>Status</th>
                <th>Created</th>
                <th className="text-end">Actions</th>
              </tr>
            </thead>
            <tbody>
              {loading && !users ? (
                <LoadingRow colSpan={6} />
              ) : !users?.length ? (
                <EmptyRow colSpan={6} text="No staff users yet." />
              ) : (
                users.map((u) => {
                  const isSelf = u.username === session?.displayName;
                  return (
                    <tr key={u.id}>
                      <td className="fw-medium">{u.username}</td>
                      <td>{u.email}</td>
                      <td>{roleLabel(u.role)}</td>
                      <td>
                        <ActiveBadge active={u.isActive} />
                      </td>
                      <td>{formatDateTime(u.createdAt)}</td>
                      <td className="text-end">
                        <BusyButton
                          className={`btn btn-sm ${u.isActive ? "btn-outline-danger" : "btn-outline-success"}`}
                          disabled={isSelf}
                          title={isSelf ? "You cannot disable yourself" : undefined}
                          onClick={() => toggle(u)}
                        >
                          {u.isActive ? "Disable" : "Enable"}
                        </BusyButton>
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </div>

      <Modal
        show={showForm}
        title="Create staff user"
        onClose={closeForm}
        onSubmit={create}
        footer={
          <>
            <button type="button" className="btn btn-outline-secondary" onClick={closeForm}>
              Cancel
            </button>
            <BusyButton type="submit" className="btn btn-primary" busy={saving}>
              Create user
            </BusyButton>
          </>
        }
      >
        <div className="mb-3">
          <label className="form-label" htmlFor="uUsername">
            Username
          </label>
          {/* Unique names + autocomplete hints stop password managers filling in the admin's own login. */}
          <input
            id="uUsername"
            name="new-staff-username"
            autoComplete="off"
            autoCapitalize="none"
            spellCheck={false}
            className={`form-control${invalid(!form.username.trim())}`}
            value={form.username}
            onChange={(e) => setField("username", e.target.value)}
            autoFocus
          />
        </div>
        <div className="mb-3">
          <label className="form-label" htmlFor="uEmail">
            Email
          </label>
          <input
            id="uEmail"
            name="new-staff-email"
            type="email"
            autoComplete="off"
            className={`form-control${invalid(!form.email.trim())}`}
            value={form.email}
            onChange={(e) => setField("email", e.target.value)}
          />
        </div>
        <div className="mb-3">
          <label className="form-label" htmlFor="uPassword">
            Password
          </label>
          <input
            id="uPassword"
            name="new-staff-password"
            type="password"
            autoComplete="new-password"
            className={`form-control${invalid(form.password.length < 6)}`}
            value={form.password}
            onChange={(e) => setField("password", e.target.value)}
          />
          <div className="form-text">At least 6 characters.</div>
        </div>
        <div className="mb-1">
          <label className="form-label" htmlFor="uRole">
            Role
          </label>
          <select
            id="uRole"
            className="form-select"
            value={form.role}
            onChange={(e) => setField("role", e.target.value as StaffRole)}
          >
            <option value="GridOperator">Grid Operator</option>
            <option value="Backoffice">Backoffice</option>
          </select>
        </div>
      </Modal>
    </>
  );
}

/*
 * File: layout.js
 * Purpose: Shared UI shell and helpers - injects the role-aware navbar and toast container on
 *          every page, plus formatting/escaping helpers used by the page controllers.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

window.SG = window.SG || {};

SG.ui = (function () {
  const NAV = [
    { href: 'dashboard.html', label: 'Dashboard', icon: 'bi-speedometer2', roles: ['Backoffice', 'GridOperator'] },
    { href: 'bookings.html', label: 'Bookings', icon: 'bi-calendar-check', roles: ['Backoffice', 'GridOperator'] },
    { href: 'nodes.html', label: 'Microgrid Nodes', icon: 'bi-lightning-charge', roles: ['Backoffice', 'GridOperator'] },
    { href: 'prosumers.html', label: 'Prosumers', icon: 'bi-house-gear', roles: ['Backoffice'] },
    { href: 'users.html', label: 'Staff Users', icon: 'bi-people', roles: ['Backoffice'] }
  ];

  function escape(value) {
    return String(value === undefined || value === null ? '' : value)
      .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;').replace(/'/g, '&#39;');
  }

  function renderNavbar(session) {
    const current = location.pathname.split('/').pop() || 'index.html';
    const links = NAV.filter(function (n) { return n.roles.indexOf(session.role) !== -1; })
      .map(function (n) {
        const active = n.href === current ? ' active" aria-current="page' : '';
        return '<li class="nav-item"><a class="nav-link' + active + '" href="' + n.href + '">' +
          '<i class="bi ' + n.icon + ' me-1"></i>' + n.label + '</a></li>';
      }).join('');

    const nav = document.createElement('nav');
    nav.className = 'navbar navbar-expand-lg navbar-dark sg-navbar mb-4';
    nav.innerHTML =
      '<div class="container">' +
      '  <a class="navbar-brand fw-semibold" href="dashboard.html"><i class="bi bi-sun-fill text-warning me-2"></i>SolarGrid</a>' +
      '  <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#sgNav" aria-controls="sgNav" aria-expanded="false" aria-label="Toggle navigation">' +
      '    <span class="navbar-toggler-icon"></span></button>' +
      '  <div class="collapse navbar-collapse" id="sgNav">' +
      '    <ul class="navbar-nav me-auto">' + links + '</ul>' +
      '    <span class="navbar-text me-3 small"><i class="bi bi-person-circle me-1"></i>' + escape(session.displayName) +
      '      <span class="badge rounded-pill text-bg-light ms-1">' + escape(roleLabel(session.role)) + '</span></span>' +
      '    <button class="btn btn-outline-light btn-sm" id="sgLogout"><i class="bi bi-box-arrow-right me-1"></i>Sign out</button>' +
      '  </div>' +
      '</div>';
    document.body.prepend(nav);
    document.getElementById('sgLogout').addEventListener('click', function () { SG.auth.logout(); });
  }

  function ensureToastHost() {
    let host = document.getElementById('sgToasts');
    if (!host) {
      host = document.createElement('div');
      host.id = 'sgToasts';
      host.className = 'toast-container position-fixed bottom-0 end-0 p-3';
      document.body.appendChild(host);
    }
    return host;
  }

  // type: 'success' | 'danger' | 'warning' | 'info'
  function toast(message, type) {
    const el = document.createElement('div');
    el.className = 'toast align-items-center border-0 text-bg-' + (type || 'success');
    el.setAttribute('role', 'alert');
    el.innerHTML = '<div class="d-flex"><div class="toast-body">' + escape(message) + '</div>' +
      '<button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button></div>';
    ensureToastHost().appendChild(el);
    const t = new bootstrap.Toast(el, { delay: 4500 });
    el.addEventListener('hidden.bs.toast', function () { el.remove(); });
    t.show();
  }

  function error(err) { toast(err && err.message ? err.message : String(err), 'danger'); }

  function roleLabel(role) { return role === 'GridOperator' ? 'Grid Operator' : role; }

  function formatDateTime(iso) {
    if (!iso) return '';
    return new Date(iso).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' });
  }

  // Value for <input type="datetime-local"> in the browser's local time.
  function toLocalInput(date) {
    const d = new Date(date);
    d.setMinutes(d.getMinutes() - d.getTimezoneOffset());
    return d.toISOString().slice(0, 16);
  }

  // Converts a datetime-local value to a UTC ISO string for the API.
  function fromLocalInput(value) { return value ? new Date(value).toISOString() : null; }

  const STATUS_CLASS = { Pending: 'warning', Approved: 'success', Cancelled: 'secondary', Completed: 'primary' };
  function statusBadge(status) {
    return '<span class="badge text-bg-' + (STATUS_CLASS[status] || 'light') + '">' + escape(status) + '</span>';
  }

  function activeBadge(isActive) {
    return isActive
      ? '<span class="badge text-bg-success">Active</span>'
      : '<span class="badge text-bg-secondary">Inactive</span>';
  }

  // Disables a button and shows a spinner while an async action runs.
  async function busy(button, action) {
    const html = button.innerHTML;
    button.disabled = true;
    button.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>' + html;
    try {
      return await action();
    } finally {
      button.disabled = false;
      button.innerHTML = html;
    }
  }

  function emptyRow(colspan, text) {
    return '<tr><td colspan="' + colspan + '" class="text-center text-body-secondary py-4">' + escape(text) + '</td></tr>';
  }

  // Guards the page, draws the shell, and returns the session (or null if redirected).
  function initPage(roles) {
    const session = SG.auth.requireRole(roles);
    if (!session) return null;
    renderNavbar(session);
    document.querySelectorAll('[data-role]').forEach(function (el) {
      if (el.getAttribute('data-role').split(',').indexOf(session.role) === -1) el.remove();
    });
    return session;
  }

  return {
    initPage: initPage,
    toast: toast,
    error: error,
    escape: escape,
    busy: busy,
    emptyRow: emptyRow,
    formatDateTime: formatDateTime,
    toLocalInput: toLocalInput,
    fromLocalInput: fromLocalInput,
    statusBadge: statusBadge,
    activeBadge: activeBadge,
    roleLabel: roleLabel
  };
})();

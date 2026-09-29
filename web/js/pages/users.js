/*
 * File: users.js
 * Purpose: Staff user management controller - list, create, enable and disable staff accounts.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

(function () {
  const session = SG.ui.initPage(['Backoffice']);
  if (!session) return;

  const rows = document.getElementById('userRows');
  const form = document.getElementById('userForm');
  const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById('userModal'));

  async function load() {
    try {
      const users = await SG.services.users.list();
      rows.innerHTML = users.length ? users.map(render).join('') : SG.ui.emptyRow(6, 'No staff users yet.');
    } catch (err) {
      SG.ui.error(err);
    }
  }

  function render(u) {
    const self = u.username === session.displayName;
    const action = u.isActive
      ? '<button class="btn btn-sm btn-outline-danger" data-action="deactivate" data-id="' + u.id + '"' + (self ? ' disabled title="You cannot disable yourself"' : '') + '>Disable</button>'
      : '<button class="btn btn-sm btn-outline-success" data-action="activate" data-id="' + u.id + '">Enable</button>';
    return '<tr><td class="fw-medium">' + SG.ui.escape(u.username) + '</td><td>' + SG.ui.escape(u.email) + '</td>' +
      '<td>' + SG.ui.escape(SG.ui.roleLabel(u.role)) + '</td><td>' + SG.ui.activeBadge(u.isActive) + '</td>' +
      '<td>' + SG.ui.formatDateTime(u.createdAt) + '</td><td class="text-end">' + action + '</td></tr>';
  }

  rows.addEventListener('click', async function (e) {
    const btn = e.target.closest('button[data-action]');
    if (!btn) return;
    try {
      const result = await SG.ui.busy(btn, function () {
        return SG.services.users[btn.dataset.action](btn.dataset.id);
      });
      SG.ui.toast(result.message);
      load();
    } catch (err) {
      SG.ui.error(err);
    }
  });

  form.addEventListener('submit', async function (e) {
    e.preventDefault();
    if (!form.checkValidity()) {
      form.classList.add('was-validated');
      return;
    }
    const user = {
      username: document.getElementById('uUsername').value.trim(),
      email: document.getElementById('uEmail').value.trim(),
      password: document.getElementById('uPassword').value,
      role: document.getElementById('uRole').value
    };
    try {
      await SG.ui.busy(document.getElementById('userSaveBtn'), function () { return SG.services.users.create(user); });
      SG.ui.toast('User "' + user.username + '" created.');
      modal.hide();
      form.reset();
      form.classList.remove('was-validated');
      load();
    } catch (err) {
      SG.ui.error(err);
    }
  });

  load();
})();

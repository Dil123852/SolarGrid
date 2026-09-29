/*
 * File: prosumers.js
 * Purpose: Prosumer account controller - the Pending Activation list (deactivated accounts)
 *          with Backoffice reactivation, and the full prosumer list.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

(function () {
  const session = SG.ui.initPage(['Backoffice']);
  if (!session) return;

  const rows = document.getElementById('prosumerRows');
  const tabs = document.getElementById('tabs');
  let activeFilter = 'false';

  async function load() {
    try {
      const [list, pending] = await Promise.all([
        SG.services.prosumers.list(activeFilter),
        activeFilter === 'false' ? null : SG.services.prosumers.list('false')
      ]);
      document.getElementById('pendingBadge').textContent = (pending || list).length;
      rows.innerHTML = list.length
        ? list.map(render).join('')
        : SG.ui.emptyRow(7, activeFilter === 'false' ? 'No accounts are waiting for activation.' : 'No prosumers registered yet.');
    } catch (err) {
      SG.ui.error(err);
    }
  }

  function render(p) {
    const button = p.isActive
      ? '<button class="btn btn-sm btn-outline-danger" data-action="deactivate" data-nic="' + SG.ui.escape(p.nic) + '">Deactivate</button>'
      : '<button class="btn btn-sm btn-success" data-action="reactivate" data-nic="' + SG.ui.escape(p.nic) + '"><i class="bi bi-person-check me-1"></i>Reactivate</button>';
    return '<tr><td class="fw-medium">' + SG.ui.escape(p.nic) + '</td><td>' + SG.ui.escape(p.name) + '</td>' +
      '<td>' + SG.ui.escape(p.email) + '</td><td>' + SG.ui.escape(p.phone) + '</td><td>' + SG.ui.activeBadge(p.isActive) + '</td>' +
      '<td class="small text-body-secondary">' + SG.ui.formatDateTime(p.createdAt) + '</td><td class="text-end">' + button + '</td></tr>';
  }

  tabs.addEventListener('click', function (e) {
    const btn = e.target.closest('button[data-active]');
    if (!btn) return;
    tabs.querySelectorAll('.nav-link').forEach(function (b) { b.classList.remove('active'); });
    btn.classList.add('active');
    activeFilter = btn.dataset.active;
    load();
  });

  rows.addEventListener('click', async function (e) {
    const btn = e.target.closest('button[data-action]');
    if (!btn) return;
    try {
      const result = await SG.ui.busy(btn, function () { return SG.services.prosumers[btn.dataset.action](btn.dataset.nic); });
      SG.ui.toast(result.message);
      load();
    } catch (err) {
      SG.ui.error(err);
    }
  });

  load();
})();

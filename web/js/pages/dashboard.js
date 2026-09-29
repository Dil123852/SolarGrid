/*
 * File: dashboard.js
 * Purpose: Dashboard controller - live counts from /api/dashboard and the next pending bookings.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

(function () {
  const session = SG.ui.initPage(['Backoffice', 'GridOperator']);
  if (!session) return;

  document.getElementById('greeting').textContent =
    'Signed in as ' + session.displayName + ' (' + SG.ui.roleLabel(session.role) + ')';

  async function load() {
    try {
      const [counts, pending] = await Promise.all([
        SG.services.dashboard.get(),
        SG.services.reservations.list({ status: 'Pending', from: new Date().toISOString() })
      ]);
      document.getElementById('pendingCount').textContent = counts.pendingCount;
      document.getElementById('approvedCount').textContent = counts.approvedFutureCount;
      document.getElementById('completedCount').textContent = counts.completedCount;

      // API returns newest slot first; show the soonest five.
      const next = pending.slice().reverse().slice(0, 5);
      document.getElementById('pendingRows').innerHTML = next.length
        ? next.map(function (r) {
            return '<tr><td>' + SG.ui.formatDateTime(r.slotTime) + '</td><td>' + SG.ui.escape(r.nodeName) +
              '</td><td>' + SG.ui.escape(r.prosumerNic) + '</td><td>' + SG.ui.statusBadge(r.status) + '</td></tr>';
          }).join('')
        : SG.ui.emptyRow(4, 'No upcoming reservations are waiting for approval.');
    } catch (err) {
      SG.ui.error(err);
    }
  }

  document.getElementById('refreshBtn').addEventListener('click', load);
  load();
})();

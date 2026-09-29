/*
 * File: bookings.js
 * Purpose: Slot booking management controller - filtered booking list for staff; Backoffice can
 *          create, reschedule, cancel and approve bookings and show the approved QR token.
 *          All window/notice/capacity rules are enforced by the API; errors are shown as returned.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

(function () {
  const session = SG.ui.initPage(['Backoffice', 'GridOperator']);
  if (!session) return;

  const canEdit = SG.auth.isBackoffice();
  const rows = document.getElementById('bookingRows');
  const filterForm = document.getElementById('filterForm');
  const bookingForm = document.getElementById('bookingForm');
  const bookingModal = bootstrap.Modal.getOrCreateInstance(document.getElementById('bookingModal'));
  const qrModal = bootstrap.Modal.getOrCreateInstance(document.getElementById('qrModal'));
  let bookings = [];
  let nodes = [];

  // Pre-select a status passed from the dashboard link (?status=Pending).
  const initialStatus = new URLSearchParams(location.search).get('status');
  if (initialStatus) document.getElementById('fStatus').value = initialStatus;

  function currentFilter() {
    const from = document.getElementById('fFrom').value;
    const to = document.getElementById('fTo').value;
    return {
      status: document.getElementById('fStatus').value,
      nodeId: document.getElementById('fNode').value,
      nic: document.getElementById('fNic').value.trim(),
      from: from ? new Date(from + 'T00:00').toISOString() : '',
      to: to ? new Date(to + 'T23:59:59').toISOString() : ''
    };
  }

  async function loadNodes() {
    try {
      nodes = await SG.services.nodes.list();
      const options = nodes.map(function (n) {
        return '<option value="' + n.id + '">' + SG.ui.escape(n.name) + (n.isActive ? '' : ' (inactive)') + '</option>';
      }).join('');
      document.getElementById('fNode').insertAdjacentHTML('beforeend', options);
    } catch (err) {
      SG.ui.error(err);
    }
  }

  async function load() {
    try {
      bookings = await SG.services.reservations.list(currentFilter());
      rows.innerHTML = bookings.length ? bookings.map(render).join('') : SG.ui.emptyRow(6, 'No bookings match these filters.');
      document.getElementById('bookingCount').textContent = bookings.length + ' booking' + (bookings.length === 1 ? '' : 's');
    } catch (err) {
      SG.ui.error(err);
    }
  }

  function render(r) {
    const live = r.status === 'Pending' || r.status === 'Approved';
    const buttons = [];
    if (r.qrToken) buttons.push(action('qr', r.id, 'bi-qr-code', 'QR', 'secondary'));
    if (canEdit && r.status === 'Pending') buttons.push(action('approve', r.id, 'bi-check2-circle', 'Approve', 'success'));
    if (canEdit && live) {
      buttons.push(action('edit', r.id, 'bi-pencil', 'Reschedule', 'primary'));
      buttons.push(action('cancel', r.id, 'bi-x-circle', 'Cancel', 'danger'));
    }
    return '<tr><td class="fw-medium text-nowrap">' + SG.ui.formatDateTime(r.slotTime) + '</td>' +
      '<td>' + SG.ui.escape(r.nodeName) + '</td><td>' + SG.ui.escape(r.prosumerNic) + '</td>' +
      '<td>' + SG.ui.statusBadge(r.status) + '</td><td class="small text-body-secondary">' + SG.ui.formatDateTime(r.createdAt) + '</td>' +
      '<td class="text-end text-nowrap">' + buttons.join(' ') + '</td></tr>';
  }

  function action(name, id, icon, label, color) {
    return '<button class="btn btn-sm btn-outline-' + color + '" data-action="' + name + '" data-id="' + id + '">' +
      '<i class="bi ' + icon + '"></i> ' + label + '</button>';
  }

  function fillNodeSelect(selectedId) {
    document.getElementById('bNode').innerHTML = nodes
      .filter(function (n) { return n.isActive || n.id === selectedId; })
      .map(function (n) {
        return '<option value="' + n.id + '"' + (n.id === selectedId ? ' selected' : '') + '>' +
          SG.ui.escape(n.name) + ' · ' + n.batterySlots + ' slots</option>';
      }).join('');
  }

  function openBookingForm(booking) {
    bookingForm.reset();
    bookingForm.classList.remove('was-validated');
    const slot = document.getElementById('bSlot');
    slot.min = SG.ui.toLocalInput(new Date());
    slot.max = SG.ui.toLocalInput(new Date(Date.now() + 7 * 24 * 3600 * 1000));

    document.getElementById('bId').value = booking ? booking.id : '';
    document.getElementById('bookingModalTitle').textContent = booking ? 'Reschedule booking' : 'New booking';
    document.getElementById('bNicGroup').classList.toggle('d-none', !!booking);
    document.getElementById('bNic').required = !booking;
    fillNodeSelect(booking ? booking.nodeId : null);
    if (booking) slot.value = SG.ui.toLocalInput(booking.slotTime);
    bookingModal.show();
  }

  function showQr(booking) {
    const canvas = document.getElementById('qrCanvas');
    canvas.innerHTML = '';
    new QRCode(canvas, { text: booking.qrToken, width: 200, height: 200 });
    document.getElementById('qrInfo').textContent = booking.nodeName + ' · ' + SG.ui.formatDateTime(booking.slotTime);
    document.getElementById('qrToken').textContent = booking.qrToken;
    qrModal.show();
  }

  if (canEdit) {
    document.getElementById('newBookingBtn').addEventListener('click', function () { openBookingForm(null); });
  }

  rows.addEventListener('click', async function (e) {
    const btn = e.target.closest('button[data-action]');
    if (!btn) return;
    const booking = bookings.find(function (b) { return b.id === btn.dataset.id; });

    if (btn.dataset.action === 'qr') return showQr(booking);
    if (btn.dataset.action === 'edit') return openBookingForm(booking);

    try {
      if (btn.dataset.action === 'cancel') {
        if (!confirm('Cancel the booking at ' + booking.nodeName + ' on ' + SG.ui.formatDateTime(booking.slotTime) + '?')) return;
        await SG.ui.busy(btn, function () { return SG.services.reservations.cancel(booking.id); });
        SG.ui.toast('Booking cancelled.');
        await load();
      } else if (btn.dataset.action === 'approve') {
        const approved = await SG.ui.busy(btn, function () { return SG.services.reservations.approve(booking.id); });
        SG.ui.toast('Booking approved - QR token issued.');
        await load();
        showQr(approved);
      }
    } catch (err) {
      SG.ui.error(err);
    }
  });

  bookingForm.addEventListener('submit', async function (e) {
    e.preventDefault();
    if (!bookingForm.checkValidity()) {
      bookingForm.classList.add('was-validated');
      return;
    }
    const id = document.getElementById('bId').value;
    const nodeId = document.getElementById('bNode').value;
    const slotTime = SG.ui.fromLocalInput(document.getElementById('bSlot').value);
    try {
      const saved = await SG.ui.busy(document.getElementById('bookingSaveBtn'), function () {
        return id
          ? SG.services.reservations.update(id, { slotTime: slotTime, nodeId: nodeId })
          : SG.services.reservations.create({ nodeId: nodeId, slotTime: slotTime, prosumerNic: document.getElementById('bNic').value.trim() });
      });
      SG.ui.toast((id ? 'Booking rescheduled' : 'Booking created') + ' for ' + saved.nodeName + ' on ' + SG.ui.formatDateTime(saved.slotTime) + '.');
      bookingModal.hide();
      load();
    } catch (err) {
      SG.ui.error(err);
    }
  });

  // Filters apply as soon as they change.
  filterForm.addEventListener('change', load);
  filterForm.addEventListener('submit', function (e) { e.preventDefault(); load(); });
  filterForm.addEventListener('reset', function () { setTimeout(load, 0); });
  let nicTimer;
  document.getElementById('fNic').addEventListener('input', function () {
    clearTimeout(nicTimer);
    nicTimer = setTimeout(load, 400);
  });

  loadNodes().then(load);
})();

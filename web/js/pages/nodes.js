/*
 * File: nodes.js
 * Purpose: Microgrid node management controller - list/filter nodes; Backoffice can create,
 *          edit, activate and deactivate (the API refuses deactivation while bookings are live).
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

(function () {
  const session = SG.ui.initPage(['Backoffice', 'GridOperator']);
  if (!session) return;

  const canEdit = SG.auth.isBackoffice();
  const rows = document.getElementById('nodeRows');
  const form = document.getElementById('nodeForm');
  const filter = document.getElementById('activeFilter');
  const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById('nodeModal'));
  let nodes = [];

  async function load() {
    try {
      nodes = await SG.services.nodes.list(filter.value);
      rows.innerHTML = nodes.length ? nodes.map(render).join('') : SG.ui.emptyRow(canEdit ? 6 : 5, 'No nodes found.');
    } catch (err) {
      SG.ui.error(err);
    }
  }

  function render(n) {
    let actions = '';
    if (canEdit) {
      const toggle = n.isActive
        ? '<button class="btn btn-sm btn-outline-danger" data-action="deactivate" data-id="' + n.id + '">Deactivate</button>'
        : '<button class="btn btn-sm btn-outline-success" data-action="activate" data-id="' + n.id + '">Activate</button>';
      actions = '<td class="text-end text-nowrap"><button class="btn btn-sm btn-outline-primary me-1" data-action="edit" data-id="' + n.id + '">' +
        '<i class="bi bi-pencil"></i> Edit</button>' + toggle + '</td>';
    }
    return '<tr><td class="fw-medium">' + SG.ui.escape(n.name) + '</td>' +
      '<td><a target="_blank" rel="noopener" href="https://www.google.com/maps?q=' + n.latitude + ',' + n.longitude + '">' +
      n.latitude.toFixed(5) + ', ' + n.longitude.toFixed(5) + ' <i class="bi bi-box-arrow-up-right small"></i></a></td>' +
      '<td class="text-end">' + n.capacityKWh + '</td><td class="text-end">' + n.batterySlots + '</td>' +
      '<td>' + SG.ui.activeBadge(n.isActive) + '</td>' + actions + '</tr>';
  }

  function openForm(node) {
    form.reset();
    form.classList.remove('was-validated');
    document.getElementById('nodeModalTitle').textContent = node ? 'Edit node' : 'New node';
    document.getElementById('nId').value = node ? node.id : '';
    if (node) {
      document.getElementById('nName').value = node.name;
      document.getElementById('nLat').value = node.latitude;
      document.getElementById('nLng').value = node.longitude;
      document.getElementById('nCapacity').value = node.capacityKWh;
      document.getElementById('nSlots').value = node.batterySlots;
    }
    modal.show();
  }

  if (canEdit) {
    document.getElementById('newNodeBtn').addEventListener('click', function () { openForm(null); });
  }

  rows.addEventListener('click', async function (e) {
    const btn = e.target.closest('button[data-action]');
    if (!btn) return;
    const id = btn.dataset.id;
    if (btn.dataset.action === 'edit') {
      openForm(nodes.find(function (n) { return n.id === id; }));
      return;
    }
    try {
      const result = await SG.ui.busy(btn, function () { return SG.services.nodes[btn.dataset.action](id); });
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
    const id = document.getElementById('nId').value;
    const node = {
      name: document.getElementById('nName').value.trim(),
      latitude: parseFloat(document.getElementById('nLat').value),
      longitude: parseFloat(document.getElementById('nLng').value),
      capacityKWh: parseFloat(document.getElementById('nCapacity').value),
      batterySlots: parseInt(document.getElementById('nSlots').value, 10)
    };
    try {
      await SG.ui.busy(document.getElementById('nodeSaveBtn'), function () {
        return id ? SG.services.nodes.update(id, node) : SG.services.nodes.create(node);
      });
      SG.ui.toast(id ? 'Node updated.' : 'Node created.');
      modal.hide();
      load();
    } catch (err) {
      SG.ui.error(err);
    }
  });

  filter.addEventListener('change', load);
  load();
})();

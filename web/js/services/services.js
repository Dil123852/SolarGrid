/*
 * File: services.js
 * Purpose: Endpoint wrappers - one object per API resource. Pages call these, never fetch()
 *          directly, so every URL the web app uses is listed in one place.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

window.SG = window.SG || {};

SG.services = {
  users: {
    list: function () { return SG.api.get('/api/users'); },
    create: function (user) { return SG.api.post('/api/auth/register', user); },
    activate: function (id) { return SG.api.put('/api/users/' + encodeURIComponent(id) + '/activate'); },
    deactivate: function (id) { return SG.api.put('/api/users/' + encodeURIComponent(id) + '/deactivate'); }
  },

  nodes: {
    list: function (active) { return SG.api.get('/api/nodes', { active: active }); },
    create: function (node) { return SG.api.post('/api/nodes', node); },
    update: function (id, node) { return SG.api.put('/api/nodes/' + encodeURIComponent(id), node); },
    activate: function (id) { return SG.api.put('/api/nodes/' + encodeURIComponent(id) + '/activate'); },
    deactivate: function (id) { return SG.api.put('/api/nodes/' + encodeURIComponent(id) + '/deactivate'); }
  },

  prosumers: {
    list: function (active) { return SG.api.get('/api/prosumers', { active: active }); },
    reactivate: function (nic) { return SG.api.put('/api/prosumers/' + encodeURIComponent(nic) + '/reactivate'); },
    deactivate: function (nic) { return SG.api.put('/api/prosumers/' + encodeURIComponent(nic) + '/deactivate'); }
  },

  reservations: {
    list: function (filter) { return SG.api.get('/api/reservations', filter); },
    create: function (booking) { return SG.api.post('/api/reservations', booking); },
    update: function (id, change) { return SG.api.put('/api/reservations/' + encodeURIComponent(id), change); },
    cancel: function (id) { return SG.api.del('/api/reservations/' + encodeURIComponent(id)); },
    approve: function (id) { return SG.api.post('/api/reservations/' + encodeURIComponent(id) + '/approve'); }
  },

  dashboard: {
    get: function () { return SG.api.get('/api/dashboard'); }
  }
};

/*
 * File: auth.js
 * Purpose: Session handling for staff users - stores the JWT and role in sessionStorage,
 *          guards pages by role, and routes each role to its landing page after login.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

window.SG = window.SG || {};

SG.auth = (function () {
  const KEY = 'sg.session';
  let session = null; // in-memory copy, mirrored to sessionStorage

  function load() {
    if (session) return session;
    try {
      session = JSON.parse(sessionStorage.getItem(KEY) || 'null');
    } catch (e) {
      session = null;
    }
    // Drop sessions whose token has expired.
    if (session && new Date(session.expiresAt) <= new Date()) {
      clear();
    }
    return session;
  }

  function save(s) {
    session = s;
    try { sessionStorage.setItem(KEY, JSON.stringify(s)); } catch (e) { /* private mode: memory only */ }
  }

  function clear() {
    session = null;
    try { sessionStorage.removeItem(KEY); } catch (e) { /* ignore */ }
  }

  async function login(username, password) {
    const result = await SG.api.post('/api/auth/login', { username: username, password: password });
    save(result);
    return result;
  }

  function logout(message) {
    clear();
    if (message) {
      try { sessionStorage.setItem('sg.flash', message); } catch (e) { /* ignore */ }
    }
    window.location.href = 'index.html';
  }

  // Call at the top of every protected page. Redirects away if not signed in or wrong role.
  function requireRole(roles) {
    const s = load();
    if (!s) {
      window.location.href = 'index.html';
      return null;
    }
    if (roles && roles.indexOf(s.role) === -1) {
      window.location.href = 'dashboard.html';
      return null;
    }
    return s;
  }

  return {
    login: login,
    logout: logout,
    requireRole: requireRole,
    session: load,
    getToken: function () { const s = load(); return s ? s.token : null; },
    isBackoffice: function () { const s = load(); return !!s && s.role === 'Backoffice'; },
    landingPage: function () { return 'dashboard.html'; }
  };
})();

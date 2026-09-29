/*
 * File: api.js
 * Purpose: The only place the web app talks HTTP. Adds the JWT bearer header, sends/parses JSON,
 *          logs the user out on 401, and turns API error bodies ({ message }) into thrown Errors
 *          so pages can show the server's business-rule message verbatim.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

window.SG = window.SG || {};

SG.api = (function () {
  async function request(method, path, body) {
    const headers = { 'Accept': 'application/json' };
    const token = SG.auth.getToken();
    if (token) headers['Authorization'] = 'Bearer ' + token;
    if (body !== undefined) headers['Content-Type'] = 'application/json';

    let response;
    try {
      response = await fetch(SG.config.apiBaseUrl + path, {
        method: method,
        headers: headers,
        body: body !== undefined ? JSON.stringify(body) : undefined
      });
    } catch (e) {
      throw new Error('Cannot reach the SolarGrid API at ' + SG.config.apiBaseUrl + '.');
    }

    // Expired/invalid session on a protected call: back to login.
    if (response.status === 401 && token) {
      SG.auth.logout('Your session has expired. Please sign in again.');
      throw new Error('Session expired.');
    }

    const text = await response.text();
    const data = text ? JSON.parse(text) : null;

    if (!response.ok) {
      const fallback = response.status === 403 ? 'You do not have permission to do that.' : 'Request failed (' + response.status + ').';
      throw new Error((data && data.message) || fallback);
    }
    return data;
  }

  // Builds ?a=1&b=2 from an object, skipping empty values.
  function query(params) {
    const parts = Object.keys(params || {})
      .filter(function (k) { return params[k] !== undefined && params[k] !== null && params[k] !== ''; })
      .map(function (k) { return encodeURIComponent(k) + '=' + encodeURIComponent(params[k]); });
    return parts.length ? '?' + parts.join('&') : '';
  }

  return {
    get: function (path, params) { return request('GET', path + query(params)); },
    post: function (path, body) { return request('POST', path, body === undefined ? {} : body); },
    put: function (path, body) { return request('PUT', path, body === undefined ? {} : body); },
    del: function (path) { return request('DELETE', path); }
  };
})();

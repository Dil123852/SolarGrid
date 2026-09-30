/*
 * File: config.js
 * Purpose: Web app configuration - the base URL of the IIS-hosted SolarGrid API (IIS site on port 8081).
 *          Override without editing code: localStorage.setItem('sg.apiBaseUrl', 'http://host:port')
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

window.SG = window.SG || {};

SG.config = {
  apiBaseUrl: (function () {
    try {
      return localStorage.getItem('sg.apiBaseUrl') || 'http://localhost:8081';
    } catch (e) {
      return 'http://localhost:8081';
    }
  })()
};

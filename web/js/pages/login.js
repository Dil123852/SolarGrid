/*
 * File: login.js
 * Purpose: Login page controller - signs staff in via /api/auth/login and routes by role.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

(function () {
  // Already signed in: skip the form.
  if (SG.auth.session()) {
    window.location.href = SG.auth.landingPage();
    return;
  }

  const form = document.getElementById('loginForm');
  const errorBox = document.getElementById('loginError');
  const flash = document.getElementById('flash');

  try {
    const message = sessionStorage.getItem('sg.flash');
    if (message) {
      flash.textContent = message;
      flash.classList.remove('d-none');
      sessionStorage.removeItem('sg.flash');
    }
  } catch (e) { /* ignore */ }

  form.addEventListener('submit', async function (event) {
    event.preventDefault();
    errorBox.classList.add('d-none');

    const username = document.getElementById('username').value.trim();
    const password = document.getElementById('password').value;
    if (!username || !password) {
      errorBox.textContent = 'Enter your username and password.';
      errorBox.classList.remove('d-none');
      return;
    }

    try {
      await SG.ui.busy(document.getElementById('loginBtn'), function () {
        return SG.auth.login(username, password);
      });
      window.location.href = SG.auth.landingPage();
    } catch (err) {
      errorBox.textContent = err.message;
      errorBox.classList.remove('d-none');
    }
  });
})();

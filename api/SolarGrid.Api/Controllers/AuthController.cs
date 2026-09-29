/*
 * File: AuthController.cs
 * Purpose: Login endpoints for staff and prosumers, and Backoffice-only staff registration.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarGrid.Api.Extensions;
using SolarGrid.Api.Security;
using SolarGrid.Application.DTOs;
using SolarGrid.Application.Services;

namespace SolarGrid.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _service;

        public AuthController(AuthService service)
        {
            _service = service;
        }

        // POST /api/auth/login - Backoffice / GridOperator login (web app, operator mode).
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request) =>
            (await _service.LoginStaffAsync(request)).ToActionResult();

        // POST /api/auth/prosumer-login - prosumer login by NIC (mobile app). 403 = pending activation.
        [AllowAnonymous]
        [HttpPost("prosumer-login")]
        public async Task<IActionResult> ProsumerLogin([FromBody] ProsumerLoginRequest request) =>
            (await _service.LoginProsumerAsync(request)).ToActionResult();

        // POST /api/auth/register - creates a Backoffice or GridOperator account.
        [Authorize(Roles = Roles.Backoffice)]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterStaffRequest request) =>
            (await _service.RegisterStaffAsync(request)).ToActionResult();
    }
}

/*
 * File: UsersController.cs
 * Purpose: Backoffice-only staff user management (list, enable, disable).
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarGrid.Api.Extensions;
using SolarGrid.Api.Security;
using SolarGrid.Application.Services;

namespace SolarGrid.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.Backoffice)]
    public class UsersController : ControllerBase
    {
        private readonly AuthService _service;

        public UsersController(AuthService service)
        {
            _service = service;
        }

        // GET /api/users - all staff accounts.
        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetStaffAsync());

        // PUT /api/users/{id}/activate
        [HttpPut("{id}/activate")]
        public async Task<IActionResult> Activate(string id) =>
            (await _service.SetStaffActiveAsync(id, true)).ToActionResult();

        // PUT /api/users/{id}/deactivate
        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(string id) =>
            (await _service.SetStaffActiveAsync(id, false)).ToActionResult();
    }
}

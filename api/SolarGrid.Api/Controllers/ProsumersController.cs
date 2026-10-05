/*
 * File: ProsumersController.cs
 * Purpose: REST endpoints for prosumer self-registration, profile management, and (de)activation.
 *          Ownership ("a prosumer may only touch their own NIC") is enforced in ProsumerService.
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
    public class ProsumersController : ControllerBase
    {
        private readonly ProsumerService _service;

        // Receives the prosumer service through dependency injection.
        public ProsumersController(ProsumerService service)
        {
            _service = service;
        }

        // GET /api/prosumers?active=false - Backoffice list; active=false is the pending-activation view.
        [Authorize(Roles = Roles.Backoffice)]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool? active) => Ok(await _service.GetAllAsync(active));

        // GET /api/prosumers/{nic}
        [Authorize(Roles = Roles.BackofficeOrProsumer)]
        [HttpGet("{nic}")]
        public async Task<IActionResult> Get(string nic) => (await _service.GetAsync(nic)).ToActionResult();

        // POST /api/prosumers - self-registration from the mobile app.
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterProsumerRequest request) =>
            (await _service.RegisterAsync(request)).ToActionResult();

        // PUT /api/prosumers/{nic} - edit profile.
        [Authorize(Roles = Roles.BackofficeOrProsumer)]
        [HttpPut("{nic}")]
        public async Task<IActionResult> Update(string nic, [FromBody] UpdateProsumerRequest request) =>
            (await _service.UpdateAsync(nic, request)).ToActionResult();

        // PUT /api/prosumers/{nic}/deactivate - a prosumer deactivates their own account.
        [Authorize(Roles = Roles.BackofficeOrProsumer)]
        [HttpPut("{nic}/deactivate")]
        public async Task<IActionResult> Deactivate(string nic) => (await _service.DeactivateAsync(nic)).ToActionResult();

        // PUT /api/prosumers/{nic}/reactivate - Backoffice-only reactivation.
        [Authorize(Roles = Roles.Backoffice)]
        [HttpPut("{nic}/reactivate")]
        public async Task<IActionResult> Reactivate(string nic) => (await _service.ReactivateAsync(nic)).ToActionResult();
    }
}

/*
 * File: ProsumersController.cs
 * Purpose: REST endpoints for Prosumer management (create, update, deactivate, reactivate).
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using Microsoft.AspNetCore.Mvc;
using SolarGrid.Models;
using SolarGrid.Services;

namespace SolarGrid.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProsumersController : ControllerBase
    {
        private readonly ProsumerService _service;

        public ProsumersController(ProsumerService service)
        {
            _service = service;
        }

        // GET /api/prosumers - returns all prosumer records.
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var prosumers = await _service.GetAllAsync();
            return Ok(prosumers);
        }

        // GET /api/prosumers/{nic} - returns one prosumer by NIC.
        [HttpGet("{nic}")]
        public async Task<IActionResult> GetByNIC(string nic)
        {
            var prosumer = await _service.GetByNICAsync(nic);
            return prosumer == null ? NotFound() : Ok(prosumer);
        }

        // POST /api/prosumers - registers a new prosumer.
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Prosumer prosumer)
        {
            await _service.CreateAsync(prosumer);
            return CreatedAtAction(nameof(GetByNIC), new { nic = prosumer.NIC }, prosumer);
        }

        // PUT /api/prosumers/{nic} - updates editable profile fields.
        [HttpPut("{nic}")]
        public async Task<IActionResult> Update(string nic, [FromBody] Prosumer updated)
        {
            var success = await _service.UpdateAsync(nic, updated);
            return success ? NoContent() : NotFound();
        }

        // PUT /api/prosumers/{nic}/deactivate - a prosumer requests their own deactivation.
        [HttpPut("{nic}/deactivate")]
        public async Task<IActionResult> Deactivate(string nic)
        {
            var success = await _service.DeactivateAsync(nic);
            return success ? NoContent() : NotFound();
        }

        // PUT /api/prosumers/{nic}/reactivate - Backoffice-only reactivation.
        [HttpPut("{nic}/reactivate")]
        public async Task<IActionResult> Reactivate(string nic)
        {
            var success = await _service.ReactivateAsync(nic);
            return success ? NoContent() : NotFound();
        }
    }
}

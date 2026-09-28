/*
 * File: NodesController.cs
 * Purpose: REST endpoints for microgrid node management.
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
    public class NodesController : ControllerBase
    {
        private readonly MicrogridNodeService _service;

        public NodesController(MicrogridNodeService service)
        {
            _service = service;
        }

        // GET /api/nodes - lists all microgrid nodes.
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var nodes = await _service.GetAllAsync();
            return Ok(nodes);
        }

        // GET /api/nodes/{id} - returns one node by Id.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var node = await _service.GetByIdAsync(id);
            return node == null ? NotFound() : Ok(node);
        }

        // POST /api/nodes - registers a new microgrid node.
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MicrogridNode node)
        {
            await _service.CreateAsync(node);
            return CreatedAtAction(nameof(GetById), new { id = node.Id }, node);
        }

        // PUT /api/nodes/{id} - updates node schedule/details.
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] MicrogridNode updated)
        {
            var success = await _service.UpdateAsync(id, updated);
            return success ? NoContent() : NotFound();
        }

        // DELETE /api/nodes/{id} - deactivates a node (blocked if active reservations exist).
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deactivate(string id)
        {
            var (success, message) = await _service.DeactivateAsync(id);
            return success ? Ok(new { message }) : BadRequest(new { message });
        }
    }
}

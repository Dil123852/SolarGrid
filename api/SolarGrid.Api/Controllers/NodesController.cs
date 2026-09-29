/*
 * File: NodesController.cs
 * Purpose: REST endpoints for microgrid node management (Backoffice) and node lookup (all roles).
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
    [Authorize]
    public class NodesController : ControllerBase
    {
        private readonly NodeService _service;

        public NodesController(NodeService service)
        {
            _service = service;
        }

        // GET /api/nodes?active=true - used by the web tables, booking dropdowns and the mobile map.
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool? active) => Ok(await _service.GetAllAsync(active));

        // GET /api/nodes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id) => (await _service.GetAsync(id)).ToActionResult();

        // POST /api/nodes
        [Authorize(Roles = Roles.Backoffice)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NodeRequest request) =>
            (await _service.CreateAsync(request)).ToActionResult();

        // PUT /api/nodes/{id}
        [Authorize(Roles = Roles.Backoffice)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] NodeRequest request) =>
            (await _service.UpdateAsync(id, request)).ToActionResult();

        // PUT /api/nodes/{id}/deactivate - 409 while the node has live reservations.
        [Authorize(Roles = Roles.Backoffice)]
        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(string id) => (await _service.DeactivateAsync(id)).ToActionResult();

        // PUT /api/nodes/{id}/activate
        [Authorize(Roles = Roles.Backoffice)]
        [HttpPut("{id}/activate")]
        public async Task<IActionResult> Activate(string id) => (await _service.ActivateAsync(id)).ToActionResult();
    }
}

/*
 * File: DashboardController.cs
 * Purpose: Live reservation counts for the web and mobile dashboards.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarGrid.Application.Services;

namespace SolarGrid.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _service;

        // Receives the dashboard service through dependency injection.
        public DashboardController(DashboardService service)
        {
            _service = service;
        }

        // GET /api/dashboard?nic= - prosumers always get their own counts.
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? nic) => Ok(await _service.GetAsync(nic));
    }
}

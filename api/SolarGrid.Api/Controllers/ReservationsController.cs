/*
 * File: ReservationsController.cs
 * Purpose: REST endpoints for energy slot reservations, QR approval, and operator verification.
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
    public class ReservationsController : ControllerBase
    {
        private readonly ReservationService _service;

        public ReservationsController(ReservationService service)
        {
            _service = service;
        }

        // GET /api/reservations?nic=&nodeId=&status=&from=&to=&search= - prosumers only ever get their own.
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] ReservationQuery query) => Ok(await _service.GetAsync(query));

        // GET /api/reservations/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id) => (await _service.GetByIdAsync(id)).ToActionResult();

        // POST /api/reservations - creates a pending reservation (7-day window, free battery slot).
        [Authorize(Roles = Roles.BackofficeOrProsumer)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReservationRequest request) =>
            (await _service.CreateAsync(request)).ToActionResult();

        // PUT /api/reservations/{id} - reschedules a reservation (12h notice).
        [Authorize(Roles = Roles.BackofficeOrProsumer)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateReservationRequest request) =>
            (await _service.UpdateAsync(id, request)).ToActionResult();

        // DELETE /api/reservations/{id} - cancels a reservation (12h notice); Grid Operators may assist.
        [Authorize(Roles = Roles.Everyone)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(string id) => (await _service.CancelAsync(id)).ToActionResult();

        // POST /api/reservations/{id}/approve - approves a pending reservation and issues its QR token.
        [Authorize(Roles = Roles.Backoffice)]
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(string id) => (await _service.ApproveAsync(id)).ToActionResult();

        // POST /api/reservations/verify-qr - operator mode scans the token and finalises the transfer.
        [Authorize(Roles = Roles.GridOperator)]
        [HttpPost("verify-qr")]
        public async Task<IActionResult> VerifyQr([FromBody] VerifyQrRequest request) =>
            (await _service.VerifyQrAsync(request)).ToActionResult();
    }
}

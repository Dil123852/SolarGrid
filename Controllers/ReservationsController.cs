/*
 * File: ReservationsController.cs
 * Purpose: REST endpoints for energy slot reservations, QR approval, and operator verification.
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
    public class ReservationsController : ControllerBase
    {
        private readonly ReservationService _service;

        public ReservationsController(ReservationService service)
        {
            _service = service;
        }

        // GET /api/reservations?nic=... - lists reservations, optionally filtered by prosumer.
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? nic)
        {
            var reservations = await _service.GetAsync(nic);
            return Ok(reservations);
        }

        // POST /api/reservations - creates a new reservation (validates the 7-day window).
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EnergyReservation reservation)
        {
            var (success, message, created) = await _service.CreateAsync(reservation);
            return success ? Ok(created) : BadRequest(new { message });
        }

        // PUT /api/reservations/{id} - reschedules a reservation (requires 12h notice).
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] DateTime newSlotTime)
        {
            var (success, message) = await _service.UpdateAsync(id, newSlotTime);
            return success ? Ok(new { message }) : BadRequest(new { message });
        }

        // DELETE /api/reservations/{id} - cancels a reservation (requires 12h notice).
        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(string id)
        {
            var (success, message) = await _service.CancelAsync(id);
            return success ? Ok(new { message }) : BadRequest(new { message });
        }

        // POST /api/reservations/{id}/approve - approves a pending reservation, generating a QR token.
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(string id)
        {
            var (success, qrToken) = await _service.ApproveAsync(id);
            return success ? Ok(new { qrToken }) : NotFound();
        }

        // POST /api/reservations/verify-qr - the mobile app's Operator Mode scans and posts the token here.
        [HttpPost("verify-qr")]
        public async Task<IActionResult> VerifyQr([FromBody] string qrToken)
        {
            var (success, message) = await _service.VerifyAndFinalizeAsync(qrToken);
            return success ? Ok(new { message }) : BadRequest(new { message });
        }
    }
}

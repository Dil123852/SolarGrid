/*
 * File: SlotsController.cs
 * Purpose: REST endpoints for energy booking slots. Everyone signed in can list slots (the mobile
 *          app shows free ones); Backoffice and Grid Operators create, update and delete them.
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
    public class SlotsController : ControllerBase
    {
        private readonly BookingSlotService _service;

        // Receives the booking slot service through dependency injection.
        public SlotsController(BookingSlotService service)
        {
            _service = service;
        }

        // GET /api/slots?nodeId=&from=&to= - slots with live booked/available counts.
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] BookingSlotQuery query) => Ok(await _service.GetAsync(query));

        // POST /api/slots - publishes a slot at a station.
        [Authorize(Roles = Roles.Staff)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BookingSlotRequest request) =>
            (await _service.CreateAsync(request)).ToActionResult();

        // PUT /api/slots/{id} - changes a slot's window or capacity.
        [Authorize(Roles = Roles.Staff)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateBookingSlotRequest request) =>
            (await _service.UpdateAsync(id, request)).ToActionResult();

        // DELETE /api/slots/{id} - removes a slot (409 while live bookings are in it).
        [Authorize(Roles = Roles.Staff)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id) => (await _service.DeleteAsync(id)).ToActionResult();
    }
}

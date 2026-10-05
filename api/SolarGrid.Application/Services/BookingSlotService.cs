/*
 * File: BookingSlotService.cs
 * Purpose: Business logic for energy booking slots - the time windows a solar station publishes
 *          for reservations. Validates windows (future, 15 min - 24 h, inside opening hours, no
 *          overlap at the same station, capacity within the station's battery slots) and keeps
 *          capacity consistent with the reservations already made in each slot.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Application.Abstractions;
using SolarGrid.Application.Common;
using SolarGrid.Application.DTOs;
using SolarGrid.Domain.Entities;
using SolarGrid.Domain.Rules;

namespace SolarGrid.Application.Services
{
    public class BookingSlotService
    {
        private readonly IBookingSlotRepository _slots;
        private readonly INodeRepository _nodes;
        private readonly IReservationRepository _reservations;
        private readonly IClock _clock;

        // Receives the slot, node and reservation repositories and the clock.
        public BookingSlotService(IBookingSlotRepository slots, INodeRepository nodes, IReservationRepository reservations, IClock clock)
        {
            _slots = slots;
            _nodes = nodes;
            _reservations = reservations;
            _clock = clock;
        }

        // Lists slots (optionally one station and a date range) with live booked/available counts.
        public async Task<List<BookingSlotResponse>> GetAsync(BookingSlotQuery query)
        {
            var slots = await _slots.FindAsync(query.NodeId, query.From?.AsUtc(), query.To?.AsUtc());
            var names = (await _nodes.GetAllAsync()).ToDictionary(n => n.Id, n => n.Name);
            var result = new List<BookingSlotResponse>();
            foreach (var slot in slots)
                result.Add(ToResponse(slot, names.GetValueOrDefault(slot.NodeId, "Unknown station"), await CountBookedAsync(slot.Id)));
            return result;
        }

        // Publishes a new slot at a station after validating the window and capacity.
        public async Task<Result<BookingSlotResponse>> CreateAsync(BookingSlotRequest request)
        {
            var node = await _nodes.GetByIdAsync(request.NodeId);
            if (node == null) return Result.NotFound<BookingSlotResponse>("Solar station not found.");
            if (!node.IsActive) return Result.Invalid<BookingSlotResponse>("Slots can only be published at an active station.");

            var start = request.StartTime.AsUtc();
            var end = request.EndTime.AsUtc();
            var error = await ValidateWindowAsync(node, start, end, request.Capacity, booked: 0, excludeSlotId: null);
            if (error != null) return Result.Invalid<BookingSlotResponse>(error);

            var slot = new EnergyBookingSlot { NodeId = node.Id, StartTime = start, EndTime = end, Capacity = request.Capacity };
            await _slots.CreateAsync(slot);
            return Result.Ok(ToResponse(slot, node.Name, 0), "Booking slot published.");
        }

        // Changes a slot's window or capacity; capacity can never drop below what is already booked.
        public async Task<Result<BookingSlotResponse>> UpdateAsync(string id, UpdateBookingSlotRequest request)
        {
            var slot = await _slots.GetByIdAsync(id);
            if (slot == null) return Result.NotFound<BookingSlotResponse>("Booking slot not found.");
            var node = await _nodes.GetByIdAsync(slot.NodeId);
            if (node == null) return Result.NotFound<BookingSlotResponse>("Solar station not found.");

            var booked = await CountBookedAsync(slot.Id);
            var start = request.StartTime.AsUtc();
            var end = request.EndTime.AsUtc();

            // Moving a window that already holds bookings would strand them, so only capacity may change then.
            if (booked > 0 && (start != slot.StartTime || end != slot.EndTime))
                return Result.Fail<BookingSlotResponse>(ErrorType.Conflict, $"This slot already has {booked} booking(s); only its capacity can be changed.");

            var error = await ValidateWindowAsync(node, start, end, request.Capacity, booked, excludeSlotId: slot.Id);
            if (error != null) return Result.Invalid<BookingSlotResponse>(error);

            slot.StartTime = start;
            slot.EndTime = end;
            slot.Capacity = request.Capacity;
            await _slots.ReplaceAsync(slot);
            return Result.Ok(ToResponse(slot, node.Name, booked), "Booking slot updated.");
        }

        // Deletes a slot unless live (pending/approved) reservations are booked into it.
        public async Task<Result> DeleteAsync(string id)
        {
            var slot = await _slots.GetByIdAsync(id);
            if (slot == null) return Result.Fail(ErrorType.NotFound, "Booking slot not found.");

            var booked = await CountBookedAsync(slot.Id);
            if (booked > 0)
                return Result.Fail(ErrorType.Conflict, $"Cannot delete: {booked} live booking(s) are in this slot. Cancel them first.");

            await _slots.DeleteAsync(slot.Id);
            return Result.Ok("Booking slot deleted.");
        }

        // Shared window checks for create and update; returns an error message or null when valid.
        private async Task<string?> ValidateWindowAsync(
            MicrogridNode node, DateTime start, DateTime end, int capacity, int booked, string? excludeSlotId)
        {
            if (!BookingSlotPolicy.HasValidLength(start, end))
                return "A slot must end after it starts and last between 15 minutes and 24 hours.";
            if (end <= _clock.UtcNow)
                return "A slot must end in the future.";
            if (capacity < 1 || capacity > node.BatterySlots)
                return $"Capacity must be between 1 and the station's {node.BatterySlots} battery slots.";
            if (capacity < booked)
                return $"Capacity cannot be lower than the {booked} booking(s) already in this slot.";
            if (!NodeSchedule.IsWithinOperatingHours(start, node.OpenTime, node.CloseTime) ||
                !NodeSchedule.IsWithinOperatingHours(end.AddMinutes(-1), node.OpenTime, node.CloseTime))
                return $"{node.Name} is open {node.OpenTime}-{node.CloseTime} (Sri Lanka time); the slot must fit inside those hours.";

            var neighbours = await _slots.FindAsync(node.Id, start.AddDays(-1), end.AddDays(1));
            if (neighbours.Any(s => s.Id != excludeSlotId && s.IsActive &&
                                    BookingSlotPolicy.Overlaps(start, end, s.StartTime, s.EndTime)))
                return "This window overlaps another slot at the same station.";
            return null;
        }

        // Live reservations (pending or approved) booked into the slot.
        private async Task<int> CountBookedAsync(string slotId) =>
            (await _reservations.FindAsync(new ReservationFilter(SlotId: slotId)))
            .Count(r => ReservationPolicy.IsModifiable(r.Status));

        // Maps a slot to its API response with booked and available counts.
        private static BookingSlotResponse ToResponse(EnergyBookingSlot s, string nodeName, int booked) =>
            new(s.Id, s.NodeId, nodeName, s.StartTime, s.EndTime, s.Capacity, booked, Math.Max(0, s.Capacity - booked), s.IsActive);
    }
}

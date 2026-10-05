/*
 * File: ReservationService.cs
 * Purpose: Business logic for energy slot reservations - enforces the 7-day scheduling window,
 *          the 12-hour minimum notice for updates/cancellations, node operating hours and battery-slot capacity,
 *          prosumer ownership, and QR-token issue/verification for the energy transfer handoff.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Application.Abstractions;
using SolarGrid.Application.Common;
using SolarGrid.Application.DTOs;
using SolarGrid.Domain.Entities;
using SolarGrid.Domain.Enums;
using SolarGrid.Domain.Rules;

namespace SolarGrid.Application.Services
{
    public class ReservationService
    {
        private readonly IReservationRepository _reservations;
        private readonly INodeRepository _nodes;
        private readonly IProsumerRepository _prosumers;
        private readonly IClock _clock;
        private readonly ICurrentUser _currentUser;

        public ReservationService(
            IReservationRepository reservations,
            INodeRepository nodes,
            IProsumerRepository prosumers,
            IClock clock,
            ICurrentUser currentUser)
        {
            _reservations = reservations;
            _nodes = nodes;
            _prosumers = prosumers;
            _clock = clock;
            _currentUser = currentUser;
        }

        // Lists reservations with optional filters; prosumers only ever see their own.
        public async Task<List<ReservationResponse>> GetAsync(ReservationQuery query)
        {
            var nic = _currentUser.Role == UserRole.Prosumer
                ? _currentUser.Nic
                : string.IsNullOrWhiteSpace(query.Nic) ? null : Validation.NormalizeNic(query.Nic);

            var filter = new ReservationFilter(nic, query.NodeId, query.Status, query.From?.AsUtc(), query.To?.AsUtc());
            var reservations = await _reservations.FindAsync(filter);
            var nodeNames = await GetNodeNamesAsync();
            var results = reservations.Select(r => r.ToResponse(NameOf(nodeNames, r.NodeId)));

            // Free-text search across node name, NIC, booking reference and status.
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim();
                results = results.Where(r =>
                    r.NodeName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    r.ProsumerNic.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    r.Id.EndsWith(term, StringComparison.OrdinalIgnoreCase) ||
                    r.Status.ToString().Equals(term, StringComparison.OrdinalIgnoreCase));
            }
            return results.ToList();
        }

        public async Task<Result<ReservationResponse>> GetByIdAsync(string id)
        {
            var reservation = await _reservations.GetByIdAsync(id);
            if (reservation == null) return Result.NotFound<ReservationResponse>("Reservation not found.");
            if (!CanView(reservation)) return Result.Forbidden<ReservationResponse>();
            return Result.Ok(await ToResponseAsync(reservation));
        }

        // Creates a pending reservation within the next 7 days at an active node with a free battery slot.
        public async Task<Result<ReservationResponse>> CreateAsync(CreateReservationRequest request)
        {
            var nic = _currentUser.Role == UserRole.Prosumer ? _currentUser.Nic : request.ProsumerNic;
            if (string.IsNullOrWhiteSpace(nic)) return Result.Invalid<ReservationResponse>("Prosumer NIC is required.");
            nic = Validation.NormalizeNic(nic);

            var prosumer = await _prosumers.GetByNicAsync(nic);
            if (prosumer == null) return Result.NotFound<ReservationResponse>("Prosumer not found.");
            if (!prosumer.IsActive) return Result.Invalid<ReservationResponse>("Deactivated accounts cannot make reservations.");

            var now = _clock.UtcNow;
            var slotTime = request.SlotTime.AsUtc();
            var slotError = await ValidateSlotAsync(request.NodeId, slotTime, now, excludeReservationId: null);
            if (slotError != null) return slotError;

            var reservation = new EnergyReservation
            {
                ProsumerNIC = nic,
                NodeId = request.NodeId,
                SlotTime = slotTime,
                Status = ReservationStatus.Pending,
                CreatedAt = now
            };
            await _reservations.CreateAsync(reservation);
            return Result.Ok(await ToResponseAsync(reservation), "Reservation created and awaiting approval.");
        }

        // Reschedules (and optionally moves) a reservation; needs 12h notice before the current slot.
        // An approved reservation goes back to Pending because its QR token was issued for the old slot.
        public async Task<Result<ReservationResponse>> UpdateAsync(string id, UpdateReservationRequest request)
        {
            var reservation = await _reservations.GetByIdAsync(id);
            if (reservation == null) return Result.NotFound<ReservationResponse>("Reservation not found.");
            if (!CanModify(reservation)) return Result.Forbidden<ReservationResponse>();
            if (!ReservationPolicy.IsModifiable(reservation.Status))
                return Result.Invalid<ReservationResponse>($"A {reservation.Status.ToString().ToLower()} reservation cannot be changed.");

            var now = _clock.UtcNow;
            if (!ReservationPolicy.HasMinimumNotice(reservation.SlotTime, now))
                return Result.Invalid<ReservationResponse>("Updates require at least 12 hours' notice before the reserved slot.");

            var nodeId = string.IsNullOrWhiteSpace(request.NodeId) ? reservation.NodeId : request.NodeId;
            var slotTime = request.SlotTime.AsUtc();
            var slotError = await ValidateSlotAsync(nodeId, slotTime, now, excludeReservationId: reservation.Id);
            if (slotError != null) return slotError;

            reservation.NodeId = nodeId;
            reservation.SlotTime = slotTime;
            reservation.Status = ReservationStatus.Pending;
            reservation.QrToken = null;
            reservation.UpdatedAt = now;
            await _reservations.ReplaceAsync(reservation);
            return Result.Ok(await ToResponseAsync(reservation), "Reservation updated and awaiting re-approval.");
        }

        // Cancels a reservation (prosumer, Backoffice or Grid Operator); needs 12h notice before the slot.
        public async Task<Result<ReservationResponse>> CancelAsync(string id)
        {
            var reservation = await _reservations.GetByIdAsync(id);
            if (reservation == null) return Result.NotFound<ReservationResponse>("Reservation not found.");
            if (!CanCancel(reservation)) return Result.Forbidden<ReservationResponse>();
            if (!ReservationPolicy.IsModifiable(reservation.Status))
                return Result.Invalid<ReservationResponse>($"A {reservation.Status.ToString().ToLower()} reservation cannot be cancelled.");

            var now = _clock.UtcNow;
            if (!ReservationPolicy.HasMinimumNotice(reservation.SlotTime, now))
                return Result.Invalid<ReservationResponse>("Cancellations require at least 12 hours' notice before the reserved slot.");

            reservation.Status = ReservationStatus.Cancelled;
            reservation.QrToken = null;
            reservation.UpdatedAt = now;
            await _reservations.ReplaceAsync(reservation);
            return Result.Ok(await ToResponseAsync(reservation), "Reservation cancelled.");
        }

        // Approves a pending, upcoming reservation and issues its one-time QR transaction token.
        public async Task<Result<ReservationResponse>> ApproveAsync(string id)
        {
            var reservation = await _reservations.GetByIdAsync(id);
            if (reservation == null) return Result.NotFound<ReservationResponse>("Reservation not found.");
            if (!ReservationPolicy.CanApprove(reservation.Status))
                return Result.Invalid<ReservationResponse>($"Only pending reservations can be approved (this one is {reservation.Status}).");

            var now = _clock.UtcNow;
            if (reservation.SlotTime <= now)
                return Result.Invalid<ReservationResponse>("This reservation's slot has already passed.");

            reservation.Status = ReservationStatus.Approved;
            reservation.QrToken = Guid.NewGuid().ToString("N");
            reservation.UpdatedAt = now;
            await _reservations.ReplaceAsync(reservation);
            return Result.Ok(await ToResponseAsync(reservation), "Reservation approved.");
        }

        // Grid Operator scans a QR token: an approved reservation becomes Completed exactly once.
        public async Task<Result<ReservationResponse>> VerifyQrAsync(VerifyQrRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.QrToken)) return Result.Invalid<ReservationResponse>("QR token is required.");

            var completed = await _reservations.CompleteByQrTokenAsync(request.QrToken.Trim(), _clock.UtcNow);
            return completed == null
                ? Result.Invalid<ReservationResponse>("Invalid or already-used QR token.")
                : Result.Ok(await ToResponseAsync(completed), "Energy transfer finalized.");
        }

        // Shared checks for a new or moved slot: window, node active, battery-slot capacity.
        private async Task<Result<ReservationResponse>?> ValidateSlotAsync(
            string nodeId, DateTime slotTime, DateTime now, string? excludeReservationId)
        {
            if (!ReservationPolicy.IsWithinBookingWindow(slotTime, now))
                return Result.Invalid<ReservationResponse>("Reservations must be scheduled within the next 7 days.");

            var node = string.IsNullOrWhiteSpace(nodeId) ? null : await _nodes.GetByIdAsync(nodeId);
            if (node == null) return Result.NotFound<ReservationResponse>("Microgrid node not found.");
            if (!node.IsActive) return Result.Invalid<ReservationResponse>("This microgrid node is not accepting reservations.");
            if (!NodeSchedule.IsWithinOperatingHours(slotTime, node.OpenTime, node.CloseTime))
                return Result.Invalid<ReservationResponse>($"{node.Name} is open {node.OpenTime}-{node.CloseTime} (Sri Lanka time). Choose a slot within those hours.");

            var sameSlot = await _reservations.FindAsync(new ReservationFilter(NodeId: nodeId, From: slotTime, To: slotTime));
            var taken = sameSlot.Count(r => r.Id != excludeReservationId && ReservationPolicy.IsModifiable(r.Status));
            if (taken >= node.BatterySlots)
                return Result.Fail<ReservationResponse>(ErrorType.Conflict, "All battery slots at this node are booked for that time.");

            return null;
        }

        private bool CanView(EnergyReservation r) =>
            _currentUser.Role is UserRole.Backoffice or UserRole.GridOperator || _currentUser.CanAccessProsumer(r.ProsumerNIC);

        private bool CanModify(EnergyReservation r) => _currentUser.CanAccessProsumer(r.ProsumerNIC);

        // Cancellations can also be made with the assistance of a Grid Operator (assignment scenario).
        private bool CanCancel(EnergyReservation r) => CanModify(r) || _currentUser.Role == UserRole.GridOperator;

        private async Task<ReservationResponse> ToResponseAsync(EnergyReservation r)
        {
            var node = await _nodes.GetByIdAsync(r.NodeId);
            return r.ToResponse(node?.Name ?? "Unknown node");
        }

        private async Task<Dictionary<string, string>> GetNodeNamesAsync() =>
            (await _nodes.GetAllAsync()).ToDictionary(n => n.Id, n => n.Name);

        private static string NameOf(Dictionary<string, string> names, string id) =>
            names.TryGetValue(id, out var name) ? name : "Unknown node";
    }
}

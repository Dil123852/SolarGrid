/*
 * File: ReservationService.cs
 * Purpose: Business logic for energy slot reservations - enforces the 7-day scheduling window
 *          and 12-hour minimum notice for updates/cancellations (assignment requirement),
 *          plus QR-token generation and verification for the energy transfer handoff.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using MongoDB.Driver;
using SolarGrid.Models;

namespace SolarGrid.Services
{
    public class ReservationService
    {
        private readonly MongoDBContext _context;

        public ReservationService(MongoDBContext context)
        {
            _context = context;
        }

        // Returns all reservations, optionally filtered to one prosumer's NIC.
        public async Task<List<EnergyReservation>> GetAsync(string? nic = null)
        {
            var filter = string.IsNullOrEmpty(nic)
                ? Builders<EnergyReservation>.Filter.Empty
                : Builders<EnergyReservation>.Filter.Eq(r => r.ProsumerNIC, nic);
            return await _context.Reservations.Find(filter).ToListAsync();
        }

        // Creates a reservation, validating that the slot falls within the next 7 days.
        public async Task<(bool Success, string Message, EnergyReservation? Reservation)> CreateAsync(EnergyReservation reservation)
        {
            if (reservation.SlotTime > DateTime.UtcNow.AddDays(7) || reservation.SlotTime < DateTime.UtcNow)
                return (false, "Reservations must be scheduled within the next 7 days.", null);

            reservation.Status = "Pending";
            reservation.CreatedAt = DateTime.UtcNow;
            await _context.Reservations.InsertOneAsync(reservation);
            return (true, "Reservation created.", reservation);
        }

        // Reschedules a reservation, requiring at least 12 hours' notice before its current slot.
        public async Task<(bool Success, string Message)> UpdateAsync(string id, DateTime newSlotTime)
        {
            var existing = await _context.Reservations.Find(r => r.Id == id).FirstOrDefaultAsync();
            if (existing == null) return (false, "Reservation not found.");

            if (existing.SlotTime < DateTime.UtcNow.AddHours(12))
                return (false, "Updates require at least 12 hours' notice.");

            var update = Builders<EnergyReservation>.Update.Set(r => r.SlotTime, newSlotTime);
            await _context.Reservations.UpdateOneAsync(r => r.Id == id, update);
            return (true, "Reservation updated.");
        }

        // Cancels a reservation, requiring at least 12 hours' notice.
        public async Task<(bool Success, string Message)> CancelAsync(string id)
        {
            var existing = await _context.Reservations.Find(r => r.Id == id).FirstOrDefaultAsync();
            if (existing == null) return (false, "Reservation not found.");

            if (existing.SlotTime < DateTime.UtcNow.AddHours(12))
                return (false, "Cancellations require at least 12 hours' notice.");

            var update = Builders<EnergyReservation>.Update.Set(r => r.Status, "Cancelled");
            await _context.Reservations.UpdateOneAsync(r => r.Id == id, update);
            return (true, "Reservation cancelled.");
        }

        // Approves a pending reservation and generates its one-time QR transaction token.
        public async Task<(bool Success, string? QrToken)> ApproveAsync(string id)
        {
            var qrToken = Guid.NewGuid().ToString("N");
            var update = Builders<EnergyReservation>.Update
                .Set(r => r.Status, "Approved")
                .Set(r => r.QrToken, qrToken);
            var result = await _context.Reservations.UpdateOneAsync(r => r.Id == id, update);
            return (result.ModifiedCount > 0, qrToken);
        }

        // Verifies a scanned QR token belongs to an approved reservation, then finalizes the transfer.
        public async Task<(bool Success, string Message)> VerifyAndFinalizeAsync(string qrToken)
        {
            var reservation = await _context.Reservations
                .Find(r => r.QrToken == qrToken && r.Status == "Approved")
                .FirstOrDefaultAsync();

            if (reservation == null) return (false, "Invalid or already-used QR token.");

            var update = Builders<EnergyReservation>.Update.Set(r => r.Status, "Completed");
            await _context.Reservations.UpdateOneAsync(r => r.Id == reservation.Id, update);
            return (true, "Energy transfer finalized.");
        }
    }
}

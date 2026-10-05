/*
 * File: EnergyReservation.cs
 * Purpose: Domain entity for a power trading / energy slot reservation made by a Prosumer.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Domain.Enums;

namespace SolarGrid.Domain.Entities
{
    public class EnergyReservation
    {
        public string Id { get; set; } = string.Empty;

        // Links to Prosumer.NIC.
        public string ProsumerNIC { get; set; } = string.Empty;

        // Links to MicrogridNode.Id.
        public string NodeId { get; set; } = string.Empty;

        // The requested date/time for the energy slot (UTC).
        public DateTime SlotTime { get; set; }

        // Links to EnergyBookingSlot.Id when the station publishes booking slots (null otherwise).
        public string? SlotId { get; set; }

        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        // Populated once approved; scanned by a Grid Operator in the mobile app's Operator Mode.
        public string? QrToken { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}

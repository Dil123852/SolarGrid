/*
 * File: EnergyBookingSlot.cs
 * Purpose: Domain entity for a bookable energy slot - a time window published at a solar station
 *          (microgrid node) with a capacity of battery positions. Reservations made inside the
 *          window reference it by SlotId and count against its capacity.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

namespace SolarGrid.Domain.Entities
{
    public class EnergyBookingSlot
    {
        public string Id { get; set; } = string.Empty;

        // Links to MicrogridNode.Id (the solar station offering the slot).
        public string NodeId { get; set; } = string.Empty;

        // Window start and end (UTC); a reservation belongs to the slot when Start <= SlotTime < End.
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        // How many reservations (battery positions) the window can take.
        public int Capacity { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

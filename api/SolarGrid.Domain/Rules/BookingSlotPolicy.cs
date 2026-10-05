/*
 * File: BookingSlotPolicy.cs
 * Purpose: Pure rules for energy booking slots - which slot a reservation time falls into,
 *          valid window lengths, and overlapping windows at the same station.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Domain.Entities;

namespace SolarGrid.Domain.Rules
{
    public static class BookingSlotPolicy
    {
        public static readonly TimeSpan MinimumLength = TimeSpan.FromMinutes(15);
        public static readonly TimeSpan MaximumLength = TimeSpan.FromHours(24);

        // A reservation time belongs to a slot when it is inside [StartTime, EndTime).
        public static bool Contains(EnergyBookingSlot slot, DateTime time) =>
            slot.IsActive && time >= slot.StartTime && time < slot.EndTime;

        // A window must run forwards and last between 15 minutes and 24 hours.
        public static bool HasValidLength(DateTime start, DateTime end) =>
            end - start >= MinimumLength && end - start <= MaximumLength;

        // Two windows overlap when each starts before the other ends.
        public static bool Overlaps(DateTime startA, DateTime endA, DateTime startB, DateTime endB) =>
            startA < endB && startB < endA;
    }
}

/*
 * File: NodeSchedule.cs
 * Purpose: Pure rules for a microgrid node's daily operating schedule. Hours are "HH:mm" in
 *          Sri Lanka local time (UTC+05:30, no daylight saving); slots are stored in UTC.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using System.Globalization;

namespace SolarGrid.Domain.Rules
{
    public static class NodeSchedule
    {
        // Sri Lanka Standard Time has a fixed offset, so no time-zone database is needed.
        public static readonly TimeSpan LocalOffset = TimeSpan.FromHours(5.5);

        // Parses an "HH:mm" time of day; returns false for anything else.
        public static bool TryParse(string? value, out TimeOnly time) =>
            TimeOnly.TryParseExact(value?.Trim(), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out time);

        // A schedule is valid when both times are absent (24 hours) or both are present with open before close.
        public static bool IsValid(string? open, string? close)
        {
            if (string.IsNullOrWhiteSpace(open) && string.IsNullOrWhiteSpace(close)) return true;
            return TryParse(open, out var o) && TryParse(close, out var c) && o < c;
        }

        // True when the UTC slot falls inside the node's local opening hours (or the node has no schedule).
        public static bool IsWithinOperatingHours(DateTime slotUtc, string? open, string? close)
        {
            if (!TryParse(open, out var o) || !TryParse(close, out var c)) return true;
            var local = TimeOnly.FromDateTime(slotUtc.Add(LocalOffset));
            return local >= o && local < c;
        }
    }
}

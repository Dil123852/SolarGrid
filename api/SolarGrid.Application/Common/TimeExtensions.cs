/*
 * File: TimeExtensions.cs
 * Purpose: Normalises client-supplied timestamps to UTC so every rule compares like with like.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

namespace SolarGrid.Application.Common
{
    public static class TimeExtensions
    {
        // Local times are converted; unspecified times (no offset sent) are treated as UTC.
        public static DateTime AsUtc(this DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}

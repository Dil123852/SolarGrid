/*
 * File: ReservationFilter.cs
 * Purpose: Criteria for listing and counting reservations; null members are not filtered on.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Domain.Enums;

namespace SolarGrid.Application.Abstractions
{
    public record ReservationFilter(
        string? ProsumerNic = null,
        string? NodeId = null,
        ReservationStatus? Status = null,
        DateTime? From = null,
        DateTime? To = null,
        string? SlotId = null);
}

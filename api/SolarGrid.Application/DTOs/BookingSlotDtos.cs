/*
 * File: BookingSlotDtos.cs
 * Purpose: Request/response contracts for energy booking slots. Responses include live booked
 *          and available counts so clients can show free capacity without any logic of their own.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

namespace SolarGrid.Application.DTOs
{
    public record BookingSlotRequest(string NodeId, DateTime StartTime, DateTime EndTime, int Capacity);

    public record UpdateBookingSlotRequest(DateTime StartTime, DateTime EndTime, int Capacity);

    public record BookingSlotQuery(string? NodeId, DateTime? From, DateTime? To);

    public record BookingSlotResponse(
        string Id,
        string NodeId,
        string NodeName,
        DateTime StartTime,
        DateTime EndTime,
        int Capacity,
        int Booked,
        int Available,
        bool IsActive);
}

/*
 * File: NodeDtos.cs
 * Purpose: Request/response contracts for microgrid node management and the mobile map.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

namespace SolarGrid.Application.DTOs
{
    // OpenTime / CloseTime are "HH:mm" in Sri Lanka time; leave both empty for a 24-hour node.
    public record NodeRequest(
        string Name,
        double Latitude,
        double Longitude,
        double CapacityKWh,
        int BatterySlots,
        string? OpenTime = null,
        string? CloseTime = null);

    // Grid Operators may only change the available battery slots of a node.
    public record UpdateBatterySlotsRequest(int BatterySlots);

    public record NodeResponse(
        string Id,
        string Name,
        double Latitude,
        double Longitude,
        double CapacityKWh,
        int BatterySlots,
        bool IsActive,
        string? OpenTime,
        string? CloseTime);
}

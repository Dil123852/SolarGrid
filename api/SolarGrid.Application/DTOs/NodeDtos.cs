/*
 * File: NodeDtos.cs
 * Purpose: Request/response contracts for microgrid node management and the mobile map.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

namespace SolarGrid.Application.DTOs
{
    public record NodeRequest(string Name, double Latitude, double Longitude, double CapacityKWh, int BatterySlots);

    public record NodeResponse(string Id, string Name, double Latitude, double Longitude, double CapacityKWh, int BatterySlots, bool IsActive);
}

/*
 * File: Mappings.cs
 * Purpose: Entity-to-DTO conversions shared by the application services.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Application.DTOs;
using SolarGrid.Domain.Entities;

namespace SolarGrid.Application.Common
{
    public static class Mappings
    {
        public static UserResponse ToResponse(this User u) =>
            new(u.Id, u.Username, u.Email, u.Role, u.IsActive, u.CreatedAt);

        public static ProsumerResponse ToResponse(this Prosumer p) =>
            new(p.NIC, p.Name, p.Email, p.Phone, p.Address, p.IsActive, p.CreatedAt);

        public static NodeResponse ToResponse(this MicrogridNode n) =>
            new(n.Id, n.Name, n.Latitude, n.Longitude, n.CapacityKWh, n.BatterySlots, n.IsActive);

        public static ReservationResponse ToResponse(this EnergyReservation r, string nodeName) =>
            new(r.Id, r.ProsumerNIC, r.NodeId, nodeName, r.SlotTime, r.Status, r.QrToken, r.CreatedAt, r.UpdatedAt);
    }
}

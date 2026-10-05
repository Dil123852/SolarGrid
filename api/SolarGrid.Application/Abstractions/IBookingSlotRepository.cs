/*
 * File: IBookingSlotRepository.cs
 * Purpose: Persistence port for energy booking slots (the EnergyBookingSlots collection).
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Domain.Entities;

namespace SolarGrid.Application.Abstractions
{
    public interface IBookingSlotRepository
    {
        // Slots for an optional station whose window overlaps [from, to], ordered by start time.
        Task<List<EnergyBookingSlot>> FindAsync(string? nodeId = null, DateTime? from = null, DateTime? to = null);
        Task<EnergyBookingSlot?> GetByIdAsync(string id);
        Task CreateAsync(EnergyBookingSlot slot);
        Task<bool> ReplaceAsync(EnergyBookingSlot slot);
        Task<bool> DeleteAsync(string id);
    }
}

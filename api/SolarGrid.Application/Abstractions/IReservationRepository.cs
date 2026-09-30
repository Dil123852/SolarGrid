/*
 * File: IReservationRepository.cs
 * Purpose: Persistence port for energy reservations, including the atomic QR completion.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Domain.Entities;

namespace SolarGrid.Application.Abstractions
{
    public interface IReservationRepository
    {
        Task<List<EnergyReservation>> FindAsync(ReservationFilter filter);
        Task<EnergyReservation?> GetByIdAsync(string id);
        Task<long> CountAsync(ReservationFilter filter);
        Task CreateAsync(EnergyReservation reservation);
        Task<bool> ReplaceAsync(EnergyReservation reservation);

        // Atomically moves an Approved reservation with this token to Completed; null if none matched.
        Task<EnergyReservation?> CompleteByQrTokenAsync(string qrToken, DateTime completedAt);
    }
}

/*
 * File: Repositories.cs
 * Purpose: Persistence ports. The application layer depends only on these interfaces;
 *          MongoDB implementations live in SolarGrid.Infrastructure.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Domain.Entities;
using SolarGrid.Domain.Enums;

namespace SolarGrid.Application.Abstractions
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllAsync();
        Task<User?> GetByUsernameAsync(string username);
        Task<bool> ExistsAsync(string username, string email);
        Task<bool> AnyAsync();
        Task CreateAsync(User user);
        Task<bool> SetActiveAsync(string id, bool isActive);
    }

    public interface IProsumerRepository
    {
        Task<List<Prosumer>> GetAllAsync(bool? isActive = null);
        Task<Prosumer?> GetByNicAsync(string nic);
        Task<bool> ExistsAsync(string nic);
        Task CreateAsync(Prosumer prosumer);
        Task<bool> ReplaceAsync(Prosumer prosumer);
        Task<bool> SetActiveAsync(string nic, bool isActive);
    }

    public interface INodeRepository
    {
        Task<List<MicrogridNode>> GetAllAsync(bool? isActive = null);
        Task<MicrogridNode?> GetByIdAsync(string id);
        Task CreateAsync(MicrogridNode node);
        Task<bool> ReplaceAsync(MicrogridNode node);
        Task<bool> SetActiveAsync(string id, bool isActive);
    }

    // Criteria for listing reservations; null members are not filtered on.
    public record ReservationFilter(
        string? ProsumerNic = null,
        string? NodeId = null,
        ReservationStatus? Status = null,
        DateTime? From = null,
        DateTime? To = null);

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

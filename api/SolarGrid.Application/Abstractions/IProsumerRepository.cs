/*
 * File: IProsumerRepository.cs
 * Purpose: Persistence port for NIC-keyed prosumer accounts.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Domain.Entities;

namespace SolarGrid.Application.Abstractions
{
    public interface IProsumerRepository
    {
        Task<List<Prosumer>> GetAllAsync(bool? isActive = null);
        Task<Prosumer?> GetByNicAsync(string nic);
        Task<bool> ExistsAsync(string nic);
        Task CreateAsync(Prosumer prosumer);
        Task<bool> ReplaceAsync(Prosumer prosumer);
        Task<bool> SetActiveAsync(string nic, bool isActive);
    }
}

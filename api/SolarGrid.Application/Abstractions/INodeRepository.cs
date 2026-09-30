/*
 * File: INodeRepository.cs
 * Purpose: Persistence port for microgrid nodes.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Domain.Entities;

namespace SolarGrid.Application.Abstractions
{
    public interface INodeRepository
    {
        Task<List<MicrogridNode>> GetAllAsync(bool? isActive = null);
        Task<MicrogridNode?> GetByIdAsync(string id);
        Task CreateAsync(MicrogridNode node);
        Task<bool> ReplaceAsync(MicrogridNode node);
        Task<bool> SetActiveAsync(string id, bool isActive);
    }
}

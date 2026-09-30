/*
 * File: IUserRepository.cs
 * Purpose: Persistence port for staff accounts; implemented by Infrastructure.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Domain.Entities;

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
}

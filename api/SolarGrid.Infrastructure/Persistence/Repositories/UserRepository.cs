/*
 * File: UserRepository.cs
 * Purpose: MongoDB implementation of IUserRepository (staff accounts).
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using MongoDB.Bson;
using MongoDB.Driver;
using SolarGrid.Application.Abstractions;
using SolarGrid.Domain.Entities;

namespace SolarGrid.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;

        // Gets the Users collection from the shared MongoDB context.
        public UserRepository(MongoDbContext context)
        {
            _users = context.Users;
        }

        // Lists all staff users sorted by username.
        public async Task<List<User>> GetAllAsync() =>
            await _users.Find(_ => true).SortBy(u => u.Username).ToListAsync();

        // Finds a staff user by (lower-case) username.
        public async Task<User?> GetByUsernameAsync(string username) =>
            await _users.Find(u => u.Username == username).FirstOrDefaultAsync();

        // True when the username or email is already taken.
        public async Task<bool> ExistsAsync(string username, string email) =>
            await _users.Find(u => u.Username == username || u.Email == email).AnyAsync();

        // True when at least one staff user exists (used by first-run seeding).
        public async Task<bool> AnyAsync() => await _users.Find(_ => true).AnyAsync();

        // Inserts a new staff user; MongoDB generates its id.
        public async Task CreateAsync(User user) => await _users.InsertOneAsync(user);

        // Enables or disables a staff account; false for unknown or malformed ids.
        public async Task<bool> SetActiveAsync(string id, bool isActive)
        {
            if (!ObjectId.TryParse(id, out _)) return false;
            var result = await _users.UpdateOneAsync(u => u.Id == id, Builders<User>.Update.Set(u => u.IsActive, isActive));
            return result.MatchedCount > 0;
        }
    }
}

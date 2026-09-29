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

        public UserRepository(MongoDbContext context)
        {
            _users = context.Users;
        }

        public async Task<List<User>> GetAllAsync() =>
            await _users.Find(_ => true).SortBy(u => u.Username).ToListAsync();

        public async Task<User?> GetByUsernameAsync(string username) =>
            await _users.Find(u => u.Username == username).FirstOrDefaultAsync();

        public async Task<bool> ExistsAsync(string username, string email) =>
            await _users.Find(u => u.Username == username || u.Email == email).AnyAsync();

        public async Task<bool> AnyAsync() => await _users.Find(_ => true).AnyAsync();

        public async Task CreateAsync(User user) => await _users.InsertOneAsync(user);

        public async Task<bool> SetActiveAsync(string id, bool isActive)
        {
            if (!ObjectId.TryParse(id, out _)) return false;
            var result = await _users.UpdateOneAsync(u => u.Id == id, Builders<User>.Update.Set(u => u.IsActive, isActive));
            return result.MatchedCount > 0;
        }
    }
}

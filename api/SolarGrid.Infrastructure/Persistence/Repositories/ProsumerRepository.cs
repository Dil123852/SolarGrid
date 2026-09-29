/*
 * File: ProsumerRepository.cs
 * Purpose: MongoDB implementation of IProsumerRepository (NIC-keyed prosumer accounts).
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using MongoDB.Driver;
using SolarGrid.Application.Abstractions;
using SolarGrid.Domain.Entities;

namespace SolarGrid.Infrastructure.Persistence.Repositories
{
    public class ProsumerRepository : IProsumerRepository
    {
        private readonly IMongoCollection<Prosumer> _prosumers;

        public ProsumerRepository(MongoDbContext context)
        {
            _prosumers = context.Prosumers;
        }

        public async Task<List<Prosumer>> GetAllAsync(bool? isActive = null)
        {
            var filter = isActive.HasValue
                ? Builders<Prosumer>.Filter.Eq(p => p.IsActive, isActive.Value)
                : Builders<Prosumer>.Filter.Empty;
            return await _prosumers.Find(filter).SortBy(p => p.Name).ToListAsync();
        }

        public async Task<Prosumer?> GetByNicAsync(string nic) =>
            await _prosumers.Find(p => p.NIC == nic).FirstOrDefaultAsync();

        public async Task<bool> ExistsAsync(string nic) => await _prosumers.Find(p => p.NIC == nic).AnyAsync();

        public async Task CreateAsync(Prosumer prosumer) => await _prosumers.InsertOneAsync(prosumer);

        public async Task<bool> ReplaceAsync(Prosumer prosumer)
        {
            var result = await _prosumers.ReplaceOneAsync(p => p.NIC == prosumer.NIC, prosumer);
            return result.MatchedCount > 0;
        }

        public async Task<bool> SetActiveAsync(string nic, bool isActive)
        {
            var result = await _prosumers.UpdateOneAsync(
                p => p.NIC == nic, Builders<Prosumer>.Update.Set(p => p.IsActive, isActive));
            return result.MatchedCount > 0;
        }
    }
}

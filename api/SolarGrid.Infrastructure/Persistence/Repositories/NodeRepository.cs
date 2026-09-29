/*
 * File: NodeRepository.cs
 * Purpose: MongoDB implementation of INodeRepository (microgrid nodes).
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using MongoDB.Bson;
using MongoDB.Driver;
using SolarGrid.Application.Abstractions;
using SolarGrid.Domain.Entities;

namespace SolarGrid.Infrastructure.Persistence.Repositories
{
    public class NodeRepository : INodeRepository
    {
        private readonly IMongoCollection<MicrogridNode> _nodes;

        public NodeRepository(MongoDbContext context)
        {
            _nodes = context.Nodes;
        }

        public async Task<List<MicrogridNode>> GetAllAsync(bool? isActive = null)
        {
            var filter = isActive.HasValue
                ? Builders<MicrogridNode>.Filter.Eq(n => n.IsActive, isActive.Value)
                : Builders<MicrogridNode>.Filter.Empty;
            return await _nodes.Find(filter).SortBy(n => n.Name).ToListAsync();
        }

        // Malformed ids are treated as "not found" rather than throwing a format exception.
        public async Task<MicrogridNode?> GetByIdAsync(string id) =>
            ObjectId.TryParse(id, out _) ? await _nodes.Find(n => n.Id == id).FirstOrDefaultAsync() : null;

        public async Task CreateAsync(MicrogridNode node) => await _nodes.InsertOneAsync(node);

        public async Task<bool> ReplaceAsync(MicrogridNode node)
        {
            var result = await _nodes.ReplaceOneAsync(n => n.Id == node.Id, node);
            return result.MatchedCount > 0;
        }

        public async Task<bool> SetActiveAsync(string id, bool isActive)
        {
            if (!ObjectId.TryParse(id, out _)) return false;
            var result = await _nodes.UpdateOneAsync(n => n.Id == id, Builders<MicrogridNode>.Update.Set(n => n.IsActive, isActive));
            return result.MatchedCount > 0;
        }
    }
}

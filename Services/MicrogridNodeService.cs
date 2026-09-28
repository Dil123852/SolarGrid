/*
 * File: MicrogridNodeService.cs
 * Purpose: Business logic for managing microgrid nodes, including the rule that a node
 *          cannot be deactivated while it has active energy reservations.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using MongoDB.Driver;
using SolarGrid.Models;

namespace SolarGrid.Services
{
    public class MicrogridNodeService
    {
        private readonly MongoDBContext _context;

        public MicrogridNodeService(MongoDBContext context)
        {
            _context = context;
        }

        // Returns all microgrid nodes.
        public async Task<List<MicrogridNode>> GetAllAsync() =>
            await _context.Nodes.Find(_ => true).ToListAsync();

        // Looks up a single node by its Id.
        public async Task<MicrogridNode?> GetByIdAsync(string id) =>
            await _context.Nodes.Find(n => n.Id == id).FirstOrDefaultAsync();

        // Creates a new microgrid node with GPS, capacity, and battery slot info.
        public async Task CreateAsync(MicrogridNode node) =>
            await _context.Nodes.InsertOneAsync(node);

        // Updates a node's schedule/details.
        public async Task<bool> UpdateAsync(string id, MicrogridNode updated)
        {
            updated.Id = id;
            var result = await _context.Nodes.ReplaceOneAsync(n => n.Id == id, updated);
            return result.ModifiedCount > 0;
        }

        // Deactivates a node, but only if it has no active (non-cancelled) reservations against it.
        public async Task<(bool Success, string Message)> DeactivateAsync(string nodeId)
        {
            var activeReservationExists = await _context.Reservations
                .Find(r => r.NodeId == nodeId && r.Status != "Cancelled")
                .AnyAsync();

            if (activeReservationExists)
                return (false, "Cannot deactivate: node has active energy reservations.");

            var update = Builders<MicrogridNode>.Update.Set(n => n.IsActive, false);
            var result = await _context.Nodes.UpdateOneAsync(n => n.Id == nodeId, update);
            return (result.ModifiedCount > 0, "Node deactivated.");
        }
    }
}

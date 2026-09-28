/*
 * File: ProsumerService.cs
 * Purpose: Business logic for creating, updating, and (de)activating Prosumer accounts.
 *          All rules live here, not in the web/mobile clients, per the FAT-service architecture.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using MongoDB.Driver;
using SolarGrid.Models;

namespace SolarGrid.Services
{
    public class ProsumerService
    {
        private readonly MongoDBContext _context;

        public ProsumerService(MongoDBContext context)
        {
            _context = context;
        }

        // Returns every prosumer record in the database.
        public async Task<List<Prosumer>> GetAllAsync() =>
            await _context.Prosumers.Find(_ => true).ToListAsync();

        // Looks up a single prosumer by their NIC (primary key).
        public async Task<Prosumer?> GetByNICAsync(string nic) =>
            await _context.Prosumers.Find(p => p.NIC == nic).FirstOrDefaultAsync();

        // Inserts a new prosumer record (used by mobile-app self-registration).
        public async Task CreateAsync(Prosumer prosumer) =>
            await _context.Prosumers.InsertOneAsync(prosumer);

        // Replaces an existing prosumer's editable fields (name, phone, address).
        public async Task<bool> UpdateAsync(string nic, Prosumer updated)
        {
            updated.NIC = nic; // keep the key stable regardless of what the client sent
            var result = await _context.Prosumers.ReplaceOneAsync(p => p.NIC == nic, updated);
            return result.ModifiedCount > 0;
        }

        // Marks a prosumer inactive. Prosumers can request this themselves via the mobile app.
        public async Task<bool> DeactivateAsync(string nic)
        {
            var update = Builders<Prosumer>.Update.Set(p => p.IsActive, false);
            var result = await _context.Prosumers.UpdateOneAsync(p => p.NIC == nic, update);
            return result.ModifiedCount > 0;
        }

        // Reactivates a prosumer. Per spec, only a Backoffice user should be able to call this
        // (enforce that role check at the controller/auth layer once login roles are added).
        public async Task<bool> ReactivateAsync(string nic)
        {
            var update = Builders<Prosumer>.Update.Set(p => p.IsActive, true);
            var result = await _context.Prosumers.UpdateOneAsync(p => p.NIC == nic, update);
            return result.ModifiedCount > 0;
        }
    }
}

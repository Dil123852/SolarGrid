/*
 * File: BookingSlotRepository.cs
 * Purpose: MongoDB implementation of IBookingSlotRepository (the EnergyBookingSlots collection).
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using MongoDB.Bson;
using MongoDB.Driver;
using SolarGrid.Application.Abstractions;
using SolarGrid.Domain.Entities;

namespace SolarGrid.Infrastructure.Persistence.Repositories
{
    public class BookingSlotRepository : IBookingSlotRepository
    {
        private readonly IMongoCollection<EnergyBookingSlot> _slots;

        // Gets the EnergyBookingSlots collection from the shared MongoDB context.
        public BookingSlotRepository(MongoDbContext context)
        {
            _slots = context.BookingSlots;
        }

        // Finds slots for an optional station whose window overlaps [from, to], earliest first.
        public async Task<List<EnergyBookingSlot>> FindAsync(string? nodeId = null, DateTime? from = null, DateTime? to = null)
        {
            var b = Builders<EnergyBookingSlot>.Filter;
            var filters = new List<FilterDefinition<EnergyBookingSlot>>();
            if (!string.IsNullOrWhiteSpace(nodeId)) filters.Add(b.Eq(s => s.NodeId, nodeId));
            if (from.HasValue) filters.Add(b.Gt(s => s.EndTime, from.Value));
            if (to.HasValue) filters.Add(b.Lt(s => s.StartTime, to.Value));
            var filter = filters.Count == 0 ? b.Empty : b.And(filters);
            return await _slots.Find(filter).SortBy(s => s.StartTime).ToListAsync();
        }

        // Finds a slot by id; null for unknown or malformed ids.
        public async Task<EnergyBookingSlot?> GetByIdAsync(string id) =>
            ObjectId.TryParse(id, out _) ? await _slots.Find(s => s.Id == id).FirstOrDefaultAsync() : null;

        // Inserts a new slot; MongoDB generates its id.
        public async Task CreateAsync(EnergyBookingSlot slot) => await _slots.InsertOneAsync(slot);

        // Replaces the stored slot document; true if it existed.
        public async Task<bool> ReplaceAsync(EnergyBookingSlot slot)
        {
            var result = await _slots.ReplaceOneAsync(s => s.Id == slot.Id, slot);
            return result.MatchedCount > 0;
        }

        // Deletes a slot; false for unknown or malformed ids.
        public async Task<bool> DeleteAsync(string id)
        {
            if (!ObjectId.TryParse(id, out _)) return false;
            var result = await _slots.DeleteOneAsync(s => s.Id == id);
            return result.DeletedCount > 0;
        }
    }
}

/*
 * File: ReservationRepository.cs
 * Purpose: MongoDB implementation of IReservationRepository, including filtered queries,
 *          dashboard counts, and the atomic one-time QR completion.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using MongoDB.Bson;
using MongoDB.Driver;
using SolarGrid.Application.Abstractions;
using SolarGrid.Domain.Entities;
using SolarGrid.Domain.Enums;

namespace SolarGrid.Infrastructure.Persistence.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly IMongoCollection<EnergyReservation> _reservations;

        public ReservationRepository(MongoDbContext context)
        {
            _reservations = context.Reservations;
        }

        public async Task<List<EnergyReservation>> FindAsync(ReservationFilter filter) =>
            await _reservations.Find(Build(filter)).SortByDescending(r => r.SlotTime).ToListAsync();

        public async Task<EnergyReservation?> GetByIdAsync(string id) =>
            ObjectId.TryParse(id, out _) ? await _reservations.Find(r => r.Id == id).FirstOrDefaultAsync() : null;

        public async Task<long> CountAsync(ReservationFilter filter) =>
            await _reservations.CountDocumentsAsync(Build(filter));

        public async Task CreateAsync(EnergyReservation reservation) => await _reservations.InsertOneAsync(reservation);

        public async Task<bool> ReplaceAsync(EnergyReservation reservation)
        {
            var result = await _reservations.ReplaceOneAsync(r => r.Id == reservation.Id, reservation);
            return result.MatchedCount > 0;
        }

        // Single find-and-update, so two scans of the same QR can never both succeed.
        public async Task<EnergyReservation?> CompleteByQrTokenAsync(string qrToken, DateTime completedAt) =>
            await _reservations.FindOneAndUpdateAsync(
                r => r.QrToken == qrToken && r.Status == ReservationStatus.Approved,
                Builders<EnergyReservation>.Update
                    .Set(r => r.Status, ReservationStatus.Completed)
                    .Set(r => r.UpdatedAt, completedAt),
                new FindOneAndUpdateOptions<EnergyReservation> { ReturnDocument = ReturnDocument.After });

        private static FilterDefinition<EnergyReservation> Build(ReservationFilter f)
        {
            var b = Builders<EnergyReservation>.Filter;
            var filters = new List<FilterDefinition<EnergyReservation>>();

            if (!string.IsNullOrWhiteSpace(f.ProsumerNic)) filters.Add(b.Eq(r => r.ProsumerNIC, f.ProsumerNic));
            if (!string.IsNullOrWhiteSpace(f.NodeId)) filters.Add(b.Eq(r => r.NodeId, f.NodeId));
            if (f.Status.HasValue) filters.Add(b.Eq(r => r.Status, f.Status.Value));
            if (f.From.HasValue) filters.Add(b.Gte(r => r.SlotTime, f.From.Value));
            if (f.To.HasValue) filters.Add(b.Lte(r => r.SlotTime, f.To.Value));

            return filters.Count == 0 ? b.Empty : b.And(filters);
        }
    }
}

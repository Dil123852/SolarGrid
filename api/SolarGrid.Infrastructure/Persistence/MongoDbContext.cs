/*
 * File: MongoDbContext.cs
 * Purpose: Creates and exposes the MongoDB database connection and its five collections
 *          (Users, Prosumers, Nodes, Reservations, EnergyBookingSlots), and ensures their indexes exist.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SolarGrid.Domain.Entities;

namespace SolarGrid.Infrastructure.Persistence
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        // Builds the Mongo client and resolves the target database once, on startup (registered as a singleton).
        public MongoDbContext(IOptions<MongoDbSettings> settings)
        {
            MongoMappings.Register();
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");

        public IMongoCollection<Prosumer> Prosumers => _database.GetCollection<Prosumer>("Prosumers");

        public IMongoCollection<MicrogridNode> Nodes => _database.GetCollection<MicrogridNode>("Nodes");

        public IMongoCollection<EnergyReservation> Reservations =>
            _database.GetCollection<EnergyReservation>("Reservations");

        public IMongoCollection<EnergyBookingSlot> BookingSlots =>
            _database.GetCollection<EnergyBookingSlot>("EnergyBookingSlots");

        // Unique usernames/emails for staff, plus lookup indexes for the common reservation queries.
        public async Task EnsureIndexesAsync()
        {
            var unique = new CreateIndexOptions { Unique = true };
            await Users.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(u => u.Username), unique),
                new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(u => u.Email), unique)
            });

            await Reservations.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<EnergyReservation>(Builders<EnergyReservation>.IndexKeys
                    .Ascending(r => r.ProsumerNIC).Ascending(r => r.SlotTime)),
                new CreateIndexModel<EnergyReservation>(Builders<EnergyReservation>.IndexKeys
                    .Ascending(r => r.NodeId).Ascending(r => r.SlotTime)),
                new CreateIndexModel<EnergyReservation>(Builders<EnergyReservation>.IndexKeys.Ascending(r => r.QrToken)),
                new CreateIndexModel<EnergyReservation>(Builders<EnergyReservation>.IndexKeys.Ascending(r => r.SlotId))
            });

            await BookingSlots.Indexes.CreateOneAsync(new CreateIndexModel<EnergyBookingSlot>(
                Builders<EnergyBookingSlot>.IndexKeys.Ascending(s => s.NodeId).Ascending(s => s.StartTime)));
        }
    }
}

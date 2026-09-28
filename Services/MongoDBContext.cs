/*
 * File: MongoDBContext.cs
 * Purpose: Creates and exposes the MongoDB database connection using settings from appsettings.json.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SolarGrid.Models;

namespace SolarGrid.Services
{
    public class MongoDBContext
    {
        private readonly IMongoDatabase _database;

        // Builds the Mongo client and resolves the target database once, on startup (registered as a singleton).
        public MongoDBContext(IOptions<MongoDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<Prosumer> Prosumers =>
            _database.GetCollection<Prosumer>("Prosumers");

        public IMongoCollection<MicrogridNode> Nodes =>
            _database.GetCollection<MicrogridNode>("Nodes");

        public IMongoCollection<EnergyReservation> Reservations =>
            _database.GetCollection<EnergyReservation>("Reservations");
    }
}

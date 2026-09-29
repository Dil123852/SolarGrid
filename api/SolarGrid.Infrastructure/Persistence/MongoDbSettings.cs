/*
 * File: MongoDbSettings.cs
 * Purpose: Strongly-typed binding for the "MongoDB" section of appsettings.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

namespace SolarGrid.Infrastructure.Persistence
{
    // Holds the MongoDB connection string and target database name, injected via IOptions<MongoDbSettings>.
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
    }
}

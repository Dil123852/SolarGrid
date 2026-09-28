/*
 * File: MongoDBSettings.cs
 * Purpose: Strongly-typed binding for the MongoDB connection settings section in appsettings.json.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

namespace SolarGrid.Models
{
    // Holds the MongoDB connection string and target database name, injected via IOptions<MongoDBSettings>.
    public class MongoDBSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
    }
}

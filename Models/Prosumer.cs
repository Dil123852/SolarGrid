/*
 * File: Prosumer.cs
 * Purpose: Data model for a Solar Prosumer (property owner with solar panel arrays).
 *          NIC (National Identity Card) is used as the primary key, per assignment spec.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SolarGrid.Models
{
    public class Prosumer
    {
        // NIC is the primary key for prosumer records (assignment requirement).
        // BsonRepresentation(String) stops the driver from trying to parse this as an ObjectId.
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string NIC { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        // False once deactivated; only a Backoffice user can flip this back to true.
        public bool IsActive { get; set; } = true;
    }
}

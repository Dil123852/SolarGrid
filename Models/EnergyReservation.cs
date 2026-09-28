/*
 * File: EnergyReservation.cs
 * Purpose: Data model for a power trading / energy slot reservation made by a Prosumer.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SolarGrid.Models
{
    public class EnergyReservation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        // Links to Prosumer.NIC.
        public string ProsumerNIC { get; set; } = string.Empty;

        // Links to MicrogridNode.Id.
        public string NodeId { get; set; } = string.Empty;

        // The requested date/time for the energy slot.
        public DateTime SlotTime { get; set; }

        // "Pending", "Approved", "Cancelled", or "Completed".
        public string Status { get; set; } = "Pending";

        // Populated once approved; scanned by a Grid Operator in the mobile app's Operator Mode.
        public string? QrToken { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

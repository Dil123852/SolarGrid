/*
 * File: Prosumer.cs
 * Purpose: Domain entity for a Solar Prosumer (property owner with solar panel arrays).
 *          NIC (National Identity Card) is the primary key, per assignment spec.
 *          Persistence mapping lives in Infrastructure, keeping this class database-agnostic.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

namespace SolarGrid.Domain.Entities
{
    public class Prosumer
    {
        // NIC is the primary key for prosumer records (assignment requirement).
        public string NIC { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        // False once deactivated; only a Backoffice user can flip this back to true.
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

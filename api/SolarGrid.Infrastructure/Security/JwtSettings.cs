/*
 * File: JwtSettings.cs
 * Purpose: Strongly-typed binding for the "Jwt" section of appsettings (signing key, issuer, lifetime).
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

namespace SolarGrid.Infrastructure.Security
{
    public class JwtSettings
    {
        // HMAC-SHA256 key; must be at least 32 characters. Real value lives in appsettings.Local.json.
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = "SolarGridAPI";
        public string Audience { get; set; } = "SolarGridClients";
        public int ExpiryMinutes { get; set; } = 480;
    }
}

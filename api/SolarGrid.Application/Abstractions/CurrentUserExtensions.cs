/*
 * File: CurrentUserExtensions.cs
 * Purpose: Ownership checks shared by services (Backoffice, or the prosumer who owns the record).
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Domain.Enums;

namespace SolarGrid.Application.Abstractions
{
    public static class CurrentUserExtensions
    {
        // Backoffice can act on any prosumer; a prosumer only on their own record.
        public static bool CanAccessProsumer(this ICurrentUser user, string nic) =>
            user.Role == UserRole.Backoffice ||
            (user.Role == UserRole.Prosumer && string.Equals(user.Nic, nic, StringComparison.OrdinalIgnoreCase));
    }
}

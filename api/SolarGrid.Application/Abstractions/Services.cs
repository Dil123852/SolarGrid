/*
 * File: Services.cs
 * Purpose: Cross-cutting ports (hashing, tokens, clock, caller identity) implemented outside
 *          the application layer so business logic stays free of framework dependencies.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Domain.Enums;

namespace SolarGrid.Application.Abstractions
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string hash, string password);
    }

    public interface ITokenService
    {
        // Issues a signed JWT; nic is only set for prosumers.
        (string Token, DateTime ExpiresAt) CreateToken(string subject, string displayName, UserRole role, string? nic);
    }

    public interface IClock
    {
        DateTime UtcNow { get; }
    }

    // The authenticated caller of the current request (anonymous when IsAuthenticated is false).
    public interface ICurrentUser
    {
        bool IsAuthenticated { get; }
        UserRole? Role { get; }
        string? Nic { get; }
    }

    public static class CurrentUserExtensions
    {
        // Backoffice can act on any prosumer; a prosumer only on their own record.
        public static bool CanAccessProsumer(this ICurrentUser user, string nic) =>
            user.Role == UserRole.Backoffice ||
            (user.Role == UserRole.Prosumer && string.Equals(user.Nic, nic, StringComparison.OrdinalIgnoreCase));
    }
}

/*
 * File: ICurrentUser.cs
 * Purpose: Port describing the authenticated caller of the current request.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Domain.Enums;

namespace SolarGrid.Application.Abstractions
{
    // The authenticated caller of the current request (anonymous when IsAuthenticated is false).
    public interface ICurrentUser
    {
        bool IsAuthenticated { get; }
        UserRole? Role { get; }
        string? Nic { get; }
    }
}

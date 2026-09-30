/*
 * File: ITokenService.cs
 * Purpose: Port for issuing signed access tokens.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Domain.Enums;

namespace SolarGrid.Application.Abstractions
{
    public interface ITokenService
    {
        // Issues a signed JWT; nic is only set for prosumers.
        (string Token, DateTime ExpiresAt) CreateToken(string subject, string displayName, UserRole role, string? nic);
    }
}

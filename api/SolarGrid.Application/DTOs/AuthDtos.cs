/*
 * File: AuthDtos.cs
 * Purpose: Request/response contracts for login and staff account management.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Domain.Enums;

namespace SolarGrid.Application.DTOs
{
    public record LoginRequest(string Username, string Password);

    public record ProsumerLoginRequest(string Nic, string Password);

    public record AuthResponse(string Token, DateTime ExpiresAt, UserRole Role, string DisplayName, string? Nic);

    public record RegisterStaffRequest(string Username, string Email, string Password, UserRole Role);

    public record UserResponse(string Id, string Username, string Email, UserRole Role, bool IsActive, DateTime CreatedAt);
}

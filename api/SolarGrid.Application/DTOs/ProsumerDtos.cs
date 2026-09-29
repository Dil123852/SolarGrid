/*
 * File: ProsumerDtos.cs
 * Purpose: Request/response contracts for prosumer registration and profile management.
 *          Responses never expose the password hash.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

namespace SolarGrid.Application.DTOs
{
    public record RegisterProsumerRequest(string Nic, string Name, string Email, string Phone, string Address, string Password);

    public record UpdateProsumerRequest(string Name, string Email, string Phone, string Address);

    public record ProsumerResponse(string Nic, string Name, string Email, string Phone, string Address, bool IsActive, DateTime CreatedAt);
}

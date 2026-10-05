/*
 * File: CurrentUser.cs
 * Purpose: Adapts the authenticated HTTP request's JWT claims to the application's ICurrentUser port.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Application.Abstractions;
using SolarGrid.Domain.Enums;
using SolarGrid.Infrastructure.Security;

namespace SolarGrid.Api.Security
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _accessor;

        // Receives the HTTP context accessor so the current request's claims can be read.
        public CurrentUser(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public bool IsAuthenticated => _accessor.HttpContext?.User.Identity?.IsAuthenticated == true;

        public UserRole? Role =>
            Enum.TryParse<UserRole>(_accessor.HttpContext?.User.FindFirst(JwtTokenService.RoleClaim)?.Value, out var role)
                ? role
                : null;

        public string? Nic => _accessor.HttpContext?.User.FindFirst(JwtTokenService.NicClaim)?.Value;
    }
}

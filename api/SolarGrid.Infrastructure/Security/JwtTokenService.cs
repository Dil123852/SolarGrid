/*
 * File: JwtTokenService.cs
 * Purpose: Issues signed JWTs carrying the caller's subject, display name, role and (for
 *          prosumers) NIC. Short claim names ("role", "nic") keep tokens readable by clients.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SolarGrid.Application.Abstractions;
using SolarGrid.Domain.Enums;

namespace SolarGrid.Infrastructure.Security
{
    public class JwtTokenService : ITokenService
    {
        public const string RoleClaim = "role";
        public const string NameClaim = "name";
        public const string NicClaim = "nic";

        private readonly JwtSettings _settings;

        // Receives the JWT settings (key, issuer, audience, lifetime).
        public JwtTokenService(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        // Issues a signed JWT with subject, name, role and (for prosumers) NIC claims.
        public (string Token, DateTime ExpiresAt) CreateToken(string subject, string displayName, UserRole role, string? nic)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, subject),
                new(NameClaim, displayName),
                new(RoleClaim, role.ToString())
            };
            if (!string.IsNullOrEmpty(nic)) claims.Add(new Claim(NicClaim, nic));

            var expires = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _settings.Issuer,
                Audience = _settings.Audience,
                Expires = expires,
                SigningCredentials = new SigningCredentials(CreateKey(_settings.Key), SecurityAlgorithms.HmacSha256)
            };

            return (new JsonWebTokenHandler().CreateToken(descriptor), expires);
        }

        // Builds the HMAC signing key from the configured secret.
        public static SymmetricSecurityKey CreateKey(string key) => new(Encoding.UTF8.GetBytes(key));
    }
}

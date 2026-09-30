/*
 * File: AuthenticationExtensions.cs
 * Purpose: JWT bearer authentication with short "role"/"name" claims, validated against the
 *          signing key, issuer and audience from configuration. Fails fast on a missing key.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SolarGrid.Infrastructure.Security;

namespace SolarGrid.Api.Extensions
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwt = configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
            if (jwt.Key.Length < 32 || jwt.Key.StartsWith("SET-IN"))
                throw new InvalidOperationException("Jwt:Key must be set to a random string of at least 32 characters.");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwt.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwt.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = JwtTokenService.CreateKey(jwt.Key),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(1),
                        RoleClaimType = JwtTokenService.RoleClaim,
                        NameClaimType = JwtTokenService.NameClaim
                    };
                });

            services.AddAuthorization();
            return services;
        }
    }
}

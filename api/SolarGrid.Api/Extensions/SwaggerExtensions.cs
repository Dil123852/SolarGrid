/*
 * File: SwaggerExtensions.cs
 * Purpose: Swagger UI at /swagger with an Authorize button for pasting a JWT.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using Microsoft.OpenApi;

namespace SolarGrid.Api.Extensions
{
    public static class SwaggerExtensions
    {
        private const string SchemeName = "Bearer";

        public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SolarGrid API", Version = "v1" });
                c.AddSecurityDefinition(SchemeName, new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Paste the token from /api/auth/login or /api/auth/prosumer-login."
                });
                c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(SchemeName, document)] = []
                });
            });
            return services;
        }
    }
}

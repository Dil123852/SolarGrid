/*
 * File: DependencyInjection.cs
 * Purpose: Registers MongoDB persistence, repositories, JWT and hashing adapters with the DI container.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SolarGrid.Application.Abstractions;
using SolarGrid.Infrastructure.Persistence;
using SolarGrid.Infrastructure.Persistence.Repositories;
using SolarGrid.Infrastructure.Security;

namespace SolarGrid.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MongoDbSettings>(configuration.GetSection("MongoDB"));
            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

            services.AddSingleton<MongoDbContext>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IProsumerRepository, ProsumerRepository>();
            services.AddScoped<INodeRepository, NodeRepository>();
            services.AddScoped<IReservationRepository, ReservationRepository>();

            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddSingleton<ITokenService, JwtTokenService>();
            services.AddSingleton<IClock, SystemClock>();
            return services;
        }
    }
}

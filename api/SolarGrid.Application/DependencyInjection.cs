/*
 * File: DependencyInjection.cs
 * Purpose: Registers the application-layer services with the DI container.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using Microsoft.Extensions.DependencyInjection;
using SolarGrid.Application.Services;

namespace SolarGrid.Application
{
    public static class DependencyInjection
    {
        // Registers the application services (all business logic) with the DI container.
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<AuthService>();
            services.AddScoped<ProsumerService>();
            services.AddScoped<NodeService>();
            services.AddScoped<ReservationService>();
            services.AddScoped<DashboardService>();
            return services;
        }
    }
}

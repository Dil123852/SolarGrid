/*
 * File: SeedTestData.cs
 * Purpose: Create test users for all three roles (Backoffice, GridOperator, Prosumer)
 *          for development and testing purposes.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Application.Services;
using SolarGrid.Application.DTOs;
using SolarGrid.Domain.Enums;

namespace SolarGrid.Api.Extensions
{
    public static class SeedTestData
    {
        public static async Task SeedTestUsersAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(SeedTestData));
            var authService = scope.ServiceProvider.GetRequiredService<AuthService>();
            var prosumerService = scope.ServiceProvider.GetRequiredService<ProsumerService>();

            try
            {
                // Seed staff users (Backoffice and GridOperator)
                var staffUsers = new[]
                {
                    new { Username = "backoffice1", Email = "backoffice1@solargrid.lk", Password = "Test@12345", Role = "Backoffice" },
                    new { Username = "backoffice2", Email = "backoffice2@solargrid.lk", Password = "Test@12345", Role = "Backoffice" },
                    new { Username = "gridoperator1", Email = "gridoperator1@solargrid.lk", Password = "Test@12345", Role = "GridOperator" },
                    new { Username = "gridoperator2", Email = "gridoperator2@solargrid.lk", Password = "Test@12345", Role = "GridOperator" },
                };

                foreach (var user in staffUsers)
                {
                    var role = user.Role == "Backoffice" ? UserRole.Backoffice : UserRole.GridOperator;
                    var request = new RegisterStaffRequest(
                        user.Username,
                        user.Email,
                        user.Password,
                        role
                    );

                    var result = await authService.RegisterStaffAsync(request);
                    if (result.IsSuccess)
                    {
                        logger.LogInformation("Seeded staff user '{Username}' with role '{Role}'.", user.Username, user.Role);
                    }
                    else
                    {
                        logger.LogWarning("Failed to seed staff user '{Username}': {Error}", user.Username, result.Error);
                    }
                }

                // Seed prosumer users
                var prosumers = new[]
                {
                    new { Nic = "199801011234", Name = "Prosumer One", Email = "prosumer1@example.lk", Phone = "+94701234567", Address = "123 Solar Street, Colombo", Password = "Test@12345" },
                    new { Nic = "199802022345", Name = "Prosumer Two", Email = "prosumer2@example.lk", Phone = "+94702234567", Address = "456 Energy Ave, Kandy", Password = "Test@12345" },
                    new { Nic = "199803033456", Name = "Prosumer Three", Email = "prosumer3@example.lk", Phone = "+94703234567", Address = "789 Grid Lane, Galle", Password = "Test@12345" },
                };

                foreach (var prosumer in prosumers)
                {
                    var request = new RegisterProsumerRequest(
                        prosumer.Nic,
                        prosumer.Name,
                        prosumer.Email,
                        prosumer.Phone,
                        prosumer.Address,
                        prosumer.Password
                    );

                    var result = await prosumerService.RegisterAsync(request);
                    if (result.IsSuccess)
                    {
                        logger.LogInformation("Seeded prosumer '{Name}' (NIC: {Nic}).", prosumer.Name, prosumer.Nic);
                    }
                    else
                    {
                        logger.LogWarning("Failed to seed prosumer '{Name}' (NIC: {Nic}): {Error}", prosumer.Name, prosumer.Nic, result.Error);
                    }
                }

                logger.LogInformation("Test data seeding completed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Test data seeding failed.");
            }
        }
    }
}

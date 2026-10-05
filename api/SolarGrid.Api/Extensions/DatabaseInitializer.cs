/*
 * File: DatabaseInitializer.cs
 * Purpose: First-start tasks - creates MongoDB indexes and the initial Backoffice account.
 *          Failures are logged, not fatal, so Swagger still comes up and shows the real error.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Application.Services;
using SolarGrid.Infrastructure.Persistence;

namespace SolarGrid.Api.Extensions
{
    public static class DatabaseInitializer
    {
        // Creates MongoDB indexes and seeds the first Backoffice account at startup; logs instead of crashing on failure.
        public static async Task InitialiseDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DatabaseInitializer));
            try
            {
                await scope.ServiceProvider.GetRequiredService<MongoDbContext>().EnsureIndexesAsync();

                var seed = app.Configuration.GetSection("Seed");
                var username = seed["BackofficeUsername"] ?? "admin";
                var seeded = await scope.ServiceProvider.GetRequiredService<AuthService>().SeedBackofficeAsync(
                    username,
                    seed["BackofficeEmail"] ?? "admin@solargrid.lk",
                    seed["BackofficePassword"] ?? string.Empty);
                if (seeded) logger.LogInformation("Seeded initial Backoffice user '{User}'.", username);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database initialisation failed.");
            }
        }
    }
}

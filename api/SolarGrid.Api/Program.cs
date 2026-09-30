/*
 * File: Program.cs
 * Purpose: Composition root - loads configuration (including git-ignored local secrets), wires the
 *          Application and Infrastructure layers, then builds the HTTP pipeline. Each concern lives
 *          in its own extension class under Extensions/.
 *          Uses an explicit static Main method (rather than top-level statements) so
 *          the entry point cannot be lost regardless of build/publish configuration.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Api.Extensions;
using SolarGrid.Api.Security;
using SolarGrid.Application;
using SolarGrid.Application.Abstractions;
using SolarGrid.Infrastructure;

namespace SolarGrid.Api
{
    public class Program
    {
        private const string CorsPolicy = "AllowClients";

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Real connection string, JWT key and seed password live here (git-ignored).
            builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

            // Layers: Api -> Application (+ Infrastructure for wiring only).
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUser, CurrentUser>();

            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddApiControllers();
            builder.Services.AddSwaggerWithJwt();

            // The web and mobile clients run on different origins from the API.
            builder.Services.AddCors(o => o.AddPolicy(CorsPolicy, p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

            var app = builder.Build();

            app.UseJsonExceptionHandler();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseCors(CorsPolicy);
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            await app.InitialiseDatabaseAsync();
            await app.RunAsync();
        }
    }
}

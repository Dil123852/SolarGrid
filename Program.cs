/*
 * File: Program.cs
 * Purpose: Application entry point - configures MongoDB settings, registers services,
 *          and wires up the controller pipeline for the SolarGrid Web API.
 *          Uses an explicit static Main method (rather than top-level statements) so
 *          the entry point cannot be lost regardless of build/publish configuration.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Models;
using SolarGrid.Services;

namespace SolarGrid
{
    public class Program
    {
        // Explicit entry point - guarantees the compiler always finds a valid Main,
        // in Debug or Release, build or publish.
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Bind the "MongoDB" section of appsettings.json to MongoDBSettings.
            builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));

            // Register the Mongo context and business-logic services for dependency injection.
            builder.Services.AddSingleton<MongoDBContext>();
            builder.Services.AddScoped<ProsumerService>();
            builder.Services.AddScoped<MicrogridNodeService>();
            builder.Services.AddScoped<ReservationService>();

            // Standard controller + Swagger setup, so endpoints can be tested directly in a browser.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Allows the separate web app and mobile app (different origins) to call this API.
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });

            var app = builder.Build();

            // Swagger UI available at /swagger - lets you test every endpoint without Postman.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseCors("AllowAll");
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}

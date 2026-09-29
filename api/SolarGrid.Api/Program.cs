/*
 * File: Program.cs
 * Purpose: Composition root - loads configuration (including git-ignored local secrets), wires the
 *          Application and Infrastructure layers, JWT bearer auth with role claims, CORS, Swagger,
 *          and runs first-start tasks (Mongo indexes, Backoffice seed account).
 *          Uses an explicit static Main method (rather than top-level statements) so
 *          the entry point cannot be lost regardless of build/publish configuration.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SolarGrid.Api.Security;
using SolarGrid.Application;
using SolarGrid.Application.Abstractions;
using SolarGrid.Application.Services;
using SolarGrid.Infrastructure;
using SolarGrid.Infrastructure.Persistence;
using SolarGrid.Infrastructure.Security;

namespace SolarGrid.Api
{
    public class Program
    {
        // Explicit entry point - guarantees the compiler always finds a valid Main,
        // in Debug or Release, build or publish.
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Real connection string, JWT key and seed password live here (git-ignored).
            builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

            var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
            if (jwt.Key.Length < 32 || jwt.Key.StartsWith("SET-IN"))
                throw new InvalidOperationException("Jwt:Key must be set to a random string of at least 32 characters.");

            // Layers: Api -> Application (+ Infrastructure for wiring only).
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUser, CurrentUser>();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            builder.Services.AddAuthorization();

            // Enums travel as strings ("Pending", "Backoffice") so clients never deal with magic numbers.
            builder.Services.AddControllers()
                .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            // Swagger UI at /swagger, with an Authorize button for pasting a JWT.
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SolarGrid API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Paste the token from /api/auth/login or /api/auth/prosumer-login."
                });
                c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
            });

            // Allows the separate web app and mobile app (different origins) to call this API.
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });

            var app = builder.Build();

            // Unhandled exceptions become a plain { message } 500 instead of a stack trace.
            app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new { message = "An unexpected server error occurred." });
            }));

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            await InitialiseDatabaseAsync(app);
            await app.RunAsync();
        }

        // Creates indexes and the first Backoffice account. Failures are logged, not fatal,
        // so Swagger still comes up and shows the real error on the first request.
        private static async Task InitialiseDatabaseAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            try
            {
                await scope.ServiceProvider.GetRequiredService<MongoDbContext>().EnsureIndexesAsync();

                var seed = app.Configuration.GetSection("Seed");
                var seeded = await scope.ServiceProvider.GetRequiredService<AuthService>().SeedBackofficeAsync(
                    seed["BackofficeUsername"] ?? "admin",
                    seed["BackofficeEmail"] ?? "admin@solargrid.lk",
                    seed["BackofficePassword"] ?? string.Empty);
                if (seeded) logger.LogInformation("Seeded initial Backoffice user '{User}'.", seed["BackofficeUsername"]);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database initialisation failed.");
            }
        }
    }
}

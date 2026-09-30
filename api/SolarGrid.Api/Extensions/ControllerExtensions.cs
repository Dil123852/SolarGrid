/*
 * File: ControllerExtensions.cs
 * Purpose: MVC controller setup - enums as strings, and every invalid request body answered
 *          with the same { message } shape the business-rule failures use.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace SolarGrid.Api.Extensions
{
    public static class ControllerExtensions
    {
        public static IServiceCollection AddApiControllers(this IServiceCollection services)
        {
            // Field validation lives in the services, so MVC's implicit [Required] is switched off.
            services.AddControllers(o => o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
                .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
                .ConfigureApiBehaviorOptions(o => o.InvalidModelStateResponseFactory = context =>
                {
                    var first = context.ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                    return new BadRequestObjectResult(new
                    {
                        message = string.IsNullOrWhiteSpace(first) ? "The request is invalid." : $"Invalid request: {first}"
                    });
                });
            return services;
        }

        // Unhandled exceptions become a plain { message } 500 instead of a stack trace.
        public static IApplicationBuilder UseJsonExceptionHandler(this IApplicationBuilder app) =>
            app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new { message = "An unexpected server error occurred." });
            }));
    }
}

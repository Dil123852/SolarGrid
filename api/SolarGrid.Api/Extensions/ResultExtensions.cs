/*
 * File: ResultExtensions.cs
 * Purpose: Maps application Results to HTTP responses. Failures always return { message, code }:
 *          clients show the message verbatim and may branch on the code (e.g. AccountInactive).
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using Microsoft.AspNetCore.Mvc;
using SolarGrid.Application.Common;

namespace SolarGrid.Api.Extensions
{
    public static class ResultExtensions
    {
        // Turns a Result without a value into 200 { message } or the matching error response.
        public static IActionResult ToActionResult(this Result result) =>
            result.IsSuccess ? new OkObjectResult(new { message = result.Message }) : Failure(result);

        // Turns a Result<T> into 200 with the value, or the matching error response.
        public static IActionResult ToActionResult<T>(this Result<T> result) =>
            result.IsSuccess ? new OkObjectResult(result.Value) : Failure(result);

        // Builds the { message, code } error body with the HTTP status for the error type.
        private static IActionResult Failure(Result result) =>
            new ObjectResult(new { message = result.Message, code = result.Error.ToString() })
            {
                StatusCode = result.Error switch
                {
                    ErrorType.Validation => StatusCodes.Status400BadRequest,
                    ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                    ErrorType.Forbidden or ErrorType.AccountInactive => StatusCodes.Status403Forbidden,
                    ErrorType.NotFound => StatusCodes.Status404NotFound,
                    ErrorType.Conflict => StatusCodes.Status409Conflict,
                    _ => StatusCodes.Status500InternalServerError
                }
            };
    }
}

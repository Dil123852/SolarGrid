/*
 * File: ResultExtensions.cs
 * Purpose: Maps application Results to HTTP responses. Failures always return { message } so
 *          both clients can show the API's business-rule message verbatim.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using Microsoft.AspNetCore.Mvc;
using SolarGrid.Application.Common;

namespace SolarGrid.Api.Extensions
{
    public static class ResultExtensions
    {
        public static IActionResult ToActionResult(this Result result) =>
            result.IsSuccess ? new OkObjectResult(new { message = result.Message }) : Failure(result);

        public static IActionResult ToActionResult<T>(this Result<T> result) =>
            result.IsSuccess ? new OkObjectResult(result.Value) : Failure(result);

        private static IActionResult Failure(Result result) =>
            new ObjectResult(new { message = result.Message })
            {
                StatusCode = result.Error switch
                {
                    ErrorType.Validation => StatusCodes.Status400BadRequest,
                    ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                    ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                    ErrorType.NotFound => StatusCodes.Status404NotFound,
                    ErrorType.Conflict => StatusCodes.Status409Conflict,
                    _ => StatusCodes.Status500InternalServerError
                }
            };
    }
}

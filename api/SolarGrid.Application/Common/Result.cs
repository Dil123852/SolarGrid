/*
 * File: Result.cs
 * Purpose: Outcome of an application-service call - success, or a typed failure the API layer
 *          maps to an HTTP status (400/401/403/404/409) without services knowing about HTTP.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

namespace SolarGrid.Application.Common
{
    public enum ErrorType
    {
        None,
        Validation,
        NotFound,
        Conflict,
        Unauthorized,
        Forbidden,

        // Deactivated prosumer or disabled staff account (403, but distinguishable by clients).
        AccountInactive
    }

    public class Result
    {
        public bool IsSuccess => Error == ErrorType.None;
        public ErrorType Error { get; }
        public string Message { get; }

        // Creates a result with an error type (None means success) and a message.
        protected Result(ErrorType error, string message)
        {
            Error = error;
            Message = message;
        }

        // Creates a successful result without a value.
        public static Result Ok(string message = "OK") => new(ErrorType.None, message);
        // Creates a failed result without a value.
        public static Result Fail(ErrorType error, string message) => new(error, message);

        // Creates a successful result carrying a value.
        public static Result<T> Ok<T>(T value, string message = "OK") => new(value, ErrorType.None, message);
        // Creates a failed result for a value-returning operation.
        public static Result<T> Fail<T>(ErrorType error, string message) => new(default, error, message);

        // Shorthands for the most common failures.
        public static Result<T> Invalid<T>(string message) => Fail<T>(ErrorType.Validation, message);
        // Shortcut for a not-found failure.
        public static Result<T> NotFound<T>(string message) => Fail<T>(ErrorType.NotFound, message);
        // Shortcut for a forbidden failure (the caller may not access the resource).
        public static Result<T> Forbidden<T>(string message = "You are not allowed to access this resource.") =>
            Fail<T>(ErrorType.Forbidden, message);
    }

    public class Result<T> : Result
    {
        public T? Value { get; }

        // Creates a result that carries a value on success.
        internal Result(T? value, ErrorType error, string message) : base(error, message)
        {
            Value = value;
        }
    }
}

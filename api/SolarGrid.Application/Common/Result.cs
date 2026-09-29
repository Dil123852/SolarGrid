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
        Forbidden
    }

    public class Result
    {
        public bool IsSuccess => Error == ErrorType.None;
        public ErrorType Error { get; }
        public string Message { get; }

        protected Result(ErrorType error, string message)
        {
            Error = error;
            Message = message;
        }

        public static Result Ok(string message = "OK") => new(ErrorType.None, message);
        public static Result Fail(ErrorType error, string message) => new(error, message);

        public static Result<T> Ok<T>(T value, string message = "OK") => new(value, ErrorType.None, message);
        public static Result<T> Fail<T>(ErrorType error, string message) => new(default, error, message);

        // Shorthands for the most common failures.
        public static Result<T> Invalid<T>(string message) => Fail<T>(ErrorType.Validation, message);
        public static Result<T> NotFound<T>(string message) => Fail<T>(ErrorType.NotFound, message);
        public static Result<T> Forbidden<T>(string message = "You are not allowed to access this resource.") =>
            Fail<T>(ErrorType.Forbidden, message);
    }

    public class Result<T> : Result
    {
        public T? Value { get; }

        internal Result(T? value, ErrorType error, string message) : base(error, message)
        {
            Value = value;
        }
    }
}

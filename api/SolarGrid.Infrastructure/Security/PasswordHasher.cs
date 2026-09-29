/*
 * File: PasswordHasher.cs
 * Purpose: Salted PBKDF2 password hashing via ASP.NET Core Identity's PasswordHasher.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using Microsoft.AspNetCore.Identity;
using SolarGrid.Application.Abstractions;

namespace SolarGrid.Infrastructure.Security
{
    public class PasswordHasher : IPasswordHasher
    {
        // The Identity hasher ignores the user argument, so a shared placeholder is fine.
        private static readonly object HashUser = new();
        private readonly PasswordHasher<object> _inner = new();

        public string Hash(string password) => _inner.HashPassword(HashUser, password);

        public bool Verify(string hash, string password) =>
            _inner.VerifyHashedPassword(HashUser, hash, password) != PasswordVerificationResult.Failed;
    }
}

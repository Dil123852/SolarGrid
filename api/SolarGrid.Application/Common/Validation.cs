/*
 * File: Validation.cs
 * Purpose: Small input checks shared by services (NIC format, email, password strength).
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using System.Text.RegularExpressions;

namespace SolarGrid.Application.Common
{
    public static partial class Validation
    {
        // Sri Lankan NIC: old format 9 digits + V/X, or new format 12 digits.
        [GeneratedRegex(@"^(\d{9}[VvXx]|\d{12})$")]
        private static partial Regex NicRegex();

        [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
        private static partial Regex EmailRegex();

        public const int MinPasswordLength = 6;

        public static bool IsValidNic(string? nic) => nic != null && NicRegex().IsMatch(nic);

        public static bool IsValidEmail(string? email) => email != null && EmailRegex().IsMatch(email);

        public static string NormalizeNic(string nic) => nic.Trim().ToUpperInvariant();
    }
}

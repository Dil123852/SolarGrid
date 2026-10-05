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

        // Compiled email pattern (source-generated regex).
        [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
        private static partial Regex EmailRegex();

        public const int MinPasswordLength = 6;

        // True when the NIC is 9 digits + V/X or 12 digits.
        public static bool IsValidNic(string? nic) => nic != null && NicRegex().IsMatch(nic);

        // True when the value looks like an email address.
        public static bool IsValidEmail(string? email) => email != null && EmailRegex().IsMatch(email);

        // Trims and upper-cases a NIC so 991234567v and 991234567V are the same key.
        public static string NormalizeNic(string nic) => nic.Trim().ToUpperInvariant();
    }
}

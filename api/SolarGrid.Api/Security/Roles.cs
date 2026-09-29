/*
 * File: Roles.cs
 * Purpose: Role-name constants for [Authorize(Roles = ...)] attributes, kept in sync with UserRole.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Domain.Enums;

namespace SolarGrid.Api.Security
{
    public static class Roles
    {
        public const string Backoffice = nameof(UserRole.Backoffice);
        public const string GridOperator = nameof(UserRole.GridOperator);
        public const string Prosumer = nameof(UserRole.Prosumer);

        public const string Staff = Backoffice + "," + GridOperator;
        public const string BackofficeOrProsumer = Backoffice + "," + Prosumer;
    }
}

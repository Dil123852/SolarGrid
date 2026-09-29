/*
 * File: UserRole.cs
 * Purpose: Roles used for JWT claims and endpoint authorization.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

namespace SolarGrid.Domain.Enums
{
    // Backoffice and GridOperator are staff (User collection); Prosumer logs in with NIC (Prosumer collection).
    public enum UserRole
    {
        Backoffice,
        GridOperator,
        Prosumer
    }
}

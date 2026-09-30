/*
 * File: IClock.cs
 * Purpose: Port for the current UTC time, so rules are testable with a fixed clock.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

namespace SolarGrid.Application.Abstractions
{
    public interface IClock
    {
        DateTime UtcNow { get; }
    }
}

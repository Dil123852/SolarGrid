/*
 * File: SystemClock.cs
 * Purpose: Real-time IClock implementation (tests substitute a fixed clock).
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Application.Abstractions;

namespace SolarGrid.Infrastructure
{
    public class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}

/*
 * File: ReservationStatus.cs
 * Purpose: Lifecycle states of an energy reservation.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

namespace SolarGrid.Domain.Enums
{
    // Pending -> Approved (QR issued) -> Completed (QR scanned), or Pending/Approved -> Cancelled.
    public enum ReservationStatus
    {
        Pending,
        Approved,
        Cancelled,
        Completed
    }
}

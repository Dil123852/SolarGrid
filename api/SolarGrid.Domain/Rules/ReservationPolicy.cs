/*
 * File: ReservationPolicy.cs
 * Purpose: Pure business rules for energy reservations - the 7-day booking window, the 12-hour
 *          notice period for updates/cancellations, approval eligibility, and which reservations
 *          block a node from being deactivated. No I/O, so every rule is unit-testable.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Domain.Entities;
using SolarGrid.Domain.Enums;

namespace SolarGrid.Domain.Rules
{
    public static class ReservationPolicy
    {
        public static readonly TimeSpan BookingWindow = TimeSpan.FromDays(7);
        public static readonly TimeSpan MinimumNotice = TimeSpan.FromHours(12);

        // A slot must be in the future and no more than 7 days ahead.
        public static bool IsWithinBookingWindow(DateTime slotTime, DateTime now) =>
            slotTime > now && slotTime <= now.Add(BookingWindow);

        // Updates and cancellations need at least 12 hours before the slot starts.
        public static bool HasMinimumNotice(DateTime slotTime, DateTime now) =>
            slotTime - now >= MinimumNotice;

        // Only pending or approved reservations can still be changed by the prosumer.
        public static bool IsModifiable(ReservationStatus status) =>
            status is ReservationStatus.Pending or ReservationStatus.Approved;

        // Only a pending reservation can be approved (and receive a QR token).
        public static bool CanApprove(ReservationStatus status) =>
            status == ReservationStatus.Pending;

        // A live (pending/approved), not-yet-past reservation prevents node deactivation.
        public static bool BlocksNodeDeactivation(EnergyReservation reservation, DateTime now) =>
            IsModifiable(reservation.Status) && reservation.SlotTime > now;
    }
}

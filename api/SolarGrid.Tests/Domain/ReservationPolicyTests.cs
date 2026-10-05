/*
 * File: ReservationPolicyTests.cs
 * Purpose: Boundary tests for the reservation business rules (7-day window, 12-hour notice,
 *          approval eligibility, node-deactivation blocking).
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Application.Common;
using SolarGrid.Domain.Entities;
using SolarGrid.Domain.Enums;
using SolarGrid.Domain.Rules;

namespace SolarGrid.Tests.Domain
{
    public class ReservationPolicyTests
    {
        private static readonly DateTime Now = new(2026, 9, 29, 12, 0, 0, DateTimeKind.Utc);

        // Checks only slots within the next 7 days are bookable.
        [Theory]
        [InlineData(1, true)]           // one hour ahead
        [InlineData(24 * 7, true)]      // exactly 7 days ahead
        [InlineData(24 * 7 + 1, false)] // just past the window
        [InlineData(0, false)]          // now is not in the future
        [InlineData(-1, false)]         // in the past
        public void BookingWindow_AllowsOnlyTheNextSevenDays(int hoursAhead, bool expected) =>
            Assert.Equal(expected, ReservationPolicy.IsWithinBookingWindow(Now.AddHours(hoursAhead), Now));

        // Checks changes need at least 12 hours' notice.
        [Theory]
        [InlineData(12, true)]
        [InlineData(48, true)]
        [InlineData(11.99, false)]
        [InlineData(1, false)]
        public void MinimumNotice_RequiresTwelveHours(double hoursAhead, bool expected) =>
            Assert.Equal(expected, ReservationPolicy.HasMinimumNotice(Now.AddHours(hoursAhead), Now));

        // Checks only Pending reservations can be approved.
        [Theory]
        [InlineData(ReservationStatus.Pending, true)]
        [InlineData(ReservationStatus.Approved, false)]
        [InlineData(ReservationStatus.Cancelled, false)]
        [InlineData(ReservationStatus.Completed, false)]
        public void CanApprove_OnlyPending(ReservationStatus status, bool expected) =>
            Assert.Equal(expected, ReservationPolicy.CanApprove(status));

        // Checks only live, upcoming reservations block node deactivation.
        [Theory]
        [InlineData(ReservationStatus.Pending, 5, true)]
        [InlineData(ReservationStatus.Approved, 5, true)]
        [InlineData(ReservationStatus.Cancelled, 5, false)]
        [InlineData(ReservationStatus.Completed, 5, false)]
        [InlineData(ReservationStatus.Approved, -5, false)] // past slots no longer block
        public void NodeDeactivation_BlockedOnlyByLiveUpcomingReservations(ReservationStatus status, int hoursAhead, bool expected)
        {
            var reservation = new EnergyReservation { Status = status, SlotTime = Now.AddHours(hoursAhead) };
            Assert.Equal(expected, ReservationPolicy.BlocksNodeDeactivation(reservation, Now));
        }

        // Checks old (9 digits + V/X) and new (12 digits) NIC formats.
        [Theory]
        [InlineData("200012345678", true)]
        [InlineData("991234567V", true)]
        [InlineData("991234567x", true)]
        [InlineData("99123456V", false)]
        [InlineData("abc", false)]
        public void Nic_AcceptsOldAndNewSriLankanFormats(string nic, bool expected) =>
            Assert.Equal(expected, Validation.IsValidNic(nic));
    }
}

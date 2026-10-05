/*
 * File: NodeScheduleTests.cs
 * Purpose: Operating-hours rules for microgrid nodes (Sri Lanka local time, UTC+05:30).
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Domain.Rules;

namespace SolarGrid.Tests.Domain
{
    public class NodeScheduleTests
    {
        // 2026-10-01 04:00 UTC is 09:30 in Sri Lanka.
        private static readonly DateTime NineThirtyLocal = new(2026, 10, 1, 4, 0, 0, DateTimeKind.Utc);

        // Checks which schedules are accepted: both empty, or two valid times with open before close.
        [Theory]
        [InlineData(null, null, true)]
        [InlineData("", "", true)]
        [InlineData("06:00", "18:00", true)]
        [InlineData("18:00", "06:00", false)]
        [InlineData("06:00", null, false)]
        [InlineData("6am", "18:00", false)]
        public void IsValid_RequiresBothTimesInOrder(string? open, string? close, bool expected) =>
            Assert.Equal(expected, NodeSchedule.IsValid(open, close));

        // Checks the UTC slot is converted to local time before comparing with the opening hours.
        [Theory]
        [InlineData("09:00", "17:00", true)]
        [InlineData("09:30", "17:00", true)]
        [InlineData("10:00", "17:00", false)]
        [InlineData("06:00", "09:30", false)]
        public void IsWithinOperatingHours_UsesSriLankaTime(string open, string close, bool expected) =>
            Assert.Equal(expected, NodeSchedule.IsWithinOperatingHours(NineThirtyLocal, open, close));

        // Checks a node without a schedule accepts any time.
        [Fact]
        public void NoSchedule_IsOpenAroundTheClock() =>
            Assert.True(NodeSchedule.IsWithinOperatingHours(NineThirtyLocal, null, null));
    }
}

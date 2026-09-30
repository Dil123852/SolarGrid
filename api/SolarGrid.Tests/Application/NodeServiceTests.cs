/*
 * File: NodeServiceTests.cs
 * Purpose: Node validation and the deactivation rule (blocked only by live, upcoming bookings).
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Application.Common;
using SolarGrid.Application.DTOs;
using SolarGrid.Application.Services;
using SolarGrid.Domain.Entities;
using SolarGrid.Domain.Enums;
using SolarGrid.Tests.Fakes;

namespace SolarGrid.Tests.Application
{
    public class NodeServiceTests
    {
        private static readonly DateTime Now = new(2026, 9, 29, 12, 0, 0, DateTimeKind.Utc);

        private readonly InMemoryNodeRepository _nodes = new();
        private readonly InMemoryReservationRepository _reservations = new();

        public NodeServiceTests()
        {
            _nodes.Items.Add(new MicrogridNode { Id = "node1", Name = "Galle Hub", BatterySlots = 2, CapacityKWh = 40, IsActive = true });
        }

        private NodeService Service() => new(_nodes, _reservations, new FixedClock(Now));

        [Fact]
        public async Task Create_ValidNode_Succeeds()
        {
            var result = await Service().CreateAsync(new NodeRequest("Colombo Hub", 6.9271, 79.8612, 50, 4));

            Assert.True(result.IsSuccess);
            Assert.True(result.Value!.IsActive);
            Assert.Equal(2, _nodes.Items.Count);
        }

        [Theory]
        [InlineData("", 7, 80, 50, 4)]      // no name
        [InlineData("Hub", 95, 80, 50, 4)]  // latitude out of range
        [InlineData("Hub", 7, 190, 50, 4)]  // longitude out of range
        [InlineData("Hub", 7, 80, 0, 4)]    // no capacity
        [InlineData("Hub", 7, 80, 50, 0)]   // no battery slots
        public async Task Create_InvalidNode_IsRejected(string name, double lat, double lng, double capacity, int slots)
        {
            var result = await Service().CreateAsync(new NodeRequest(name, lat, lng, capacity, slots));
            Assert.Equal(ErrorType.Validation, result.Error);
        }

        [Fact]
        public async Task Deactivate_WithPendingUpcomingBooking_IsConflict()
        {
            AddReservation(ReservationStatus.Pending, Now.AddDays(1));

            var result = await Service().DeactivateAsync("node1");

            Assert.Equal(ErrorType.Conflict, result.Error);
            Assert.True(_nodes.Items[0].IsActive);
        }

        [Fact]
        public async Task Deactivate_WithOnlyCompletedOrCancelledBookings_Succeeds()
        {
            AddReservation(ReservationStatus.Completed, Now.AddDays(1));
            AddReservation(ReservationStatus.Cancelled, Now.AddDays(2));

            var result = await Service().DeactivateAsync("node1");

            Assert.True(result.IsSuccess);
            Assert.False(_nodes.Items[0].IsActive);
        }

        [Fact]
        public async Task Deactivate_UnknownNode_IsNotFound()
        {
            var result = await Service().DeactivateAsync("missing");
            Assert.Equal(ErrorType.NotFound, result.Error);
        }

        private void AddReservation(ReservationStatus status, DateTime slot) =>
            _reservations.Items.Add(new EnergyReservation
            {
                Id = Guid.NewGuid().ToString("N"), NodeId = "node1", ProsumerNIC = "991234567V", SlotTime = slot, Status = status
            });
    }
}

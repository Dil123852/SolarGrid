/*
 * File: BookingSlotServiceTests.cs
 * Purpose: Energy booking slot rules - window validation, overlap, capacity limits, protected
 *          updates/deletes - and how reservations are bound to and counted against slots.
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
    public class BookingSlotServiceTests
    {
        // 12:00 UTC = 17:30 Sri Lanka time.
        private static readonly DateTime Now = new(2026, 9, 29, 12, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime Tomorrow = Now.AddDays(1);
        private const string Nic = "991234567V";

        private readonly InMemoryBookingSlotRepository _slots = new();
        private readonly InMemoryNodeRepository _nodes = new();
        private readonly InMemoryReservationRepository _reservations = new();
        private readonly InMemoryProsumerRepository _prosumers = new();
        private readonly FakeCurrentUser _user = new() { Role = UserRole.Prosumer, Nic = Nic };

        // Seeds an active station with 3 battery slots and an active prosumer.
        public BookingSlotServiceTests()
        {
            _nodes.Items.Add(new MicrogridNode { Id = "node1", Name = "Colombo Hub", BatterySlots = 3, IsActive = true });
            _prosumers.Items.Add(new Prosumer { NIC = Nic, Name = "Test", IsActive = true });
        }

        // Creates the slot service under test.
        private BookingSlotService Slots() => new(_slots, _nodes, _reservations, new FixedClock(Now));

        // Creates the reservation service under test (shares the same in-memory stores).
        private ReservationService Reservations() =>
            new(_reservations, _nodes, _slots, _prosumers, new FixedClock(Now), _user);

        // Publishes a slot tomorrow from the given hour for the given number of hours.
        private async Task<BookingSlotResponse> Publish(int startHour, int hours = 2, int capacity = 2)
        {
            var start = Tomorrow.Date.AddHours(startHour);
            var result = await Slots().CreateAsync(new BookingSlotRequest("node1", start, start.AddHours(hours), capacity));
            Assert.True(result.IsSuccess, result.Message);
            return result.Value!;
        }

        // Checks a valid window is published with full availability.
        [Fact]
        public async Task Create_ValidSlot_IsPublishedWithFullAvailability()
        {
            var slot = await Publish(4);
            Assert.Equal(2, slot.Capacity);
            Assert.Equal(0, slot.Booked);
            Assert.Equal(2, slot.Available);
            Assert.Equal("Colombo Hub", slot.NodeName);
        }

        // Checks windows that end before they start, or run too long, are refused.
        [Theory]
        [InlineData(4, -1)]
        [InlineData(4, 25)]
        public async Task Create_InvalidLength_IsRejected(int startHour, int hours)
        {
            var start = Tomorrow.Date.AddHours(startHour);
            var result = await Slots().CreateAsync(new BookingSlotRequest("node1", start, start.AddHours(hours), 1));
            Assert.Equal(ErrorType.Validation, result.Error);
        }

        // Checks capacity cannot exceed the station's battery slots.
        [Fact]
        public async Task Create_CapacityAboveBatterySlots_IsRejected()
        {
            var start = Tomorrow.Date.AddHours(4);
            var result = await Slots().CreateAsync(new BookingSlotRequest("node1", start, start.AddHours(1), 4));
            Assert.Equal(ErrorType.Validation, result.Error);
        }

        // Checks two slots at the same station cannot overlap.
        [Fact]
        public async Task Create_OverlappingSlot_IsRejected()
        {
            await Publish(4, hours: 2);
            var start = Tomorrow.Date.AddHours(5);
            var result = await Slots().CreateAsync(new BookingSlotRequest("node1", start, start.AddHours(2), 1));
            Assert.Equal(ErrorType.Validation, result.Error);
        }

        // Checks a slot must fit inside the station's opening hours.
        [Fact]
        public async Task Create_OutsideOpeningHours_IsRejected()
        {
            _nodes.Items[0].OpenTime = "08:00";
            _nodes.Items[0].CloseTime = "17:00";
            // 13:00 UTC = 18:30 local, after closing.
            var start = Tomorrow.Date.AddHours(13);
            var result = await Slots().CreateAsync(new BookingSlotRequest("node1", start, start.AddHours(1), 1));
            Assert.Equal(ErrorType.Validation, result.Error);
        }

        // Checks a booking inside a slot is bound to it and counted against its capacity.
        [Fact]
        public async Task Reservation_InsideSlot_IsBoundAndCounted()
        {
            var slot = await Publish(4);

            var booking = await Reservations().CreateAsync(new CreateReservationRequest("node1", slot.StartTime.AddMinutes(30)));

            Assert.True(booking.IsSuccess, booking.Message);
            Assert.Equal(slot.Id, booking.Value!.SlotId);
            var listed = Assert.Single(await Slots().GetAsync(new BookingSlotQuery("node1", null, null)));
            Assert.Equal(1, listed.Booked);
            Assert.Equal(1, listed.Available);
        }

        // Checks a station with published slots refuses times outside them.
        [Fact]
        public async Task Reservation_OutsidePublishedSlots_IsRejected()
        {
            var slot = await Publish(4);
            var result = await Reservations().CreateAsync(new CreateReservationRequest("node1", slot.EndTime.AddHours(1)));
            Assert.Equal(ErrorType.Validation, result.Error);
        }

        // Checks a full slot refuses further bookings.
        [Fact]
        public async Task Reservation_InFullSlot_IsConflict()
        {
            var slot = await Publish(4, capacity: 1);
            Assert.True((await Reservations().CreateAsync(new CreateReservationRequest("node1", slot.StartTime))).IsSuccess);

            var second = await Reservations().CreateAsync(new CreateReservationRequest("node1", slot.StartTime.AddMinutes(10)));
            Assert.Equal(ErrorType.Conflict, second.Error);
        }

        // Checks capacity cannot be reduced below the bookings already made.
        [Fact]
        public async Task Update_CapacityBelowBooked_IsRejected()
        {
            var slot = await Publish(4, capacity: 2);
            await Reservations().CreateAsync(new CreateReservationRequest("node1", slot.StartTime));
            await Reservations().CreateAsync(new CreateReservationRequest("node1", slot.StartTime.AddMinutes(5)));

            var result = await Slots().UpdateAsync(slot.Id, new UpdateBookingSlotRequest(slot.StartTime, slot.EndTime, 1));
            Assert.Equal(ErrorType.Validation, result.Error);
        }

        // Checks a slot holding live bookings cannot be deleted.
        [Fact]
        public async Task Delete_WithLiveBookings_IsConflict()
        {
            var slot = await Publish(4);
            await Reservations().CreateAsync(new CreateReservationRequest("node1", slot.StartTime));

            var result = await Slots().DeleteAsync(slot.Id);
            Assert.Equal(ErrorType.Conflict, result.Error);
        }

        // Checks an empty slot can be deleted.
        [Fact]
        public async Task Delete_EmptySlot_Succeeds()
        {
            var slot = await Publish(4);
            Assert.True((await Slots().DeleteAsync(slot.Id)).IsSuccess);
            Assert.Empty(_slots.Items);
        }
    }
}

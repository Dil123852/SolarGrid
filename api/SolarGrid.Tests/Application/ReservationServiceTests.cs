/*
 * File: ReservationServiceTests.cs
 * Purpose: Reservation workflow rules - booking window, battery-slot capacity, 12-hour notice,
 *          approval eligibility, ownership, and one-time QR verification.
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
    public class ReservationServiceTests
    {
        private static readonly DateTime Now = new(2026, 9, 29, 12, 0, 0, DateTimeKind.Utc);
        private const string Nic = "991234567V";

        private readonly InMemoryReservationRepository _reservations = new();
        private readonly InMemoryNodeRepository _nodes = new();
        private readonly InMemoryBookingSlotRepository _slots = new();
        private readonly InMemoryProsumerRepository _prosumers = new();
        private readonly FakeCurrentUser _user = new() { Role = UserRole.Prosumer, Nic = Nic };

        // Seeds one active node and one active prosumer for each test.
        public ReservationServiceTests()
        {
            _nodes.Items.Add(new MicrogridNode { Id = "node1", Name = "Kandy Hub", BatterySlots = 1, IsActive = true });
            _prosumers.Items.Add(new Prosumer { NIC = Nic, Name = "Test", IsActive = true });
        }

        // Creates the service under test with in-memory fakes and a fixed clock.
        private ReservationService Service() => new(_reservations, _nodes, _slots, _prosumers, new FixedClock(Now), _user);

        // Checks a booking inside 7 days is Pending and carries the node name.
        [Fact]
        public async Task Create_WithinWindow_IsPendingWithNodeName()
        {
            var result = await Service().CreateAsync(new CreateReservationRequest("node1", Now.AddDays(2)));

            Assert.True(result.IsSuccess);
            Assert.Equal(ReservationStatus.Pending, result.Value!.Status);
            Assert.Equal("Kandy Hub", result.Value.NodeName);
        }

        // Checks a booking 8 days ahead is refused.
        [Fact]
        public async Task Create_EightDaysOut_IsRejected()
        {
            var result = await Service().CreateAsync(new CreateReservationRequest("node1", Now.AddDays(8)));
            Assert.Equal(ErrorType.Validation, result.Error);
        }

        // Checks bookings at an inactive node are refused.
        [Fact]
        public async Task Create_AtInactiveNode_IsRejected()
        {
            _nodes.Items[0].IsActive = false;
            var result = await Service().CreateAsync(new CreateReservationRequest("node1", Now.AddDays(2)));
            Assert.Equal(ErrorType.Validation, result.Error);
        }

        // Checks deactivated prosumers cannot book.
        [Fact]
        public async Task Create_ByDeactivatedProsumer_IsRejected()
        {
            _prosumers.Items[0].IsActive = false;
            var result = await Service().CreateAsync(new CreateReservationRequest("node1", Now.AddDays(2)));
            Assert.Equal(ErrorType.Validation, result.Error);
        }

        // Checks a slot is refused once all battery slots are taken.
        [Fact]
        public async Task Create_WhenBatterySlotsFull_IsConflict()
        {
            var slot = Now.AddDays(1);
            await Service().CreateAsync(new CreateReservationRequest("node1", slot));

            var second = await Service().CreateAsync(new CreateReservationRequest("node1", slot));
            Assert.Equal(ErrorType.Conflict, second.Error);
        }

        // Checks rescheduling beyond 7 days is refused.
        [Fact]
        public async Task Update_NewSlotBeyondSevenDays_IsRejected()
        {
            _reservations.Items.Add(Reservation("r1", Now.AddDays(2), ReservationStatus.Pending));
            var result = await Service().UpdateAsync("r1", new UpdateReservationRequest(Now.AddDays(9)));
            Assert.Equal(ErrorType.Validation, result.Error);
        }

        // Checks rescheduling an approved booking returns it to Pending and drops the QR token.
        [Fact]
        public async Task Update_ApprovedReservation_ReturnsToPendingAndDropsQr()
        {
            var r = Reservation("r1", Now.AddDays(2), ReservationStatus.Approved);
            r.QrToken = "abc";
            _reservations.Items.Add(r);

            var result = await Service().UpdateAsync("r1", new UpdateReservationRequest(Now.AddDays(3)));

            Assert.True(result.IsSuccess);
            Assert.Equal(ReservationStatus.Pending, result.Value!.Status);
            Assert.Null(result.Value.QrToken);
        }

        // Checks cancelling within 12 hours of the slot is refused.
        [Fact]
        public async Task Cancel_InsideTwelveHours_IsRejected()
        {
            _reservations.Items.Add(Reservation("r1", Now.AddHours(6), ReservationStatus.Pending));
            var result = await Service().CancelAsync("r1");
            Assert.Equal(ErrorType.Validation, result.Error);
        }

        // Checks a cancelled booking cannot be approved.
        [Fact]
        public async Task Approve_CancelledReservation_IsRejected()
        {
            _reservations.Items.Add(Reservation("r1", Now.AddDays(2), ReservationStatus.Cancelled));
            _user.Role = UserRole.Backoffice;

            var result = await Service().ApproveAsync("r1");
            Assert.Equal(ErrorType.Validation, result.Error);
        }

        // Checks approving a pending booking issues a QR token.
        [Fact]
        public async Task Approve_Pending_IssuesQrToken()
        {
            _reservations.Items.Add(Reservation("r1", Now.AddDays(2), ReservationStatus.Pending));
            _user.Role = UserRole.Backoffice;

            var result = await Service().ApproveAsync("r1");

            Assert.True(result.IsSuccess);
            Assert.Equal(ReservationStatus.Approved, result.Value!.Status);
            Assert.False(string.IsNullOrEmpty(result.Value.QrToken));
        }

        // Checks a prosumer cannot cancel someone else's booking.
        [Fact]
        public async Task Prosumer_CannotCancelSomeoneElsesReservation()
        {
            var r = Reservation("r1", Now.AddDays(2), ReservationStatus.Pending);
            r.ProsumerNIC = "200012345678";
            _reservations.Items.Add(r);

            var result = await Service().CancelAsync("r1");
            Assert.Equal(ErrorType.Forbidden, result.Error);
        }

        // Checks a prosumer only ever lists their own bookings.
        [Fact]
        public async Task Prosumer_ListOnlyReturnsOwnReservations()
        {
            _reservations.Items.Add(Reservation("mine", Now.AddDays(2), ReservationStatus.Pending));
            var other = Reservation("theirs", Now.AddDays(2), ReservationStatus.Pending);
            other.ProsumerNIC = "200012345678";
            _reservations.Items.Add(other);

            // Asking for another NIC is ignored for prosumers.
            var list = await Service().GetAsync(new ReservationQuery("200012345678", null, null, null, null));

            Assert.Equal("mine", Assert.Single(list).Id);
        }

        // Checks a QR token completes the transfer only once.
        [Fact]
        public async Task VerifyQr_SecondScan_Fails()
        {
            var r = Reservation("r1", Now.AddDays(1), ReservationStatus.Approved);
            r.QrToken = "token";
            _reservations.Items.Add(r);
            _user.Role = UserRole.GridOperator;

            Assert.True((await Service().VerifyQrAsync(new VerifyQrRequest("token"))).IsSuccess);
            Assert.False((await Service().VerifyQrAsync(new VerifyQrRequest("token"))).IsSuccess);
        }

        // Builds a reservation for the test prosumer at node1.
        private static EnergyReservation Reservation(string id, DateTime slot, ReservationStatus status) =>
            new() { Id = id, ProsumerNIC = Nic, NodeId = "node1", SlotTime = slot, Status = status };

        // Checks a slot outside the node's opening hours is refused (Now = 17:30 Sri Lanka time).
        [Fact]
        public async Task Create_OutsideOperatingHours_IsRejected()
        {
            _nodes.Items[0].OpenTime = "06:00";
            _nodes.Items[0].CloseTime = "17:00";

            var result = await Service().CreateAsync(new CreateReservationRequest("node1", Now.AddDays(2)));

            Assert.Equal(ErrorType.Validation, result.Error);
            Assert.Contains("06:00-17:00", result.Message);
        }

        // Checks a slot inside the opening hours is accepted.
        [Fact]
        public async Task Create_InsideOperatingHours_IsAccepted()
        {
            _nodes.Items[0].OpenTime = "06:00";
            _nodes.Items[0].CloseTime = "18:00";

            var result = await Service().CreateAsync(new CreateReservationRequest("node1", Now.AddDays(2)));
            Assert.True(result.IsSuccess);
        }

        // Checks a Grid Operator can cancel a prosumer's booking on their behalf.
        [Fact]
        public async Task GridOperator_CanCancelOnBehalfOfProsumer()
        {
            _reservations.Items.Add(Reservation("r1", Now.AddDays(2), ReservationStatus.Pending));
            _user.Role = UserRole.GridOperator;
            _user.Nic = null;

            var result = await Service().CancelAsync("r1");

            Assert.True(result.IsSuccess);
            Assert.Equal(ReservationStatus.Cancelled, result.Value!.Status);
        }

        // Checks a Grid Operator still cannot reschedule a prosumer's booking.
        [Fact]
        public async Task GridOperator_CannotReschedule()
        {
            _reservations.Items.Add(Reservation("r1", Now.AddDays(2), ReservationStatus.Pending));
            _user.Role = UserRole.GridOperator;
            _user.Nic = null;

            var result = await Service().UpdateAsync("r1", new UpdateReservationRequest(Now.AddDays(3)));
            Assert.Equal(ErrorType.Forbidden, result.Error);
        }

        // Checks free-text search matches the node name, case-insensitively.
        [Fact]
        public async Task Search_MatchesNodeName()
        {
            _reservations.Items.Add(Reservation("r1", Now.AddDays(2), ReservationStatus.Pending));
            _nodes.Items.Add(new MicrogridNode { Id = "node2", Name = "Galle Hub", BatterySlots = 1, IsActive = true });
            var other = Reservation("r2", Now.AddDays(3), ReservationStatus.Pending);
            other.NodeId = "node2";
            _reservations.Items.Add(other);

            var list = await Service().GetAsync(new ReservationQuery(null, null, null, null, null, "kandy"));

            Assert.Equal("r1", Assert.Single(list).Id);
        }
    }
}

/*
 * File: ReservationServiceTests.cs
 * Purpose: Service-level tests using in-memory fakes of the repository ports - proves the
 *          business rules hold without a database (a direct benefit of the layered design).
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Application.Abstractions;
using SolarGrid.Application.Common;
using SolarGrid.Application.DTOs;
using SolarGrid.Application.Services;
using SolarGrid.Domain.Entities;
using SolarGrid.Domain.Enums;

namespace SolarGrid.Tests
{
    public class ReservationServiceTests
    {
        private static readonly DateTime Now = new(2026, 9, 29, 12, 0, 0, DateTimeKind.Utc);
        private const string Nic = "991234567V";

        private readonly FakeReservations _reservations = new();
        private readonly FakeNodes _nodes = new();
        private readonly FakeProsumers _prosumers = new();
        private readonly FakeUser _user = new() { Role = UserRole.Prosumer, Nic = Nic };

        public ReservationServiceTests()
        {
            _nodes.Items.Add(new MicrogridNode { Id = "node1", Name = "Kandy Hub", BatterySlots = 1, IsActive = true });
            _prosumers.Items.Add(new Prosumer { NIC = Nic, Name = "Test", IsActive = true });
        }

        private ReservationService Service() =>
            new(_reservations, _nodes, _prosumers, new FixedClock(Now), _user);

        [Fact]
        public async Task Create_WithinWindow_IsPendingWithNodeName()
        {
            var result = await Service().CreateAsync(new CreateReservationRequest("node1", Now.AddDays(2)));

            Assert.True(result.IsSuccess);
            Assert.Equal(ReservationStatus.Pending, result.Value!.Status);
            Assert.Equal("Kandy Hub", result.Value.NodeName);
        }

        [Fact]
        public async Task Create_EightDaysOut_IsRejected()
        {
            var result = await Service().CreateAsync(new CreateReservationRequest("node1", Now.AddDays(8)));
            Assert.Equal(ErrorType.Validation, result.Error);
        }

        [Fact]
        public async Task Create_WhenBatterySlotsFull_IsConflict()
        {
            var slot = Now.AddDays(1);
            await Service().CreateAsync(new CreateReservationRequest("node1", slot));

            var second = await Service().CreateAsync(new CreateReservationRequest("node1", slot));
            Assert.Equal(ErrorType.Conflict, second.Error);
        }

        [Fact]
        public async Task Cancel_InsideTwelveHours_IsRejected()
        {
            _reservations.Items.Add(Reservation("r1", Now.AddHours(6), ReservationStatus.Pending));
            var result = await Service().CancelAsync("r1");
            Assert.Equal(ErrorType.Validation, result.Error);
        }

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

        [Fact]
        public async Task Approve_CancelledReservation_IsRejected()
        {
            _reservations.Items.Add(Reservation("r1", Now.AddDays(2), ReservationStatus.Cancelled));
            _user.Role = UserRole.Backoffice;

            var result = await Service().ApproveAsync("r1");
            Assert.Equal(ErrorType.Validation, result.Error);
        }

        [Fact]
        public async Task Prosumer_CannotCancelSomeoneElsesReservation()
        {
            var r = Reservation("r1", Now.AddDays(2), ReservationStatus.Pending);
            r.ProsumerNIC = "200012345678";
            _reservations.Items.Add(r);

            var result = await Service().CancelAsync("r1");
            Assert.Equal(ErrorType.Forbidden, result.Error);
        }

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

        private static EnergyReservation Reservation(string id, DateTime slot, ReservationStatus status) =>
            new() { Id = id, ProsumerNIC = Nic, NodeId = "node1", SlotTime = slot, Status = status };

        // ---- In-memory fakes of the application ports ----

        private class FixedClock(DateTime now) : IClock
        {
            public DateTime UtcNow => now;
        }

        private class FakeUser : ICurrentUser
        {
            public bool IsAuthenticated => true;
            public UserRole? Role { get; set; }
            public string? Nic { get; set; }
        }

        private class FakeNodes : INodeRepository
        {
            public List<MicrogridNode> Items { get; } = new();
            public Task<List<MicrogridNode>> GetAllAsync(bool? isActive = null) => Task.FromResult(Items.ToList());
            public Task<MicrogridNode?> GetByIdAsync(string id) => Task.FromResult(Items.FirstOrDefault(n => n.Id == id));
            public Task CreateAsync(MicrogridNode node) { Items.Add(node); return Task.CompletedTask; }
            public Task<bool> ReplaceAsync(MicrogridNode node) => Task.FromResult(true);
            public Task<bool> SetActiveAsync(string id, bool isActive) => Task.FromResult(true);
        }

        private class FakeProsumers : IProsumerRepository
        {
            public List<Prosumer> Items { get; } = new();
            public Task<List<Prosumer>> GetAllAsync(bool? isActive = null) => Task.FromResult(Items.ToList());
            public Task<Prosumer?> GetByNicAsync(string nic) => Task.FromResult(Items.FirstOrDefault(p => p.NIC == nic));
            public Task<bool> ExistsAsync(string nic) => Task.FromResult(Items.Any(p => p.NIC == nic));
            public Task CreateAsync(Prosumer prosumer) { Items.Add(prosumer); return Task.CompletedTask; }
            public Task<bool> ReplaceAsync(Prosumer prosumer) => Task.FromResult(true);
            public Task<bool> SetActiveAsync(string nic, bool isActive) => Task.FromResult(true);
        }

        private class FakeReservations : IReservationRepository
        {
            public List<EnergyReservation> Items { get; } = new();

            public Task<List<EnergyReservation>> FindAsync(ReservationFilter f) => Task.FromResult(Items.Where(r =>
                (f.ProsumerNic == null || r.ProsumerNIC == f.ProsumerNic) &&
                (f.NodeId == null || r.NodeId == f.NodeId) &&
                (f.Status == null || r.Status == f.Status) &&
                (f.From == null || r.SlotTime >= f.From) &&
                (f.To == null || r.SlotTime <= f.To)).ToList());

            public Task<EnergyReservation?> GetByIdAsync(string id) => Task.FromResult(Items.FirstOrDefault(r => r.Id == id));
            public async Task<long> CountAsync(ReservationFilter filter) => (await FindAsync(filter)).Count;

            public Task CreateAsync(EnergyReservation reservation)
            {
                reservation.Id = Guid.NewGuid().ToString("N");
                Items.Add(reservation);
                return Task.CompletedTask;
            }

            public Task<bool> ReplaceAsync(EnergyReservation reservation) => Task.FromResult(true);

            public Task<EnergyReservation?> CompleteByQrTokenAsync(string qrToken, DateTime completedAt)
            {
                var r = Items.FirstOrDefault(x => x.QrToken == qrToken && x.Status == ReservationStatus.Approved);
                if (r != null) r.Status = ReservationStatus.Completed;
                return Task.FromResult(r);
            }
        }
    }
}

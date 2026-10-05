/*
 * File: Fakes.cs
 * Purpose: In-memory implementations of the application ports, so services can be tested
 *          without MongoDB, HTTP or real crypto - a direct benefit of the layered design.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Application.Abstractions;
using SolarGrid.Domain.Entities;
using SolarGrid.Domain.Enums;

namespace SolarGrid.Tests.Fakes
{
    public class FixedClock(DateTime now) : IClock
    {
        public DateTime UtcNow => now;
    }

    public class FakeCurrentUser : ICurrentUser
    {
        public bool IsAuthenticated => Role != null;
        public UserRole? Role { get; set; }
        public string? Nic { get; set; }
    }

    // "hash" = "hashed:" + password, so tests can assert on it.
    public class FakePasswordHasher : IPasswordHasher
    {
        // Fake hash: prefixes the password so tests can assert on it.
        public string Hash(string password) => "hashed:" + password;
        // Fake verify: matches the fake hash format.
        public bool Verify(string hash, string password) => hash == "hashed:" + password;
    }

    public class FakeTokenService : ITokenService
    {
        // Fake token: encodes role and subject so tests can read them.
        public (string Token, DateTime ExpiresAt) CreateToken(string subject, string displayName, UserRole role, string? nic) =>
            ($"token-{role}-{subject}", DateTime.UtcNow.AddHours(1));
    }

    public class InMemoryUserRepository : IUserRepository
    {
        public List<User> Items { get; } = new();

        // Returns all fake staff users.
        public Task<List<User>> GetAllAsync() => Task.FromResult(Items.ToList());
        // Finds a fake staff user by username.
        public Task<User?> GetByUsernameAsync(string username) => Task.FromResult(Items.FirstOrDefault(u => u.Username == username));
        // True when the username or email is already used.
        public Task<bool> ExistsAsync(string username, string email) =>
            Task.FromResult(Items.Any(u => u.Username == username || u.Email == email));
        // True when any fake staff user exists.
        public Task<bool> AnyAsync() => Task.FromResult(Items.Count > 0);

        // Adds a fake staff user with a generated id.
        public Task CreateAsync(User user)
        {
            user.Id = Guid.NewGuid().ToString("N");
            Items.Add(user);
            return Task.CompletedTask;
        }

        // Toggles a fake staff user's active flag.
        public Task<bool> SetActiveAsync(string id, bool isActive)
        {
            var user = Items.FirstOrDefault(u => u.Id == id);
            if (user != null) user.IsActive = isActive;
            return Task.FromResult(user != null);
        }
    }

    public class InMemoryProsumerRepository : IProsumerRepository
    {
        public List<Prosumer> Items { get; } = new();

        // Returns fake prosumers, optionally filtered by active flag.
        public Task<List<Prosumer>> GetAllAsync(bool? isActive = null) =>
            Task.FromResult(Items.Where(p => isActive == null || p.IsActive == isActive).ToList());
        // Finds a fake prosumer by NIC.
        public Task<Prosumer?> GetByNicAsync(string nic) => Task.FromResult(Items.FirstOrDefault(p => p.NIC == nic));
        // True when a fake prosumer has this NIC.
        public Task<bool> ExistsAsync(string nic) => Task.FromResult(Items.Any(p => p.NIC == nic));

        // Adds a fake prosumer.
        public Task CreateAsync(Prosumer prosumer)
        {
            Items.Add(prosumer);
            return Task.CompletedTask;
        }

        // Replaces a fake prosumer by NIC.
        public Task<bool> ReplaceAsync(Prosumer prosumer)
        {
            var index = Items.FindIndex(p => p.NIC == prosumer.NIC);
            if (index >= 0) Items[index] = prosumer;
            return Task.FromResult(index >= 0);
        }

        // Toggles a fake prosumer's active flag.
        public Task<bool> SetActiveAsync(string nic, bool isActive)
        {
            var prosumer = Items.FirstOrDefault(p => p.NIC == nic);
            if (prosumer != null) prosumer.IsActive = isActive;
            return Task.FromResult(prosumer != null);
        }
    }

    public class InMemoryNodeRepository : INodeRepository
    {
        public List<MicrogridNode> Items { get; } = new();

        // Returns fake nodes, optionally filtered by active flag.
        public Task<List<MicrogridNode>> GetAllAsync(bool? isActive = null) =>
            Task.FromResult(Items.Where(n => isActive == null || n.IsActive == isActive).ToList());
        // Finds a fake node by id.
        public Task<MicrogridNode?> GetByIdAsync(string id) => Task.FromResult(Items.FirstOrDefault(n => n.Id == id));

        // Adds a fake node with a generated id.
        public Task CreateAsync(MicrogridNode node)
        {
            node.Id = Guid.NewGuid().ToString("N");
            Items.Add(node);
            return Task.CompletedTask;
        }

        // Replaces a fake node by id.
        public Task<bool> ReplaceAsync(MicrogridNode node)
        {
            var index = Items.FindIndex(n => n.Id == node.Id);
            if (index >= 0) Items[index] = node;
            return Task.FromResult(index >= 0);
        }

        // Toggles a fake node's active flag.
        public Task<bool> SetActiveAsync(string id, bool isActive)
        {
            var node = Items.FirstOrDefault(n => n.Id == id);
            if (node != null) node.IsActive = isActive;
            return Task.FromResult(node != null);
        }
    }

    public class InMemoryReservationRepository : IReservationRepository
    {
        public List<EnergyReservation> Items { get; } = new();

        // Filters fake reservations the same way the MongoDB repository does.
        public Task<List<EnergyReservation>> FindAsync(ReservationFilter f) => Task.FromResult(Items.Where(r =>
            (f.ProsumerNic == null || r.ProsumerNIC == f.ProsumerNic) &&
            (f.SlotId == null || r.SlotId == f.SlotId) &&
            (f.NodeId == null || r.NodeId == f.NodeId) &&
            (f.Status == null || r.Status == f.Status) &&
            (f.From == null || r.SlotTime >= f.From) &&
            (f.To == null || r.SlotTime <= f.To)).ToList());

        // Finds a fake reservation by id.
        public Task<EnergyReservation?> GetByIdAsync(string id) => Task.FromResult(Items.FirstOrDefault(r => r.Id == id));
        // Counts fake reservations matching the filter.
        public async Task<long> CountAsync(ReservationFilter filter) => (await FindAsync(filter)).Count;

        // Adds a fake reservation with a generated id.
        public Task CreateAsync(EnergyReservation reservation)
        {
            reservation.Id = Guid.NewGuid().ToString("N");
            Items.Add(reservation);
            return Task.CompletedTask;
        }

        // Replaces a fake reservation by id.
        public Task<bool> ReplaceAsync(EnergyReservation reservation)
        {
            var index = Items.FindIndex(r => r.Id == reservation.Id);
            if (index >= 0) Items[index] = reservation;
            return Task.FromResult(index >= 0);
        }

        // Completes an approved fake reservation with this QR token, once.
        public Task<EnergyReservation?> CompleteByQrTokenAsync(string qrToken, DateTime completedAt)
        {
            var r = Items.FirstOrDefault(x => x.QrToken == qrToken && x.Status == ReservationStatus.Approved);
            if (r != null)
            {
                r.Status = ReservationStatus.Completed;
                r.UpdatedAt = completedAt;
            }
            return Task.FromResult(r);
        }
    }

    public class InMemoryBookingSlotRepository : IBookingSlotRepository
    {
        public List<EnergyBookingSlot> Items { get; } = new();

        // Returns fake slots overlapping the range, earliest first.
        public Task<List<EnergyBookingSlot>> FindAsync(string? nodeId = null, DateTime? from = null, DateTime? to = null) =>
            Task.FromResult(Items.Where(s =>
                (nodeId == null || s.NodeId == nodeId) &&
                (from == null || s.EndTime > from) &&
                (to == null || s.StartTime < to)).OrderBy(s => s.StartTime).ToList());

        // Finds a fake slot by id.
        public Task<EnergyBookingSlot?> GetByIdAsync(string id) => Task.FromResult(Items.FirstOrDefault(s => s.Id == id));

        // Adds a fake slot with a generated id.
        public Task CreateAsync(EnergyBookingSlot slot)
        {
            slot.Id = Guid.NewGuid().ToString("N");
            Items.Add(slot);
            return Task.CompletedTask;
        }

        // Replaces a fake slot by id.
        public Task<bool> ReplaceAsync(EnergyBookingSlot slot)
        {
            var index = Items.FindIndex(s => s.Id == slot.Id);
            if (index >= 0) Items[index] = slot;
            return Task.FromResult(index >= 0);
        }

        // Removes a fake slot by id.
        public Task<bool> DeleteAsync(string id) => Task.FromResult(Items.RemoveAll(s => s.Id == id) > 0);
    }
}

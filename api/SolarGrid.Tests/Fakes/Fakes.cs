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
        public string Hash(string password) => "hashed:" + password;
        public bool Verify(string hash, string password) => hash == "hashed:" + password;
    }

    public class FakeTokenService : ITokenService
    {
        public (string Token, DateTime ExpiresAt) CreateToken(string subject, string displayName, UserRole role, string? nic) =>
            ($"token-{role}-{subject}", DateTime.UtcNow.AddHours(1));
    }

    public class InMemoryUserRepository : IUserRepository
    {
        public List<User> Items { get; } = new();

        public Task<List<User>> GetAllAsync() => Task.FromResult(Items.ToList());
        public Task<User?> GetByUsernameAsync(string username) => Task.FromResult(Items.FirstOrDefault(u => u.Username == username));
        public Task<bool> ExistsAsync(string username, string email) =>
            Task.FromResult(Items.Any(u => u.Username == username || u.Email == email));
        public Task<bool> AnyAsync() => Task.FromResult(Items.Count > 0);

        public Task CreateAsync(User user)
        {
            user.Id = Guid.NewGuid().ToString("N");
            Items.Add(user);
            return Task.CompletedTask;
        }

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

        public Task<List<Prosumer>> GetAllAsync(bool? isActive = null) =>
            Task.FromResult(Items.Where(p => isActive == null || p.IsActive == isActive).ToList());
        public Task<Prosumer?> GetByNicAsync(string nic) => Task.FromResult(Items.FirstOrDefault(p => p.NIC == nic));
        public Task<bool> ExistsAsync(string nic) => Task.FromResult(Items.Any(p => p.NIC == nic));

        public Task CreateAsync(Prosumer prosumer)
        {
            Items.Add(prosumer);
            return Task.CompletedTask;
        }

        public Task<bool> ReplaceAsync(Prosumer prosumer)
        {
            var index = Items.FindIndex(p => p.NIC == prosumer.NIC);
            if (index >= 0) Items[index] = prosumer;
            return Task.FromResult(index >= 0);
        }

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

        public Task<List<MicrogridNode>> GetAllAsync(bool? isActive = null) =>
            Task.FromResult(Items.Where(n => isActive == null || n.IsActive == isActive).ToList());
        public Task<MicrogridNode?> GetByIdAsync(string id) => Task.FromResult(Items.FirstOrDefault(n => n.Id == id));

        public Task CreateAsync(MicrogridNode node)
        {
            node.Id = Guid.NewGuid().ToString("N");
            Items.Add(node);
            return Task.CompletedTask;
        }

        public Task<bool> ReplaceAsync(MicrogridNode node)
        {
            var index = Items.FindIndex(n => n.Id == node.Id);
            if (index >= 0) Items[index] = node;
            return Task.FromResult(index >= 0);
        }

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

        public Task<bool> ReplaceAsync(EnergyReservation reservation)
        {
            var index = Items.FindIndex(r => r.Id == reservation.Id);
            if (index >= 0) Items[index] = reservation;
            return Task.FromResult(index >= 0);
        }

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
}

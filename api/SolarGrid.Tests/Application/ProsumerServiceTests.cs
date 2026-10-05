/*
 * File: ProsumerServiceTests.cs
 * Purpose: Prosumer registration (NIC key, uniqueness), profile updates, ownership and
 *          (de)activation rules.
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
    public class ProsumerServiceTests
    {
        private const string Nic = "991234567V";

        private readonly InMemoryProsumerRepository _prosumers = new();
        private readonly FakeCurrentUser _user = new();

        // Creates the service under test with in-memory fakes.
        private ProsumerService Service() => new(_prosumers, new FakePasswordHasher(), _user);

        // Builds a valid registration request for the given NIC.
        private static RegisterProsumerRequest Registration(string nic = Nic) =>
            new(nic, "Nimal Perera", "nimal@example.com", "0771234567", "Kandy", "secret1");

        // Checks the NIC is upper-cased and the password is stored hashed.
        [Fact]
        public async Task Register_NormalisesNicAndHashesPassword()
        {
            var result = await Service().RegisterAsync(Registration("991234567v"));

            Assert.True(result.IsSuccess);
            var stored = Assert.Single(_prosumers.Items);
            Assert.Equal("991234567V", stored.NIC);
            Assert.Equal("hashed:secret1", stored.PasswordHash);
        }

        // Checks malformed NICs are refused.
        [Theory]
        [InlineData("12345")]
        [InlineData("99123456VV")]
        public async Task Register_InvalidNic_IsRejected(string nic)
        {
            var result = await Service().RegisterAsync(Registration(nic));
            Assert.Equal(ErrorType.Validation, result.Error);
        }

        // Checks a second account with the same NIC is a Conflict.
        [Fact]
        public async Task Register_DuplicateNic_IsConflict()
        {
            await Service().RegisterAsync(Registration());
            var second = await Service().RegisterAsync(Registration());
            Assert.Equal(ErrorType.Conflict, second.Error);
        }

        // Checks profile updates keep the password hash and active flag.
        [Fact]
        public async Task Update_KeepsPasswordAndActiveFlag()
        {
            _prosumers.Items.Add(new Prosumer { NIC = Nic, Name = "Old", Email = "old@example.com", Phone = "1", PasswordHash = "hashed:pw", IsActive = true });
            _user.Role = UserRole.Prosumer;
            _user.Nic = Nic;

            var result = await Service().UpdateAsync(Nic, new UpdateProsumerRequest("New Name", "new@example.com", "0770000000", "Galle"));

            Assert.True(result.IsSuccess);
            var stored = _prosumers.Items[0];
            Assert.Equal("New Name", stored.Name);
            Assert.Equal("hashed:pw", stored.PasswordHash);
            Assert.True(stored.IsActive);
        }

        // Checks a prosumer cannot read another prosumer's profile.
        [Fact]
        public async Task Prosumer_CannotReadAnotherProsumer()
        {
            _prosumers.Items.Add(new Prosumer { NIC = "200012345678", Name = "Other" });
            _user.Role = UserRole.Prosumer;
            _user.Nic = Nic;

            var result = await Service().GetAsync("200012345678");
            Assert.Equal(ErrorType.Forbidden, result.Error);
        }

        // Checks deactivate then reactivate toggles the active flag.
        [Fact]
        public async Task Deactivate_ThenReactivate_TogglesActiveFlag()
        {
            _prosumers.Items.Add(new Prosumer { NIC = Nic, IsActive = true });
            _user.Role = UserRole.Prosumer;
            _user.Nic = Nic;

            Assert.True((await Service().DeactivateAsync(Nic)).IsSuccess);
            Assert.False(_prosumers.Items[0].IsActive);

            Assert.True((await Service().ReactivateAsync(Nic)).IsSuccess);
            Assert.True(_prosumers.Items[0].IsActive);
        }
    }
}

/*
 * File: AuthServiceTests.cs
 * Purpose: Staff and prosumer login outcomes (success, bad credentials, pending activation),
 *          staff registration rules and first-run seeding.
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
    public class AuthServiceTests
    {
        private readonly InMemoryUserRepository _users = new();
        private readonly InMemoryProsumerRepository _prosumers = new();

        private AuthService Service() => new(_users, _prosumers, new FakePasswordHasher(), new FakeTokenService());

        [Fact]
        public async Task StaffLogin_CaseInsensitiveUsername_ReturnsRole()
        {
            _users.Items.Add(new User { Id = "u1", Username = "operator1", PasswordHash = "hashed:pw", Role = UserRole.GridOperator });

            var result = await Service().LoginStaffAsync(new LoginRequest("Operator1", "pw"));

            Assert.True(result.IsSuccess);
            Assert.Equal(UserRole.GridOperator, result.Value!.Role);
        }

        [Fact]
        public async Task StaffLogin_WrongPassword_IsUnauthorized()
        {
            _users.Items.Add(new User { Id = "u1", Username = "admin", PasswordHash = "hashed:pw", Role = UserRole.Backoffice });
            var result = await Service().LoginStaffAsync(new LoginRequest("admin", "nope"));
            Assert.Equal(ErrorType.Unauthorized, result.Error);
        }

        [Fact]
        public async Task ProsumerLogin_DeactivatedAccount_IsInactiveWithPendingMessage()
        {
            _prosumers.Items.Add(new Prosumer { NIC = "991234567V", Name = "Nimal", PasswordHash = "hashed:pw", IsActive = false });

            var result = await Service().LoginProsumerAsync(new ProsumerLoginRequest("991234567v", "pw"));

            Assert.Equal(ErrorType.AccountInactive, result.Error);
            Assert.Equal(AuthService.PendingActivationMessage, result.Message);
        }

        [Fact]
        public async Task ProsumerLogin_Active_ReturnsNic()
        {
            _prosumers.Items.Add(new Prosumer { NIC = "991234567V", Name = "Nimal", PasswordHash = "hashed:pw", IsActive = true });

            var result = await Service().LoginProsumerAsync(new ProsumerLoginRequest("991234567V", "pw"));

            Assert.True(result.IsSuccess);
            Assert.Equal("991234567V", result.Value!.Nic);
            Assert.Equal(UserRole.Prosumer, result.Value.Role);
        }

        [Fact]
        public async Task RegisterStaff_WithProsumerRole_IsRejected()
        {
            var result = await Service().RegisterStaffAsync(new RegisterStaffRequest("x", "x@example.com", "secret1", UserRole.Prosumer));
            Assert.Equal(ErrorType.Validation, result.Error);
        }

        [Fact]
        public async Task RegisterStaff_DuplicateUsername_IsConflict()
        {
            await Service().RegisterStaffAsync(new RegisterStaffRequest("op", "op@example.com", "secret1", UserRole.GridOperator));
            var second = await Service().RegisterStaffAsync(new RegisterStaffRequest("OP", "other@example.com", "secret1", UserRole.GridOperator));
            Assert.Equal(ErrorType.Conflict, second.Error);
        }

        [Fact]
        public async Task Seed_OnlyRunsWhenNoUsersExist()
        {
            Assert.True(await Service().SeedBackofficeAsync("admin", "admin@solargrid.lk", "secret1"));
            Assert.False(await Service().SeedBackofficeAsync("admin2", "admin2@solargrid.lk", "secret1"));
            Assert.Single(_users.Items);
        }

        // ---- Mobile sign-in: one form, the service decides the account type ----

        [Fact]
        public async Task MobileLogin_NicIdentifier_SignsInProsumer()
        {
            _prosumers.Items.Add(new Prosumer { NIC = "991234567V", Name = "Nimal", PasswordHash = "hashed:pw", IsActive = true });

            var result = await Service().LoginMobileAsync(new MobileLoginRequest(" 991234567v ", "pw"));

            Assert.True(result.IsSuccess);
            Assert.Equal(UserRole.Prosumer, result.Value!.Role);
            Assert.Equal("991234567V", result.Value.Nic);
        }

        [Fact]
        public async Task MobileLogin_Username_SignsInGridOperator()
        {
            _users.Items.Add(new User { Id = "u1", Username = "operator1", PasswordHash = "hashed:pw", Role = UserRole.GridOperator });

            var result = await Service().LoginMobileAsync(new MobileLoginRequest("operator1", "pw"));

            Assert.True(result.IsSuccess);
            Assert.Equal(UserRole.GridOperator, result.Value!.Role);
        }

        [Fact]
        public async Task MobileLogin_Backoffice_IsSentToWebPortal()
        {
            _users.Items.Add(new User { Id = "u1", Username = "admin", PasswordHash = "hashed:pw", Role = UserRole.Backoffice });

            var result = await Service().LoginMobileAsync(new MobileLoginRequest("admin", "pw"));

            Assert.Equal(ErrorType.Forbidden, result.Error);
            Assert.Equal(AuthService.BackofficeUsesWebMessage, result.Message);
        }

        [Fact]
        public async Task MobileLogin_DeactivatedProsumer_IsAccountInactive()
        {
            _prosumers.Items.Add(new Prosumer { NIC = "200012345678", Name = "Kamal", PasswordHash = "hashed:pw", IsActive = false });

            var result = await Service().LoginMobileAsync(new MobileLoginRequest("200012345678", "pw"));

            Assert.Equal(ErrorType.AccountInactive, result.Error);
        }

        [Fact]
        public async Task MobileLogin_WrongPassword_IsUnauthorized()
        {
            _users.Items.Add(new User { Id = "u1", Username = "operator1", PasswordHash = "hashed:pw", Role = UserRole.GridOperator });
            var result = await Service().LoginMobileAsync(new MobileLoginRequest("operator1", "nope"));
            Assert.Equal(ErrorType.Unauthorized, result.Error);
        }

        [Fact]
        public async Task RegisterStaff_NicShapedUsername_IsRejected()
        {
            var result = await Service().RegisterStaffAsync(new RegisterStaffRequest("200012345678", "x@example.com", "secret1", UserRole.GridOperator));
            Assert.Equal(ErrorType.Validation, result.Error);
        }
    }
}

/*
 * File: AuthService.cs
 * Purpose: Login for staff (username) and prosumers (NIC), staff registration, and first-run
 *          seeding of a Backoffice account. Deactivated prosumers are refused with a
 *          "pending activation" message so the mobile app can show that state.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Application.Abstractions;
using SolarGrid.Application.Common;
using SolarGrid.Application.DTOs;
using SolarGrid.Domain.Entities;
using SolarGrid.Domain.Enums;

namespace SolarGrid.Application.Services
{
    public class AuthService
    {
        public const string PendingActivationMessage =
            "Your account is deactivated and pending activation by a Backoffice officer.";

        private readonly IUserRepository _users;
        private readonly IProsumerRepository _prosumers;
        private readonly IPasswordHasher _hasher;
        private readonly ITokenService _tokens;

        public AuthService(IUserRepository users, IProsumerRepository prosumers, IPasswordHasher hasher, ITokenService tokens)
        {
            _users = users;
            _prosumers = prosumers;
            _hasher = hasher;
            _tokens = tokens;
        }

        // Staff login (Backoffice / GridOperator) by username + password.
        public async Task<Result<AuthResponse>> LoginStaffAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return Result.Invalid<AuthResponse>("Username and password are required.");

            var user = await _users.GetByUsernameAsync(request.Username.Trim().ToLowerInvariant());
            if (user == null || !_hasher.Verify(user.PasswordHash, request.Password))
                return Result.Fail<AuthResponse>(ErrorType.Unauthorized, "Invalid username or password.");

            if (!user.IsActive)
                return Result.Fail<AuthResponse>(ErrorType.Forbidden, "This staff account has been disabled.");

            var (token, expires) = _tokens.CreateToken(user.Id, user.Username, user.Role, null);
            return Result.Ok(new AuthResponse(token, expires, user.Role, user.Username, null));
        }

        // Prosumer login by NIC + password.
        public async Task<Result<AuthResponse>> LoginProsumerAsync(ProsumerLoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nic) || string.IsNullOrWhiteSpace(request.Password))
                return Result.Invalid<AuthResponse>("NIC and password are required.");

            var prosumer = await _prosumers.GetByNicAsync(Validation.NormalizeNic(request.Nic));
            if (prosumer == null || string.IsNullOrEmpty(prosumer.PasswordHash) ||
                !_hasher.Verify(prosumer.PasswordHash, request.Password))
                return Result.Fail<AuthResponse>(ErrorType.Unauthorized, "Invalid NIC or password.");

            if (!prosumer.IsActive)
                return Result.Fail<AuthResponse>(ErrorType.Forbidden, PendingActivationMessage);

            var (token, expires) = _tokens.CreateToken(prosumer.NIC, prosumer.Name, UserRole.Prosumer, prosumer.NIC);
            return Result.Ok(new AuthResponse(token, expires, UserRole.Prosumer, prosumer.Name, prosumer.NIC));
        }

        // Creates a Backoffice or GridOperator account (caller must be Backoffice - enforced at the API).
        public async Task<Result<UserResponse>> RegisterStaffAsync(RegisterStaffRequest request)
        {
            if (request.Role == UserRole.Prosumer)
                return Result.Invalid<UserResponse>("Prosumers register through the mobile app.");
            if (string.IsNullOrWhiteSpace(request.Username))
                return Result.Invalid<UserResponse>("Username is required.");
            if (!Validation.IsValidEmail(request.Email?.Trim()))
                return Result.Invalid<UserResponse>("A valid email is required.");
            if (string.IsNullOrEmpty(request.Password) || request.Password.Length < Validation.MinPasswordLength)
                return Result.Invalid<UserResponse>($"Password must be at least {Validation.MinPasswordLength} characters.");

            var username = request.Username.Trim().ToLowerInvariant();
            var email = request.Email!.Trim().ToLowerInvariant();
            if (await _users.ExistsAsync(username, email))
                return Result.Fail<UserResponse>(ErrorType.Conflict, "A user with that username or email already exists.");

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = _hasher.Hash(request.Password),
                Role = request.Role
            };
            await _users.CreateAsync(user);
            return Result.Ok(user.ToResponse(), "User created.");
        }

        public async Task<List<UserResponse>> GetStaffAsync() =>
            (await _users.GetAllAsync()).Select(u => u.ToResponse()).ToList();

        // Enables or disables a staff account.
        public async Task<Result> SetStaffActiveAsync(string id, bool isActive) =>
            await _users.SetActiveAsync(id, isActive)
                ? Result.Ok(isActive ? "User enabled." : "User disabled.")
                : Result.Fail(ErrorType.NotFound, "User not found.");

        // Creates the first Backoffice account when the Users collection is empty.
        public async Task<bool> SeedBackofficeAsync(string username, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(password) || await _users.AnyAsync()) return false;
            var result = await RegisterStaffAsync(new RegisterStaffRequest(username, email, password, UserRole.Backoffice));
            return result.IsSuccess;
        }
    }
}

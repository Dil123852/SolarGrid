/*
 * File: ProsumerService.cs
 * Purpose: Business logic for prosumer self-registration, profile updates, and (de)activation.
 *          All rules live here, not in the web/mobile clients, per the FAT-service architecture.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Application.Abstractions;
using SolarGrid.Application.Common;
using SolarGrid.Application.DTOs;
using SolarGrid.Domain.Entities;

namespace SolarGrid.Application.Services
{
    public class ProsumerService
    {
        private readonly IProsumerRepository _prosumers;
        private readonly IPasswordHasher _hasher;
        private readonly ICurrentUser _currentUser;

        // Receives the prosumer repository, password hasher and current caller.
        public ProsumerService(IProsumerRepository prosumers, IPasswordHasher hasher, ICurrentUser currentUser)
        {
            _prosumers = prosumers;
            _hasher = hasher;
            _currentUser = currentUser;
        }

        // Lists prosumers; isActive=false gives the Backoffice "pending activation" list.
        public async Task<List<ProsumerResponse>> GetAllAsync(bool? isActive) =>
            (await _prosumers.GetAllAsync(isActive)).Select(p => p.ToResponse()).ToList();

        // Returns one prosumer profile; prosumers may only read their own.
        public async Task<Result<ProsumerResponse>> GetAsync(string nic)
        {
            nic = Validation.NormalizeNic(nic);
            if (!_currentUser.CanAccessProsumer(nic)) return Result.Forbidden<ProsumerResponse>();

            var prosumer = await _prosumers.GetByNicAsync(nic);
            return prosumer == null
                ? Result.NotFound<ProsumerResponse>("Prosumer not found.")
                : Result.Ok(prosumer.ToResponse());
        }

        // Self-registration from the mobile app; NIC is the unique key.
        public async Task<Result<ProsumerResponse>> RegisterAsync(RegisterProsumerRequest request)
        {
            if (!Validation.IsValidNic(request.Nic?.Trim()))
                return Result.Invalid<ProsumerResponse>("NIC must be 9 digits followed by V/X, or 12 digits.");
            var profileError = ValidateProfile(request.Name, request.Email, request.Phone);
            if (profileError != null) return Result.Invalid<ProsumerResponse>(profileError);
            if (string.IsNullOrEmpty(request.Password) || request.Password.Length < Validation.MinPasswordLength)
                return Result.Invalid<ProsumerResponse>($"Password must be at least {Validation.MinPasswordLength} characters.");

            var nic = Validation.NormalizeNic(request.Nic!);
            if (await _prosumers.ExistsAsync(nic))
                return Result.Fail<ProsumerResponse>(ErrorType.Conflict, "An account with this NIC already exists.");

            var prosumer = new Prosumer
            {
                NIC = nic,
                Name = request.Name.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                Phone = request.Phone.Trim(),
                Address = request.Address?.Trim() ?? string.Empty,
                PasswordHash = _hasher.Hash(request.Password)
            };
            await _prosumers.CreateAsync(prosumer);
            return Result.Ok(prosumer.ToResponse(), "Account created.");
        }

        // Updates editable profile fields only - NIC, password and active flag are preserved.
        public async Task<Result<ProsumerResponse>> UpdateAsync(string nic, UpdateProsumerRequest request)
        {
            nic = Validation.NormalizeNic(nic);
            if (!_currentUser.CanAccessProsumer(nic)) return Result.Forbidden<ProsumerResponse>();

            var profileError = ValidateProfile(request.Name, request.Email, request.Phone);
            if (profileError != null) return Result.Invalid<ProsumerResponse>(profileError);

            var prosumer = await _prosumers.GetByNicAsync(nic);
            if (prosumer == null) return Result.NotFound<ProsumerResponse>("Prosumer not found.");

            prosumer.Name = request.Name.Trim();
            prosumer.Email = request.Email.Trim().ToLowerInvariant();
            prosumer.Phone = request.Phone.Trim();
            prosumer.Address = request.Address?.Trim() ?? string.Empty;
            await _prosumers.ReplaceAsync(prosumer);
            return Result.Ok(prosumer.ToResponse(), "Profile updated.");
        }

        // A prosumer deactivates their own account (Backoffice may also do it).
        public async Task<Result> DeactivateAsync(string nic)
        {
            nic = Validation.NormalizeNic(nic);
            if (!_currentUser.CanAccessProsumer(nic))
                return Result.Fail(ErrorType.Forbidden, "You can only deactivate your own account.");

            return await _prosumers.SetActiveAsync(nic, false)
                ? Result.Ok("Account deactivated. Contact the Backoffice to reactivate it.")
                : Result.Fail(ErrorType.NotFound, "Prosumer not found.");
        }

        // Reactivation is Backoffice-only (enforced by the controller's role policy).
        public async Task<Result> ReactivateAsync(string nic) =>
            await _prosumers.SetActiveAsync(Validation.NormalizeNic(nic), true)
                ? Result.Ok("Account reactivated.")
                : Result.Fail(ErrorType.NotFound, "Prosumer not found.");

        // Checks the editable profile fields (name, email, phone).
        private static string? ValidateProfile(string? name, string? email, string? phone)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Name is required.";
            if (!Validation.IsValidEmail(email?.Trim())) return "A valid email is required.";
            if (string.IsNullOrWhiteSpace(phone)) return "Phone number is required.";
            return null;
        }
    }
}

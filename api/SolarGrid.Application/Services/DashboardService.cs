/*
 * File: DashboardService.cs
 * Purpose: Live reservation counts for the dashboards - pending, approved-and-upcoming, completed.
 *          Prosumers get their own counts; staff get system-wide counts (or one prosumer's via nic).
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Application.Abstractions;
using SolarGrid.Application.Common;
using SolarGrid.Application.DTOs;
using SolarGrid.Domain.Enums;

namespace SolarGrid.Application.Services
{
    public class DashboardService
    {
        private readonly IReservationRepository _reservations;
        private readonly IClock _clock;
        private readonly ICurrentUser _currentUser;

        // Receives the reservation repository, clock and current caller.
        public DashboardService(IReservationRepository reservations, IClock clock, ICurrentUser currentUser)
        {
            _reservations = reservations;
            _clock = clock;
            _currentUser = currentUser;
        }

        // Counts pending, approved-upcoming and completed reservations (prosumers only see their own).
        public async Task<DashboardResponse> GetAsync(string? nic)
        {
            nic = _currentUser.Role == UserRole.Prosumer
                ? _currentUser.Nic
                : string.IsNullOrWhiteSpace(nic) ? null : Validation.NormalizeNic(nic);

            var pending = await _reservations.CountAsync(new ReservationFilter(nic, Status: ReservationStatus.Pending));
            var approvedFuture = await _reservations.CountAsync(
                new ReservationFilter(nic, Status: ReservationStatus.Approved, From: _clock.UtcNow));
            var completed = await _reservations.CountAsync(new ReservationFilter(nic, Status: ReservationStatus.Completed));

            return new DashboardResponse(pending, approvedFuture, completed);
        }
    }
}

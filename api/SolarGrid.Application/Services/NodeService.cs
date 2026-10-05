/*
 * File: NodeService.cs
 * Purpose: Business logic for managing microgrid nodes - details, operating schedule and battery
 *          slots - including the rule that a node cannot be deactivated while it has live
 *          (pending/approved, upcoming) reservations.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Application.Abstractions;
using SolarGrid.Application.Common;
using SolarGrid.Application.DTOs;
using SolarGrid.Domain.Entities;
using SolarGrid.Domain.Rules;

namespace SolarGrid.Application.Services
{
    public class NodeService
    {
        private readonly INodeRepository _nodes;
        private readonly IReservationRepository _reservations;
        private readonly IClock _clock;

        // Receives the node and reservation repositories and the clock.
        public NodeService(INodeRepository nodes, IReservationRepository reservations, IClock clock)
        {
            _nodes = nodes;
            _reservations = reservations;
            _clock = clock;
        }

        // Lists nodes, optionally only active or only inactive ones.
        public async Task<List<NodeResponse>> GetAllAsync(bool? isActive) =>
            (await _nodes.GetAllAsync(isActive)).Select(n => n.ToResponse()).ToList();

        // Returns one node by id, or NotFound.
        public async Task<Result<NodeResponse>> GetAsync(string id)
        {
            var node = await _nodes.GetByIdAsync(id);
            return node == null ? Result.NotFound<NodeResponse>("Node not found.") : Result.Ok(node.ToResponse());
        }

        // Validates and creates a new microgrid node.
        public async Task<Result<NodeResponse>> CreateAsync(NodeRequest request)
        {
            var error = Validate(request);
            if (error != null) return Result.Invalid<NodeResponse>(error);

            var node = new MicrogridNode();
            Apply(node, request);
            await _nodes.CreateAsync(node);
            return Result.Ok(node.ToResponse(), "Node created.");
        }

        // Validates and updates a node's details and operating schedule.
        public async Task<Result<NodeResponse>> UpdateAsync(string id, NodeRequest request)
        {
            var error = Validate(request);
            if (error != null) return Result.Invalid<NodeResponse>(error);

            var node = await _nodes.GetByIdAsync(id);
            if (node == null) return Result.NotFound<NodeResponse>("Node not found.");

            Apply(node, request);
            await _nodes.ReplaceAsync(node);
            return Result.Ok(node.ToResponse(), "Node updated.");
        }

        // Deactivates a node unless it still has live reservations against it.
        public async Task<Result> DeactivateAsync(string id)
        {
            if (await _nodes.GetByIdAsync(id) == null) return Result.Fail(ErrorType.NotFound, "Node not found.");

            var now = _clock.UtcNow;
            var upcoming = await _reservations.FindAsync(new ReservationFilter(NodeId: id, From: now));
            if (upcoming.Any(r => ReservationPolicy.BlocksNodeDeactivation(r, now)))
                return Result.Fail(ErrorType.Conflict, "Cannot deactivate: node has active energy reservations.");

            await _nodes.SetActiveAsync(id, false);
            return Result.Ok("Node deactivated.");
        }

        // Grid Operators keep battery-slot availability current without touching other node details.
        public async Task<Result<NodeResponse>> UpdateBatterySlotsAsync(string id, UpdateBatterySlotsRequest request)
        {
            if (request.BatterySlots < 1) return Result.Invalid<NodeResponse>("A node needs at least one battery slot.");

            var node = await _nodes.GetByIdAsync(id);
            if (node == null) return Result.NotFound<NodeResponse>("Node not found.");

            node.BatterySlots = request.BatterySlots;
            await _nodes.ReplaceAsync(node);
            return Result.Ok(node.ToResponse(), "Battery slots updated.");
        }

        // Re-activates a node so it accepts reservations again.
        public async Task<Result> ActivateAsync(string id) =>
            await _nodes.SetActiveAsync(id, true)
                ? Result.Ok("Node activated.")
                : Result.Fail(ErrorType.NotFound, "Node not found.");

        // Checks node fields: name, GPS range, capacity, battery slots and opening hours.
        private static string? Validate(NodeRequest r)
        {
            if (string.IsNullOrWhiteSpace(r.Name)) return "Node name is required.";
            if (r.Latitude is < -90 or > 90) return "Latitude must be between -90 and 90.";
            if (r.Longitude is < -180 or > 180) return "Longitude must be between -180 and 180.";
            if (r.CapacityKWh <= 0) return "Capacity must be greater than zero.";
            if (r.BatterySlots < 1) return "A node needs at least one battery slot.";
            if (!NodeSchedule.IsValid(r.OpenTime, r.CloseTime))
                return "Opening hours must be two HH:mm times with opening before closing, or both left empty.";
            return null;
        }

        // Copies validated request fields onto the node entity.
        private static void Apply(MicrogridNode node, NodeRequest r)
        {
            node.Name = r.Name.Trim();
            node.Latitude = r.Latitude;
            node.Longitude = r.Longitude;
            node.CapacityKWh = r.CapacityKWh;
            node.BatterySlots = r.BatterySlots;
            node.OpenTime = string.IsNullOrWhiteSpace(r.OpenTime) ? null : r.OpenTime.Trim();
            node.CloseTime = string.IsNullOrWhiteSpace(r.CloseTime) ? null : r.CloseTime.Trim();
        }
    }
}

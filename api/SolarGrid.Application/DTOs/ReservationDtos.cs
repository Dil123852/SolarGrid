/*
 * File: ReservationDtos.cs
 * Purpose: Request/response contracts for energy reservations, QR verification and dashboards.
 *          ReservationResponse carries the node name so clients can render summary screens directly.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using SolarGrid.Domain.Enums;

namespace SolarGrid.Application.DTOs
{
    // ProsumerNic is only read for Backoffice callers; prosumers always book for themselves.
    public record CreateReservationRequest(string NodeId, DateTime SlotTime, string? ProsumerNic = null);

    // NodeId is optional - omit it to keep the current node.
    public record UpdateReservationRequest(DateTime SlotTime, string? NodeId = null);

    // Search is free text matched against node name, prosumer NIC, booking reference and status.
    public record ReservationQuery(string? Nic, string? NodeId, ReservationStatus? Status, DateTime? From, DateTime? To, string? Search = null);

    public record ReservationResponse(
        string Id,
        string ProsumerNic,
        string NodeId,
        string NodeName,
        DateTime SlotTime,
        ReservationStatus Status,
        string? QrToken,
        DateTime CreatedAt,
        DateTime? UpdatedAt);

    public record VerifyQrRequest(string QrToken);

    public record DashboardResponse(long PendingCount, long ApprovedFutureCount, long CompletedCount);
}

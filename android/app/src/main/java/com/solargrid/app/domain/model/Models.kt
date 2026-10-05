/*
 * File: Models.kt
 * Purpose: Domain models used by ViewModels and screens - independent of Retrofit DTOs and SQLite.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.domain.model

import java.time.Instant

enum class Role { Backoffice, GridOperator, Prosumer }

enum class ReservationStatus { Pending, Approved, Cancelled, Completed }

// The signed-in user, cached in SQLite so the app reopens straight into the right home screen.
data class Session(
    val token: String,
    val role: Role,
    val displayName: String,
    val nic: String?,
    val expiresAt: Instant
) {
    val isExpired: Boolean get() = Instant.now().isAfter(expiresAt)
}

data class Prosumer(
    val nic: String,
    val name: String,
    val email: String,
    val phone: String,
    val address: String,
    val isActive: Boolean
)

data class MicrogridNode(
    val id: String,
    val name: String,
    val latitude: Double,
    val longitude: Double,
    val capacityKWh: Double,
    val batterySlots: Int,
    val isActive: Boolean,
    // Operating hours, "HH:mm" Sri Lanka time; null = open around the clock.
    val openTime: String? = null,
    val closeTime: String? = null
) {
    val hoursLabel: String get() = if (openTime != null && closeTime != null) "$openTime-$closeTime" else "24 hours"
}

data class Reservation(
    val id: String,
    val prosumerNic: String,
    val nodeId: String,
    val nodeName: String,
    val slotTime: Instant,
    val status: ReservationStatus,
    val qrToken: String?,
    val createdAt: Instant,
    // The published booking slot this reservation sits in (null when the station publishes none).
    val slotId: String? = null
) {
    val isUpcoming: Boolean get() = slotTime.isAfter(Instant.now())
    val isLive: Boolean get() = status == ReservationStatus.Pending || status == ReservationStatus.Approved
}

// A time window a station takes bookings in; capacity is shared by every booking inside it.
data class BookingSlot(
    val id: String,
    val nodeId: String,
    val nodeName: String,
    val start: Instant,
    val end: Instant,
    val capacity: Int,
    val booked: Int,
    val available: Int
) {
    val isFull: Boolean get() = available <= 0
    fun contains(time: Instant): Boolean = !time.isBefore(start) && time.isBefore(end)
}

data class DashboardCounts(val pending: Long, val approvedFuture: Long, val completed: Long)

data class ReservationFilter(
    val nodeId: String? = null,
    val status: ReservationStatus? = null,
    val from: Instant? = null,
    val to: Instant? = null,
    // Free text the API matches against node name, NIC, booking reference and status.
    val search: String? = null
)

// Raised when a deactivated prosumer tries to sign in (API returns 403).
class PendingActivationException(message: String) : Exception(message)

/*
 * File: Dtos.kt
 * Purpose: JSON contracts of the SolarGrid API. Response fields are nullable because Gson
 *          bypasses Kotlin constructors; mappers apply safe defaults when converting to domain models.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.data.remote.dto

// ---- Requests ----

data class LoginRequest(val username: String, val password: String)

data class ProsumerLoginRequest(val nic: String, val password: String)

// One mobile sign-in form: identifier is a prosumer NIC or a Grid Operator username.
data class MobileLoginRequest(val identifier: String, val password: String)

data class RegisterProsumerRequest(
    val nic: String,
    val name: String,
    val email: String,
    val phone: String,
    val address: String,
    val password: String
)

data class UpdateProsumerRequest(val name: String, val email: String, val phone: String, val address: String)

data class CreateReservationRequest(val nodeId: String, val slotTime: String)

data class UpdateReservationRequest(val slotTime: String, val nodeId: String?)

data class VerifyQrRequest(val qrToken: String)

// ---- Responses ----

// Error bodies carry a machine-readable code (e.g. "AccountInactive") next to the message.
data class MessageDto(val message: String?, val code: String? = null)

data class AuthResponseDto(
    val token: String?,
    val expiresAt: String?,
    val role: String?,
    val displayName: String?,
    val nic: String?
)

data class ProsumerDto(
    val nic: String?,
    val name: String?,
    val email: String?,
    val phone: String?,
    val address: String?,
    val isActive: Boolean?
)

data class NodeDto(
    val id: String?,
    val name: String?,
    val latitude: Double?,
    val longitude: Double?,
    val capacityKWh: Double?,
    val batterySlots: Int?,
    val isActive: Boolean?,
    val openTime: String? = null,
    val closeTime: String? = null
)

data class ReservationDto(
    val id: String?,
    val prosumerNic: String?,
    val nodeId: String?,
    val nodeName: String?,
    val slotTime: String?,
    val status: String?,
    val qrToken: String?,
    val createdAt: String?,
    val slotId: String? = null
)

// A bookable time window a station has published, with live booked / available counts.
data class BookingSlotDto(
    val id: String?,
    val nodeId: String?,
    val nodeName: String?,
    val startTime: String?,
    val endTime: String?,
    val capacity: Int?,
    val booked: Int?,
    val available: Int?,
    val isActive: Boolean?
)

data class DashboardDto(val pendingCount: Long?, val approvedFutureCount: Long?, val completedCount: Long?)

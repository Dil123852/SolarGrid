/*
 * File: Mappers.kt
 * Purpose: DTO -> domain conversions, including tolerant parsing of the API's UTC timestamps.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.data.repository

import com.solargrid.app.data.remote.dto.AuthResponseDto
import com.solargrid.app.data.remote.dto.DashboardDto
import com.solargrid.app.data.remote.dto.NodeDto
import com.solargrid.app.data.remote.dto.ProsumerDto
import com.solargrid.app.data.remote.dto.ReservationDto
import com.solargrid.app.domain.model.DashboardCounts
import com.solargrid.app.domain.model.MicrogridNode
import com.solargrid.app.domain.model.Prosumer
import com.solargrid.app.domain.model.Reservation
import com.solargrid.app.domain.model.ReservationStatus
import com.solargrid.app.domain.model.Role
import com.solargrid.app.domain.model.Session
import java.time.Instant
import java.time.LocalDateTime
import java.time.ZoneOffset

// Accepts "…Z" instants and offset-less values (treated as UTC, as the API stores them).
internal fun parseInstant(value: String?): Instant {
    if (value.isNullOrBlank()) return Instant.EPOCH
    return runCatching { Instant.parse(value) }
        .recoverCatching { LocalDateTime.parse(value).toInstant(ZoneOffset.UTC) }
        .getOrDefault(Instant.EPOCH)
}

internal fun AuthResponseDto.toSession(): Session = Session(
    token = token.orEmpty(),
    role = runCatching { Role.valueOf(role.orEmpty()) }.getOrDefault(Role.Prosumer),
    displayName = displayName.orEmpty(),
    nic = nic,
    expiresAt = parseInstant(expiresAt)
)

internal fun ProsumerDto.toDomain(): Prosumer = Prosumer(
    nic = nic.orEmpty(),
    name = name.orEmpty(),
    email = email.orEmpty(),
    phone = phone.orEmpty(),
    address = address.orEmpty(),
    isActive = isActive ?: false
)

internal fun NodeDto.toDomain(): MicrogridNode = MicrogridNode(
    id = id.orEmpty(),
    name = name.orEmpty(),
    latitude = latitude ?: 0.0,
    longitude = longitude ?: 0.0,
    capacityKWh = capacityKWh ?: 0.0,
    batterySlots = batterySlots ?: 0,
    isActive = isActive ?: false
)

internal fun ReservationDto.toDomain(): Reservation = Reservation(
    id = id.orEmpty(),
    prosumerNic = prosumerNic.orEmpty(),
    nodeId = nodeId.orEmpty(),
    nodeName = nodeName ?: "Unknown node",
    slotTime = parseInstant(slotTime),
    status = runCatching { ReservationStatus.valueOf(status.orEmpty()) }.getOrDefault(ReservationStatus.Pending),
    qrToken = qrToken,
    createdAt = parseInstant(createdAt)
)

internal fun DashboardDto.toDomain(): DashboardCounts =
    DashboardCounts(pendingCount ?: 0, approvedFutureCount ?: 0, completedCount ?: 0)

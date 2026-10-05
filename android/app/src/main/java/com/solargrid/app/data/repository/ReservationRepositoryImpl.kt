/*
 * File: ReservationRepositoryImpl.kt
 * Purpose: Reservation create/update/cancel/list, QR verification and dashboard counts via the API.
 *          The API scopes prosumer calls to their own NIC from the JWT, so no NIC is sent here.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.data.repository

import com.solargrid.app.data.remote.ApiService
import com.solargrid.app.data.remote.apiCall
import com.solargrid.app.data.remote.dto.CreateReservationRequest
import com.solargrid.app.data.remote.dto.UpdateReservationRequest
import com.solargrid.app.data.remote.dto.VerifyQrRequest
import com.solargrid.app.domain.model.DashboardCounts
import com.solargrid.app.domain.model.Reservation
import com.solargrid.app.domain.model.ReservationFilter
import com.solargrid.app.domain.repository.ReservationRepository
import java.time.Instant

class ReservationRepositoryImpl(private val api: ApiService) : ReservationRepository {

    override suspend fun list(filter: ReservationFilter): Result<List<Reservation>> =
        apiCall {
            api.getReservations(
                nodeId = filter.nodeId,
                status = filter.status?.name,
                from = filter.from?.toString(),
                to = filter.to?.toString(),
                search = filter.search?.takeIf { it.isNotBlank() }?.trim()
            )
        }.map { list -> list.map { it.toDomain() } }

    override suspend fun get(id: String): Result<Reservation> =
        apiCall { api.getReservation(id) }.map { it.toDomain() }

    override suspend fun create(nodeId: String, slotTime: Instant): Result<Reservation> =
        apiCall { api.createReservation(CreateReservationRequest(nodeId, slotTime.toString())) }.map { it.toDomain() }

    override suspend fun update(id: String, nodeId: String, slotTime: Instant): Result<Reservation> =
        apiCall { api.updateReservation(id, UpdateReservationRequest(slotTime.toString(), nodeId)) }.map { it.toDomain() }

    override suspend fun cancel(id: String): Result<Reservation> =
        apiCall { api.cancelReservation(id) }.map { it.toDomain() }

    override suspend fun verifyQr(token: String): Result<Reservation> =
        apiCall { api.verifyQr(VerifyQrRequest(token.trim())) }.map { it.toDomain() }

    override suspend fun dashboard(): Result<DashboardCounts> =
        apiCall { api.dashboard() }.map { it.toDomain() }
}

/*
 * File: Repositories.kt
 * Purpose: Repository contracts the ViewModels depend on. Implementations live in data/repository
 *          and combine the Retrofit API with the SQLite cache.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.domain.repository

import com.solargrid.app.domain.model.DashboardCounts
import com.solargrid.app.domain.model.MicrogridNode
import com.solargrid.app.domain.model.Prosumer
import com.solargrid.app.domain.model.Reservation
import com.solargrid.app.domain.model.ReservationFilter
import com.solargrid.app.domain.model.Session
import java.time.Instant

interface AuthRepository {
    fun currentSession(): Session?
    // One sign-in for the app: identifier is a prosumer NIC or a Grid Operator username.
    suspend fun login(identifier: String, password: String): Result<Session>
    suspend fun registerProsumer(
        nic: String, name: String, email: String, phone: String, address: String, password: String
    ): Result<Session>
    fun logout()
}

interface ProsumerRepository {
    suspend fun getProfile(nic: String): Result<Prosumer>
    suspend fun updateProfile(nic: String, name: String, email: String, phone: String, address: String): Result<Prosumer>
    suspend fun deactivate(nic: String): Result<String>
}

interface NodeRepository {
    // Falls back to the SQLite cache when the API is unreachable.
    suspend fun getNodes(activeOnly: Boolean = false): Result<List<MicrogridNode>>
}

interface ReservationRepository {
    suspend fun list(filter: ReservationFilter = ReservationFilter()): Result<List<Reservation>>
    suspend fun get(id: String): Result<Reservation>
    suspend fun create(nodeId: String, slotTime: Instant): Result<Reservation>
    suspend fun update(id: String, nodeId: String, slotTime: Instant): Result<Reservation>
    suspend fun cancel(id: String): Result<Reservation>
    suspend fun verifyQr(token: String): Result<Reservation>
    suspend fun dashboard(): Result<DashboardCounts>
}

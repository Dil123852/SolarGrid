/*
 * File: NodeRepositoryImpl.kt
 * Purpose: Loads microgrid nodes from the API and mirrors them into SQLite; when offline,
 *          serves the cached copy instead.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.data.repository

import com.solargrid.app.data.local.SolarGridDbHelper
import com.solargrid.app.data.remote.ApiService
import com.solargrid.app.data.remote.apiCall
import com.solargrid.app.domain.model.BookingSlot
import com.solargrid.app.domain.model.MicrogridNode
import com.solargrid.app.domain.repository.NodeRepository
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.time.Instant

class NodeRepositoryImpl(
    private val api: ApiService,
    private val db: SolarGridDbHelper
) : NodeRepository {

    override suspend fun getNodes(activeOnly: Boolean): Result<List<MicrogridNode>> {
        val remote = apiCall { api.getNodes(null) }.map { list -> list.map { it.toDomain() } }

        val nodes = remote.getOrNull()
        if (nodes != null) {
            withContext(Dispatchers.IO) { db.replaceNodes(nodes) }
            return Result.success(nodes.filter { !activeOnly || it.isActive })
        }

        val cached = withContext(Dispatchers.IO) { db.loadNodes() }
        return if (cached.isNotEmpty()) Result.success(cached.filter { !activeOnly || it.isActive }) else remote
    }

    // Availability changes with every booking, so slots always come from the API.
    override suspend fun getSlots(nodeId: String, from: Instant, to: Instant): Result<List<BookingSlot>> =
        apiCall { api.getSlots(nodeId, from.toString(), to.toString()) }
            .map { list -> list.filter { it.isActive != false }.map { it.toDomain() }.sortedBy { it.start } }
}

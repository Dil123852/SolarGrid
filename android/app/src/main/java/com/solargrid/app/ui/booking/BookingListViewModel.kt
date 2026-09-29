/*
 * File: BookingListViewModel.kt
 * Purpose: Booking lists - "Current" (pending/approved and still upcoming) and full "History",
 *          filtered by node and status. Filters are sent to the API as query parameters.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.booking

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.solargrid.app.domain.model.MicrogridNode
import com.solargrid.app.domain.model.Reservation
import com.solargrid.app.domain.model.ReservationFilter
import com.solargrid.app.domain.model.ReservationStatus
import com.solargrid.app.domain.repository.NodeRepository
import com.solargrid.app.domain.repository.ReservationRepository
import kotlinx.coroutines.launch

enum class BookingTab { Current, History }

class BookingListViewModel(
    private val reservations: ReservationRepository,
    private val nodes: NodeRepository
) : ViewModel() {

    var tab by mutableStateOf(BookingTab.Current)
        private set
    var nodeFilter by mutableStateOf<MicrogridNode?>(null)
        private set
    var statusFilter by mutableStateOf<ReservationStatus?>(null)
        private set
    var nodeOptions by mutableStateOf<List<MicrogridNode>>(emptyList())
        private set

    private var all by mutableStateOf<List<Reservation>>(emptyList())

    var loading by mutableStateOf(false)
        private set
    var error by mutableStateOf<String?>(null)
        private set

    // Current = still actionable; History = everything, newest first.
    val visible: List<Reservation>
        get() = when (tab) {
            BookingTab.Current -> all.filter { it.isLive && it.isUpcoming }.sortedBy { it.slotTime }
            BookingTab.History -> all.sortedByDescending { it.slotTime }
        }

    init {
        viewModelScope.launch { nodes.getNodes().onSuccess { nodeOptions = it } }
    }

    fun selectTab(newTab: BookingTab) {
        tab = newTab
    }

    fun filterByNode(node: MicrogridNode?) {
        nodeFilter = node
        load()
    }

    fun filterByStatus(status: ReservationStatus?) {
        statusFilter = status
        load()
    }

    fun load() {
        loading = true
        error = null
        viewModelScope.launch {
            reservations.list(ReservationFilter(nodeId = nodeFilter?.id, status = statusFilter))
                .onSuccess { all = it }
                .onFailure { error = it.message }
            loading = false
        }
    }
}

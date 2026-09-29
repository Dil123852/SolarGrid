/*
 * File: BookingFormViewModel.kt
 * Purpose: Create / reschedule booking state - node choice plus local date and time, converted
 *          to a UTC instant for the API. The API enforces the 7-day window and 12-hour notice.
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
import com.solargrid.app.domain.repository.NodeRepository
import com.solargrid.app.domain.repository.ReservationRepository
import kotlinx.coroutines.launch
import java.time.Duration
import java.time.Instant
import java.time.LocalDate
import java.time.LocalTime
import java.time.ZoneId

class BookingFormViewModel(
    private val reservationId: String?,
    private val reservations: ReservationRepository,
    private val nodes: NodeRepository
) : ViewModel() {

    val isEdit: Boolean = reservationId != null

    var nodeOptions by mutableStateOf<List<MicrogridNode>>(emptyList())
        private set
    var selectedNode by mutableStateOf<MicrogridNode?>(null)
    var date by mutableStateOf(LocalDate.now().plusDays(1))
    var time by mutableStateOf(LocalTime.of(9, 0))

    var loading by mutableStateOf(true)
        private set
    var saving by mutableStateOf(false)
        private set
    var error by mutableStateOf<String?>(null)
        private set
    var savedId by mutableStateOf<String?>(null)
        private set

    init {
        viewModelScope.launch {
            nodes.getNodes(activeOnly = true)
                .onSuccess { nodeOptions = it }
                .onFailure { error = it.message }

            if (reservationId != null) {
                reservations.get(reservationId)
                    .onSuccess { r ->
                        val local = r.slotTime.atZone(ZoneId.systemDefault())
                        date = local.toLocalDate()
                        time = local.toLocalTime().withSecond(0).withNano(0)
                        selectedNode = nodeOptions.firstOrNull { it.id == r.nodeId }
                    }
                    .onFailure { error = it.message }
            }
            if (selectedNode == null) selectedNode = nodeOptions.firstOrNull()
            loading = false
        }
    }

    fun save() {
        val node = selectedNode
        if (node == null) {
            error = "Choose a microgrid node."
            return
        }
        val slot = date.atTime(time).atZone(ZoneId.systemDefault()).toInstant()
        val now = Instant.now()
        // Quick local check; the API has the final say.
        if (!slot.isAfter(now) || slot.isAfter(now.plus(Duration.ofDays(7)))) {
            error = "Pick a time within the next 7 days."
            return
        }

        saving = true
        error = null
        viewModelScope.launch {
            val result = if (reservationId == null) reservations.create(node.id, slot)
            else reservations.update(reservationId, node.id, slot)
            result.onSuccess { savedId = it.id }.onFailure { error = it.message }
            saving = false
        }
    }
}

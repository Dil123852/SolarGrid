/*
 * File: BookingFormViewModel.kt
 * Purpose: Create / reschedule booking state - station choice, local date, and either one of the
 *          station's published booking slots or (when it publishes none) a free time, converted to
 *          a UTC instant for the API. The API enforces the 7-day window, 12-hour notice and slot capacity.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.booking

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.solargrid.app.domain.model.BookingSlot
import com.solargrid.app.domain.model.MicrogridNode
import com.solargrid.app.domain.repository.NodeRepository
import com.solargrid.app.domain.repository.ReservationRepository
import kotlinx.coroutines.Job
import kotlinx.coroutines.launch
import java.time.Duration
import java.time.Instant
import java.time.LocalDate
import java.time.LocalTime
import java.time.ZoneId
import java.time.temporal.ChronoUnit

class BookingFormViewModel(
    private val reservationId: String?,
    private val reservations: ReservationRepository,
    private val nodes: NodeRepository
) : ViewModel() {

    val isEdit: Boolean = reservationId != null
    private val zone: ZoneId = ZoneId.systemDefault()

    var nodeOptions by mutableStateOf<List<MicrogridNode>>(emptyList())
        private set
    var selectedNode by mutableStateOf<MicrogridNode?>(null)
        private set
    var date by mutableStateOf(LocalDate.now().plusDays(1))
        private set
    var time by mutableStateOf(LocalTime.of(9, 0))
        private set

    // Slots the chosen station has published on the chosen day; empty = book any time in its hours.
    var slots by mutableStateOf<List<BookingSlot>>(emptyList())
        private set
    var slotsLoading by mutableStateOf(false)
        private set
    var selectedSlotId by mutableStateOf<String?>(null)
        private set
    // When rescheduling, the slot the booking already holds (selectable even if otherwise full).
    private var currentSlotId: String? = null
    private var slotJob: Job? = null

    var loading by mutableStateOf(true)
        private set
    var saving by mutableStateOf(false)
        private set
    var error by mutableStateOf<String?>(null)
        private set
    var savedId by mutableStateOf<String?>(null)
        private set

    // Loads active stations and, when rescheduling, the booking being changed; then that day's slots.
    init {
        viewModelScope.launch {
            nodes.getNodes(activeOnly = true)
                .onSuccess { nodeOptions = it }
                .onFailure { error = it.message }

            if (reservationId != null) {
                reservations.get(reservationId)
                    .onSuccess { r ->
                        val local = r.slotTime.atZone(zone)
                        date = local.toLocalDate()
                        time = local.toLocalTime().withSecond(0).withNano(0)
                        selectedNode = nodeOptions.firstOrNull { it.id == r.nodeId }
                        currentSlotId = r.slotId
                        selectedSlotId = r.slotId
                    }
                    .onFailure { error = it.message }
            }
            if (selectedNode == null) selectedNode = nodeOptions.firstOrNull()
            loading = false
            loadSlots()
        }
    }

    // Picks a station and loads its slots for the chosen day.
    fun selectNode(node: MicrogridNode) {
        if (node.id == selectedNode?.id) return
        selectedNode = node
        selectedSlotId = null
        loadSlots()
    }

    // Picks a day and loads the station's slots for it.
    fun selectDate(day: LocalDate) {
        if (day == date) return
        date = day
        selectedSlotId = null
        loadSlots()
    }

    // Free time choice, used when the station publishes no slots that day.
    fun selectTime(value: LocalTime) {
        time = value
        selectedSlotId = slots.firstOrNull { it.contains(instantOf(date, value)) }?.id
    }

    // Books a published slot: its start, or the next quarter hour if it has already begun.
    fun selectSlot(slot: BookingSlot) {
        val soon = Instant.now().plus(Duration.ofMinutes(15)).truncatedTo(ChronoUnit.MINUTES)
        val at = if (slot.start.isAfter(soon)) slot.start else soon
        if (!slot.contains(at)) {
            error = "This slot is about to end. Choose a later one."
            return
        }
        error = null
        time = at.atZone(zone).toLocalTime().withSecond(0).withNano(0)
        selectedSlotId = slot.id
    }

    // Whether a slot can be picked: not over, and with room (or already holding this booking).
    fun isSelectable(slot: BookingSlot): Boolean =
        slot.end.isAfter(Instant.now()) && (!slot.isFull || slot.id == currentSlotId)

    // Fetches the chosen station's published slots for the chosen local day.
    private fun loadSlots() {
        val node = selectedNode ?: return
        slotJob?.cancel()
        slotsLoading = true
        slotJob = viewModelScope.launch {
            val from = date.atStartOfDay(zone).toInstant()
            val to = date.plusDays(1).atStartOfDay(zone).toInstant()
            nodes.getSlots(node.id, from, to)
                .onSuccess { list ->
                    slots = list.filter { it.end.isAfter(Instant.now()) }
                    if (slots.none { it.id == selectedSlotId }) selectedSlotId = null
                }
                .onFailure { slots = emptyList() }
            slotsLoading = false
        }
    }

    private fun instantOf(day: LocalDate, at: LocalTime): Instant = day.atTime(at).atZone(zone).toInstant()

    // Sends the booking (or reschedule) to the API after quick local checks.
    fun save() {
        val node = selectedNode
        if (node == null) {
            error = "Choose a solar station."
            return
        }
        if (slots.isNotEmpty() && selectedSlotId == null) {
            error = "${node.name} takes bookings in its published slots. Choose one above."
            return
        }
        val slotTime = instantOf(date, time)
        val now = Instant.now()
        // Quick local check; the API has the final say.
        if (!slotTime.isAfter(now) || slotTime.isAfter(now.plus(Duration.ofDays(7)))) {
            error = "Pick a time within the next 7 days."
            return
        }

        saving = true
        error = null
        viewModelScope.launch {
            val result = if (reservationId == null) reservations.create(node.id, slotTime)
            else reservations.update(reservationId, node.id, slotTime)
            result.onSuccess { savedId = it.id }.onFailure { error = it.message }
            saving = false
        }
    }
}

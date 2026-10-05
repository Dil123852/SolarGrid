/*
 * File: BookingDetailViewModel.kt
 * Purpose: One booking's details, its QR code (once approved), and cancellation.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.booking

import android.graphics.Bitmap
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.google.zxing.BarcodeFormat
import com.journeyapps.barcodescanner.BarcodeEncoder
import com.solargrid.app.domain.model.BookingSlot
import com.solargrid.app.domain.model.Reservation
import com.solargrid.app.domain.repository.NodeRepository
import com.solargrid.app.domain.repository.ReservationRepository
import kotlinx.coroutines.launch

class BookingDetailViewModel(
    private val reservationId: String,
    private val reservations: ReservationRepository,
    private val nodes: NodeRepository
) : ViewModel() {

    var reservation by mutableStateOf<Reservation?>(null)
        private set
    var qrBitmap by mutableStateOf<Bitmap?>(null)
        private set
    // The published booking slot the reservation sits in, when the station uses slots.
    var slot by mutableStateOf<BookingSlot?>(null)
        private set
    var loading by mutableStateOf(false)
        private set
    var cancelling by mutableStateOf(false)
        private set
    var error by mutableStateOf<String?>(null)
        private set
    var cancelled by mutableStateOf(false)
        private set

    fun load() {
        loading = true
        viewModelScope.launch {
            reservations.get(reservationId)
                .onSuccess { r ->
                    reservation = r
                    qrBitmap = r.qrToken?.let { token ->
                        runCatching { BarcodeEncoder().encodeBitmap(token, BarcodeFormat.QR_CODE, 600, 600) }.getOrNull()
                    }
                }
                .onFailure { error = it.message }
            reservation?.let { r ->
                if (r.slotId != null) {
                    nodes.getSlots(r.nodeId, r.slotTime, r.slotTime.plusSeconds(1))
                        .onSuccess { list -> slot = list.firstOrNull { it.id == r.slotId } }
                }
            }
            loading = false
        }
    }

    fun cancel() {
        cancelling = true
        error = null
        viewModelScope.launch {
            reservations.cancel(reservationId)
                .onSuccess { cancelled = true }
                .onFailure { error = it.message }
            cancelling = false
        }
    }
}

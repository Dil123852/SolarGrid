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
import com.solargrid.app.domain.model.Reservation
import com.solargrid.app.domain.repository.ReservationRepository
import kotlinx.coroutines.launch

class BookingDetailViewModel(
    private val reservationId: String,
    private val reservations: ReservationRepository
) : ViewModel() {

    var reservation by mutableStateOf<Reservation?>(null)
        private set
    var qrBitmap by mutableStateOf<Bitmap?>(null)
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

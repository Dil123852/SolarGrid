/*
 * File: BookingSummaryViewModel.kt
 * Purpose: Loads the booking that was just created, updated or cancelled, straight from the API,
 *          so the summary always reflects what the server stored.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.booking

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.solargrid.app.domain.model.Reservation
import com.solargrid.app.domain.repository.ReservationRepository
import kotlinx.coroutines.launch

class BookingSummaryViewModel(
    reservationId: String,
    reservations: ReservationRepository
) : ViewModel() {

    var reservation by mutableStateOf<Reservation?>(null)
        private set
    var error by mutableStateOf<String?>(null)
        private set

    init {
        viewModelScope.launch {
            reservations.get(reservationId)
                .onSuccess { reservation = it }
                .onFailure { error = it.message }
        }
    }
}

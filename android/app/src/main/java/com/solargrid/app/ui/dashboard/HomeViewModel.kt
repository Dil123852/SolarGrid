/*
 * File: HomeViewModel.kt
 * Purpose: Prosumer dashboard state - live pending / approved-upcoming counts from the API
 *          plus the next few upcoming bookings. Nothing here is hard-coded.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.dashboard

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.solargrid.app.domain.model.DashboardCounts
import com.solargrid.app.domain.model.Reservation
import com.solargrid.app.domain.model.ReservationFilter
import com.solargrid.app.domain.repository.AuthRepository
import com.solargrid.app.domain.repository.ReservationRepository
import kotlinx.coroutines.async
import kotlinx.coroutines.launch
import java.time.Instant

class HomeViewModel(
    auth: AuthRepository,
    private val reservations: ReservationRepository
) : ViewModel() {

    val displayName: String = auth.currentSession()?.displayName.orEmpty()

    var counts by mutableStateOf<DashboardCounts?>(null)
        private set
    var upcoming by mutableStateOf<List<Reservation>>(emptyList())
        private set
    var loading by mutableStateOf(false)
        private set
    var error by mutableStateOf<String?>(null)
        private set

    fun load() {
        loading = true
        error = null
        viewModelScope.launch {
            val countsCall = async { reservations.dashboard() }
            val listCall = async { reservations.list(ReservationFilter(from = Instant.now())) }

            countsCall.await().onSuccess { counts = it }.onFailure { error = it.message }
            listCall.await()
                .onSuccess { list -> upcoming = list.filter { it.isLive }.sortedBy { it.slotTime }.take(3) }
                .onFailure { error = it.message }
            loading = false
        }
    }
}

/*
 * File: OperatorViewModel.kt
 * Purpose: Operator Mode state - sends a scanned (or typed) QR token to
 *          /api/reservations/verify-qr and exposes success or failure.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.operator

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.solargrid.app.domain.model.Reservation
import com.solargrid.app.domain.repository.AuthRepository
import com.solargrid.app.domain.repository.ReservationRepository
import kotlinx.coroutines.launch

sealed interface VerifyResult {
    data class Success(val reservation: Reservation) : VerifyResult
    data class Failure(val message: String) : VerifyResult
}

class OperatorViewModel(
    auth: AuthRepository,
    private val reservations: ReservationRepository
) : ViewModel() {

    val operatorName: String = auth.currentSession()?.displayName.orEmpty()

    var manualToken by mutableStateOf("")
    var verifying by mutableStateOf(false)
        private set
    var result by mutableStateOf<VerifyResult?>(null)
        private set

    fun verify(token: String) {
        if (token.isBlank()) {
            result = VerifyResult.Failure("No QR token was read.")
            return
        }
        verifying = true
        result = null
        viewModelScope.launch {
            result = reservations.verifyQr(token).fold(
                onSuccess = { VerifyResult.Success(it) },
                onFailure = { VerifyResult.Failure(it.message ?: "Verification failed.") }
            )
            verifying = false
            manualToken = ""
        }
    }
}

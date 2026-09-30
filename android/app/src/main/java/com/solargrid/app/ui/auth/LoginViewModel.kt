/*
 * File: LoginViewModel.kt
 * Purpose: Sign-in state - one "NIC or username" field for everyone who uses the app. The API
 *          decides the account type; the returned role routes prosumers to their dashboard and
 *          Grid Operators to Operator Mode.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.auth

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.solargrid.app.domain.model.PendingActivationException
import com.solargrid.app.domain.model.Role
import com.solargrid.app.domain.repository.AuthRepository
import kotlinx.coroutines.launch

sealed interface LoginEvent {
    data class LoggedIn(val role: Role) : LoginEvent
    data class PendingActivation(val message: String) : LoginEvent
}

class LoginViewModel(private val auth: AuthRepository) : ViewModel() {

    var identifier by mutableStateOf("")
    var password by mutableStateOf("")
    var loading by mutableStateOf(false)
        private set
    var error by mutableStateOf<String?>(null)
        private set
    var event by mutableStateOf<LoginEvent?>(null)
        private set

    fun submit() {
        if (identifier.isBlank() || password.isBlank()) {
            error = "Enter your NIC or username and your password."
            return
        }
        loading = true
        error = null
        viewModelScope.launch {
            auth.login(identifier, password)
                .onSuccess { event = LoginEvent.LoggedIn(it.role) }
                .onFailure {
                    if (it is PendingActivationException) event = LoginEvent.PendingActivation(it.message.orEmpty())
                    else error = it.message
                }
            loading = false
        }
    }

    fun consumeEvent() {
        event = null
    }
}

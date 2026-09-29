/*
 * File: LoginViewModel.kt
 * Purpose: Login state - prosumers sign in with NIC, grid operators with their username.
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

enum class LoginMode { Prosumer, Operator }

sealed interface LoginEvent {
    data class LoggedIn(val role: Role) : LoginEvent
    data class PendingActivation(val message: String) : LoginEvent
}

class LoginViewModel(private val auth: AuthRepository) : ViewModel() {

    var mode by mutableStateOf(LoginMode.Prosumer)
        private set
    var identifier by mutableStateOf("")
    var password by mutableStateOf("")
    var loading by mutableStateOf(false)
        private set
    var error by mutableStateOf<String?>(null)
        private set
    var event by mutableStateOf<LoginEvent?>(null)
        private set

    fun switchMode(newMode: LoginMode) {
        mode = newMode
        error = null
    }

    fun submit() {
        if (identifier.isBlank() || password.isBlank()) {
            error = if (mode == LoginMode.Prosumer) "Enter your NIC and password." else "Enter your username and password."
            return
        }
        loading = true
        error = null
        viewModelScope.launch {
            val result = if (mode == LoginMode.Prosumer) auth.loginProsumer(identifier, password)
            else auth.loginOperator(identifier, password)
            loading = false
            result
                .onSuccess { event = LoginEvent.LoggedIn(it.role) }
                .onFailure {
                    if (it is PendingActivationException) event = LoginEvent.PendingActivation(it.message.orEmpty())
                    else error = it.message
                }
        }
    }

    fun consumeEvent() {
        event = null
    }
}

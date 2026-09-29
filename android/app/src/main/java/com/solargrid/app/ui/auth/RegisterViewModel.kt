/*
 * File: RegisterViewModel.kt
 * Purpose: Prosumer self-registration state. Light client-side checks for fast feedback;
 *          the API remains the authority on every rule (NIC format, uniqueness, etc.).
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.auth

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.solargrid.app.domain.repository.AuthRepository
import kotlinx.coroutines.launch

class RegisterViewModel(private val auth: AuthRepository) : ViewModel() {

    var nic by mutableStateOf("")
    var name by mutableStateOf("")
    var email by mutableStateOf("")
    var phone by mutableStateOf("")
    var address by mutableStateOf("")
    var password by mutableStateOf("")
    var confirmPassword by mutableStateOf("")

    var loading by mutableStateOf(false)
        private set
    var error by mutableStateOf<String?>(null)
        private set
    var registered by mutableStateOf(false)
        private set

    fun submit() {
        error = validate()
        if (error != null) return

        loading = true
        viewModelScope.launch {
            auth.registerProsumer(nic, name, email, phone, address, password)
                .onSuccess { registered = true }
                .onFailure { error = it.message }
            loading = false
        }
    }

    private fun validate(): String? = when {
        !Regex("^(\\d{9}[VvXx]|\\d{12})$").matches(nic.trim()) -> "NIC must be 9 digits + V/X, or 12 digits."
        name.isBlank() -> "Enter your name."
        !email.contains("@") -> "Enter a valid email."
        phone.isBlank() -> "Enter your phone number."
        password.length < 6 -> "Password must be at least 6 characters."
        password != confirmPassword -> "Passwords do not match."
        else -> null
    }
}

/*
 * File: ProfileViewModel.kt
 * Purpose: Prosumer profile editing and self-deactivation (reactivation is Backoffice-only).
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.profile

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.solargrid.app.domain.repository.AuthRepository
import com.solargrid.app.domain.repository.ProsumerRepository
import kotlinx.coroutines.launch

class ProfileViewModel(
    auth: AuthRepository,
    private val prosumers: ProsumerRepository
) : ViewModel() {

    val nic: String = auth.currentSession()?.nic.orEmpty()

    var name by mutableStateOf("")
    var email by mutableStateOf("")
    var phone by mutableStateOf("")
    var address by mutableStateOf("")

    var loading by mutableStateOf(true)
        private set
    var saving by mutableStateOf(false)
        private set
    var message by mutableStateOf<String?>(null)
        private set
    var error by mutableStateOf<String?>(null)
        private set
    var deactivatedMessage by mutableStateOf<String?>(null)
        private set

    init {
        viewModelScope.launch {
            prosumers.getProfile(nic)
                .onSuccess {
                    name = it.name
                    email = it.email
                    phone = it.phone
                    address = it.address
                }
                .onFailure { error = it.message }
            loading = false
        }
    }

    fun save() {
        saving = true
        error = null
        message = null
        viewModelScope.launch {
            prosumers.updateProfile(nic, name, email, phone, address)
                .onSuccess { message = "Profile updated." }
                .onFailure { error = it.message }
            saving = false
        }
    }

    fun deactivate() {
        saving = true
        error = null
        viewModelScope.launch {
            prosumers.deactivate(nic)
                .onSuccess { deactivatedMessage = it }
                .onFailure { error = it.message }
            saving = false
        }
    }
}

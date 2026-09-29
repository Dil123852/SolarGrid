/*
 * File: ProsumerRepositoryImpl.kt
 * Purpose: Prosumer profile read/update and self-deactivation via the API.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.data.repository

import com.solargrid.app.data.remote.ApiService
import com.solargrid.app.data.remote.apiCall
import com.solargrid.app.data.remote.dto.UpdateProsumerRequest
import com.solargrid.app.domain.model.Prosumer
import com.solargrid.app.domain.repository.ProsumerRepository

class ProsumerRepositoryImpl(private val api: ApiService) : ProsumerRepository {

    override suspend fun getProfile(nic: String): Result<Prosumer> =
        apiCall { api.getProsumer(nic) }.map { it.toDomain() }

    override suspend fun updateProfile(
        nic: String, name: String, email: String, phone: String, address: String
    ): Result<Prosumer> =
        apiCall { api.updateProsumer(nic, UpdateProsumerRequest(name.trim(), email.trim(), phone.trim(), address.trim())) }
            .map { it.toDomain() }

    override suspend fun deactivate(nic: String): Result<String> =
        apiCall { api.deactivateProsumer(nic) }.map { it.message ?: "Account deactivated." }
}

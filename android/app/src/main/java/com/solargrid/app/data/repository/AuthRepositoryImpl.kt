/*
 * File: AuthRepositoryImpl.kt
 * Purpose: Single sign-in (prosumer NIC or Grid Operator username - the API tells them apart),
 *          prosumer self-registration, and session persistence in SQLite via SessionStore.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.data.repository

import com.solargrid.app.data.local.SessionStore
import com.solargrid.app.data.remote.ApiException
import com.solargrid.app.data.remote.ApiService
import com.solargrid.app.data.remote.apiCall
import com.solargrid.app.data.remote.dto.MobileLoginRequest
import com.solargrid.app.data.remote.dto.RegisterProsumerRequest
import com.solargrid.app.domain.model.PendingActivationException
import com.solargrid.app.domain.model.Session
import com.solargrid.app.domain.repository.AuthRepository

class AuthRepositoryImpl(
    private val api: ApiService,
    private val sessionStore: SessionStore
) : AuthRepository {

    override fun currentSession(): Session? = sessionStore.get()

    // The API decides whether the identifier is a NIC (prosumer) or a username (staff) and refuses
    // Backoffice accounts; a deactivated account comes back as 403 with code "AccountInactive".
    override suspend fun login(identifier: String, password: String): Result<Session> {
        val result = apiCall { api.mobileLogin(MobileLoginRequest(identifier.trim(), password)) }
        val error = result.exceptionOrNull()
        if (error is ApiException && error.errorCode == ACCOUNT_INACTIVE) {
            return Result.failure(PendingActivationException(error.message ?: "Account pending activation."))
        }
        return result.map { it.toSession() }.onSuccess(sessionStore::save)
    }

    override suspend fun registerProsumer(
        nic: String, name: String, email: String, phone: String, address: String, password: String
    ): Result<Session> {
        val registered = apiCall {
            api.registerProsumer(RegisterProsumerRequest(nic.trim(), name.trim(), email.trim(), phone.trim(), address.trim(), password))
        }
        return registered.fold(
            onSuccess = { login(nic, password) },
            onFailure = { Result.failure(it) }
        )
    }

    override fun logout() = sessionStore.clear()

    private companion object {
        const val ACCOUNT_INACTIVE = "AccountInactive"
    }
}

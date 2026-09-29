/*
 * File: AuthRepositoryImpl.kt
 * Purpose: Prosumer (NIC) and Grid Operator (username) sign-in, prosumer self-registration,
 *          and session persistence in SQLite via SessionStore.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.data.repository

import com.solargrid.app.data.local.SessionStore
import com.solargrid.app.data.remote.ApiException
import com.solargrid.app.data.remote.ApiService
import com.solargrid.app.data.remote.apiCall
import com.solargrid.app.data.remote.dto.LoginRequest
import com.solargrid.app.data.remote.dto.ProsumerLoginRequest
import com.solargrid.app.data.remote.dto.RegisterProsumerRequest
import com.solargrid.app.domain.model.PendingActivationException
import com.solargrid.app.domain.model.Role
import com.solargrid.app.domain.model.Session
import com.solargrid.app.domain.repository.AuthRepository

class AuthRepositoryImpl(
    private val api: ApiService,
    private val sessionStore: SessionStore
) : AuthRepository {

    override fun currentSession(): Session? = sessionStore.get()

    override suspend fun loginProsumer(nic: String, password: String): Result<Session> {
        val result = apiCall { api.prosumerLogin(ProsumerLoginRequest(nic.trim(), password)) }
        val error = result.exceptionOrNull()
        // 403 from prosumer-login means the account is deactivated and waiting for Backoffice.
        if (error is ApiException && error.code == 403) {
            return Result.failure(PendingActivationException(error.message ?: "Account pending activation."))
        }
        return result.map { it.toSession() }.onSuccess(sessionStore::save)
    }

    override suspend fun loginOperator(username: String, password: String): Result<Session> =
        apiCall { api.login(LoginRequest(username.trim(), password)) }
            .map { it.toSession() }
            .mapCatching { session ->
                if (session.role != Role.GridOperator) {
                    throw IllegalStateException("Operator Mode is for Grid Operators. Backoffice staff use the web portal.")
                }
                session
            }
            .onSuccess(sessionStore::save)

    override suspend fun registerProsumer(
        nic: String, name: String, email: String, phone: String, address: String, password: String
    ): Result<Session> {
        val registered = apiCall {
            api.registerProsumer(RegisterProsumerRequest(nic.trim(), name.trim(), email.trim(), phone.trim(), address.trim(), password))
        }
        return registered.fold(
            onSuccess = { loginProsumer(nic, password) },
            onFailure = { Result.failure(it) }
        )
    }

    override fun logout() = sessionStore.clear()
}

/*
 * File: ServiceLocator.kt
 * Purpose: Manual dependency injection - builds the SQLite helper, Retrofit client and
 *          repositories once, and exposes them to ViewModels through their interfaces.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.di

import android.content.Context
import com.solargrid.app.BuildConfig
import com.solargrid.app.data.local.SessionStore
import com.solargrid.app.data.local.SolarGridDbHelper
import com.solargrid.app.data.remote.ApiClient
import com.solargrid.app.data.repository.AuthRepositoryImpl
import com.solargrid.app.data.repository.NodeRepositoryImpl
import com.solargrid.app.data.repository.ProsumerRepositoryImpl
import com.solargrid.app.data.repository.ReservationRepositoryImpl
import com.solargrid.app.domain.repository.AuthRepository
import com.solargrid.app.domain.repository.NodeRepository
import com.solargrid.app.domain.repository.ProsumerRepository
import com.solargrid.app.domain.repository.ReservationRepository

object ServiceLocator {

    lateinit var authRepository: AuthRepository
        private set
    lateinit var prosumerRepository: ProsumerRepository
        private set
    lateinit var nodeRepository: NodeRepository
        private set
    lateinit var reservationRepository: ReservationRepository
        private set

    fun init(context: Context) {
        val db = SolarGridDbHelper(context.applicationContext)
        val sessionStore = SessionStore(db)
        val api = ApiClient.create(BuildConfig.API_BASE_URL, sessionStore::token, BuildConfig.DEBUG)

        authRepository = AuthRepositoryImpl(api, sessionStore)
        prosumerRepository = ProsumerRepositoryImpl(api)
        nodeRepository = NodeRepositoryImpl(api, db)
        reservationRepository = ReservationRepositoryImpl(api)
    }
}

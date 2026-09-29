/*
 * File: SessionStore.kt
 * Purpose: In-memory view of the SQLite session row, shared by the auth repository and the
 *          HTTP interceptor. Expired sessions are discarded on read.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.data.local

import com.solargrid.app.domain.model.Session

class SessionStore(private val db: SolarGridDbHelper) {

    @Volatile
    private var cached: Session? = null

    @Volatile
    private var loaded = false

    @Synchronized
    fun get(): Session? {
        if (!loaded) {
            cached = db.loadSession()
            loaded = true
        }
        val session = cached
        if (session != null && session.isExpired) {
            clear()
            return null
        }
        return session
    }

    @Synchronized
    fun save(session: Session) {
        db.saveSession(session)
        cached = session
        loaded = true
    }

    @Synchronized
    fun clear() {
        db.clearSession()
        cached = null
        loaded = true
    }

    fun token(): String? = get()?.token
}

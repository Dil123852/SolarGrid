/*
 * File: SolarGridDbHelper.kt
 * Purpose: SQLite local persistence. "session" holds the signed-in user (one row) so the app
 *          reopens without logging in again; "node_cache" keeps the last node list so the map
 *          still shows hubs when the API is unreachable.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.data.local

import android.content.ContentValues
import android.content.Context
import android.database.sqlite.SQLiteDatabase
import android.database.sqlite.SQLiteOpenHelper
import com.solargrid.app.domain.model.MicrogridNode
import com.solargrid.app.domain.model.Role
import com.solargrid.app.domain.model.Session
import java.time.Instant

class SolarGridDbHelper(context: Context) :
    SQLiteOpenHelper(context, DATABASE_NAME, null, DATABASE_VERSION) {

    override fun onCreate(db: SQLiteDatabase) {
        db.execSQL(
            """
            CREATE TABLE $TABLE_SESSION (
                id INTEGER PRIMARY KEY CHECK (id = 1),
                token TEXT NOT NULL,
                role TEXT NOT NULL,
                display_name TEXT NOT NULL,
                nic TEXT,
                expires_at TEXT NOT NULL
            )
            """.trimIndent()
        )
        db.execSQL(
            """
            CREATE TABLE $TABLE_NODES (
                id TEXT PRIMARY KEY,
                name TEXT NOT NULL,
                latitude REAL NOT NULL,
                longitude REAL NOT NULL,
                capacity_kwh REAL NOT NULL,
                battery_slots INTEGER NOT NULL,
                is_active INTEGER NOT NULL,
                open_time TEXT,
                close_time TEXT
            )
            """.trimIndent()
        )
    }

    override fun onUpgrade(db: SQLiteDatabase, oldVersion: Int, newVersion: Int) {
        db.execSQL("DROP TABLE IF EXISTS $TABLE_SESSION")
        db.execSQL("DROP TABLE IF EXISTS $TABLE_NODES")
        onCreate(db)
    }

    // ---- Session ----

    fun saveSession(session: Session) {
        val values = ContentValues().apply {
            put("id", 1)
            put("token", session.token)
            put("role", session.role.name)
            put("display_name", session.displayName)
            put("nic", session.nic)
            put("expires_at", session.expiresAt.toString())
        }
        writableDatabase.insertWithOnConflict(TABLE_SESSION, null, values, SQLiteDatabase.CONFLICT_REPLACE)
    }

    fun loadSession(): Session? {
        readableDatabase.query(TABLE_SESSION, null, "id = 1", null, null, null, null).use { c ->
            if (!c.moveToFirst()) return null
            val role = runCatching { Role.valueOf(c.getString(c.getColumnIndexOrThrow("role"))) }.getOrNull()
                ?: return null
            return Session(
                token = c.getString(c.getColumnIndexOrThrow("token")),
                role = role,
                displayName = c.getString(c.getColumnIndexOrThrow("display_name")),
                nic = c.getString(c.getColumnIndexOrThrow("nic")),
                expiresAt = runCatching { Instant.parse(c.getString(c.getColumnIndexOrThrow("expires_at"))) }
                    .getOrDefault(Instant.EPOCH)
            )
        }
    }

    fun clearSession() {
        writableDatabase.delete(TABLE_SESSION, null, null)
    }

    // ---- Node cache ----

    fun replaceNodes(nodes: List<MicrogridNode>) {
        val db = writableDatabase
        db.beginTransaction()
        try {
            db.delete(TABLE_NODES, null, null)
            nodes.forEach { n ->
                db.insert(TABLE_NODES, null, ContentValues().apply {
                    put("id", n.id)
                    put("name", n.name)
                    put("latitude", n.latitude)
                    put("longitude", n.longitude)
                    put("capacity_kwh", n.capacityKWh)
                    put("battery_slots", n.batterySlots)
                    put("is_active", if (n.isActive) 1 else 0)
                    put("open_time", n.openTime)
                    put("close_time", n.closeTime)
                })
            }
            db.setTransactionSuccessful()
        } finally {
            db.endTransaction()
        }
    }

    fun loadNodes(): List<MicrogridNode> =
        readableDatabase.query(TABLE_NODES, null, null, null, null, null, "name").use { c ->
            val nodes = mutableListOf<MicrogridNode>()
            while (c.moveToNext()) {
                nodes += MicrogridNode(
                    id = c.getString(c.getColumnIndexOrThrow("id")),
                    name = c.getString(c.getColumnIndexOrThrow("name")),
                    latitude = c.getDouble(c.getColumnIndexOrThrow("latitude")),
                    longitude = c.getDouble(c.getColumnIndexOrThrow("longitude")),
                    capacityKWh = c.getDouble(c.getColumnIndexOrThrow("capacity_kwh")),
                    batterySlots = c.getInt(c.getColumnIndexOrThrow("battery_slots")),
                    isActive = c.getInt(c.getColumnIndexOrThrow("is_active")) == 1,
                    openTime = c.getString(c.getColumnIndexOrThrow("open_time")),
                    closeTime = c.getString(c.getColumnIndexOrThrow("close_time"))
                )
            }
            nodes
        }

    companion object {
        private const val DATABASE_NAME = "solargrid.db"
        // v2 added node opening hours to node_cache.
        private const val DATABASE_VERSION = 2
        private const val TABLE_SESSION = "session"
        private const val TABLE_NODES = "node_cache"
    }
}

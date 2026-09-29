/*
 * File: SolarGridApp.kt
 * Purpose: Application entry point - initialises the dependency graph once per process.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app

import android.app.Application
import com.solargrid.app.di.ServiceLocator

class SolarGridApp : Application() {
    override fun onCreate() {
        super.onCreate()
        ServiceLocator.init(this)
    }
}

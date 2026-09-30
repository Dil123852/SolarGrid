/*
 * File: MainActivity.kt
 * Purpose: Single activity hosting the Compose navigation graph.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app

import android.os.Bundle
import android.graphics.Color
import androidx.activity.ComponentActivity
import androidx.activity.SystemBarStyle
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import com.solargrid.app.ui.navigation.SolarGridNavGraph
import com.solargrid.app.ui.theme.SolarGridTheme

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        // Draw behind the system bars. Every screen starts with the dark ink bar, so the status bar
        // uses light icons; the navigation bar follows the light paper background.
        enableEdgeToEdge(
            statusBarStyle = SystemBarStyle.dark(Color.TRANSPARENT),
            navigationBarStyle = SystemBarStyle.light(Color.TRANSPARENT, Color.TRANSPARENT)
        )
        setContent {
            SolarGridTheme {
                SolarGridNavGraph()
            }
        }
    }
}

/*
 * File: MainActivity.kt
 * Purpose: Single activity hosting the Compose navigation graph.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import com.solargrid.app.ui.navigation.SolarGridNavGraph
import com.solargrid.app.ui.theme.SolarGridTheme

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        // Android 15 draws behind system bars; Scaffold screens pad themselves, others use systemBarsPadding().
        enableEdgeToEdge()
        setContent {
            SolarGridTheme {
                SolarGridNavGraph()
            }
        }
    }
}

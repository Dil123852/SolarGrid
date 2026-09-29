/*
 * File: Theme.kt
 * Purpose: Material 3 theme matching the web app palette (solar green + amber accent).
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.theme

import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color

val SolarGreen = Color(0xFF198754)
val SolarGreenDark = Color(0xFF0A3622)
val SolarAmber = Color(0xFFF5A524)
val StatusPending = Color(0xFFF5A524)
val StatusApproved = Color(0xFF198754)
val StatusCancelled = Color(0xFF6C757D)
val StatusCompleted = Color(0xFF0D6EFD)

private val LightColors = lightColorScheme(
    primary = SolarGreen,
    onPrimary = Color.White,
    primaryContainer = Color(0xFFD1E7DD),
    onPrimaryContainer = SolarGreenDark,
    secondary = SolarAmber,
    onSecondary = Color(0xFF3D2A00),
    secondaryContainer = Color(0xFFFFECC2),
    onSecondaryContainer = Color(0xFF3D2A00),
    background = Color(0xFFF4F6F3),
    surface = Color.White,
    error = Color(0xFFDC3545)
)

@Composable
fun SolarGridTheme(content: @Composable () -> Unit) {
    MaterialTheme(colorScheme = LightColors, content = content)
}

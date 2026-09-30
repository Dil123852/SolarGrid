/*
 * File: Theme.kt
 * Purpose: SolarGrid design system for Android - the same language as the web client
 *          (web/src/styles/app.css): ink / paper palette, Space Grotesk headings, DM Sans text,
 *          square corners everywhere and muted status colours.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.theme

import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Shapes
import androidx.compose.material3.Typography
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.text.font.Font
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.em
import androidx.compose.ui.unit.sp
import com.solargrid.app.R

// ---- Palette (matches the web theme tokens) ----
val Ink = Color(0xFF0B0B0B)
val Ink2 = Color(0xFF2A2A2A)
val Paper = Color(0xFFF5F5F3)
val White = Color(0xFFFFFFFF)
val Border = Color(0xFFD4D4D1)
val BorderSoft = Color(0xFFE6E6E3)
val Muted = Color(0xFF6B6B6B)
val GridLine = Color(0x12FFFFFF)
val ErrorRed = Color(0xFFB42318)
val ErrorBg = Color(0xFFFDECEA)
val SuccessGreen = Color(0xFF2F7D4F)

// Status badges: foreground / background pairs, as on the web.
val StatusPending = Color(0xFF7A5200)
val StatusPendingBg = Color(0xFFFFF2D1)
val StatusApproved = Color(0xFF1F5C3A)
val StatusApprovedBg = Color(0xFFDCEFE3)
val StatusCancelled = Color(0xFF555555)
val StatusCancelledBg = Color(0xFFECECEA)
val StatusCompleted = White
val StatusCompletedBg = Ink

// Legacy names still used by a few screens.
val SolarGreen = SuccessGreen
val SolarGreenDark = Ink
val SolarAmber = Color(0xFFC98A00)

// ---- Type ----
val SpaceGrotesk = FontFamily(
    Font(R.font.space_grotesk_medium, FontWeight.Medium),
    Font(R.font.space_grotesk_semibold, FontWeight.SemiBold),
    Font(R.font.space_grotesk_bold, FontWeight.Bold)
)

val DmSans = FontFamily(
    Font(R.font.dm_sans_regular, FontWeight.Normal),
    Font(R.font.dm_sans_medium, FontWeight.Medium),
    Font(R.font.dm_sans_semibold, FontWeight.SemiBold)
)

private fun display(size: Int, weight: FontWeight = FontWeight.SemiBold, spacing: Double = -0.01) =
    TextStyle(fontFamily = SpaceGrotesk, fontWeight = weight, fontSize = size.sp, letterSpacing = spacing.em)

private fun body(size: Int, weight: FontWeight = FontWeight.Normal, line: Int = size + 7) =
    TextStyle(fontFamily = DmSans, fontWeight = weight, fontSize = size.sp, lineHeight = line.sp)

private val SgTypography = Typography(
    displaySmall = display(36, FontWeight.Bold, -0.015),
    headlineLarge = display(32, FontWeight.Bold, -0.015),
    headlineMedium = display(28),
    headlineSmall = display(24),
    titleLarge = display(20),
    titleMedium = display(17),
    titleSmall = display(15, FontWeight.Medium),
    bodyLarge = body(16),
    bodyMedium = body(14),
    bodySmall = body(12, line = 18),
    labelLarge = body(14, FontWeight.SemiBold, 20),
    labelMedium = body(12, FontWeight.Medium, 16),
    labelSmall = body(11, FontWeight.Medium, 14)
)

// Square corners everywhere, like the web theme.
private val Square = RoundedCornerShape(0.dp)
private val SgShapes = Shapes(
    extraSmall = Square,
    small = Square,
    medium = Square,
    large = Square,
    extraLarge = Square
)

private val SgColors = lightColorScheme(
    primary = Ink,
    onPrimary = White,
    primaryContainer = Paper,
    onPrimaryContainer = Ink,
    secondary = Ink2,
    onSecondary = White,
    secondaryContainer = Paper,
    onSecondaryContainer = Ink,
    background = Paper,
    onBackground = Ink,
    surface = White,
    onSurface = Ink,
    surfaceVariant = Paper,
    onSurfaceVariant = Muted,
    surfaceContainer = White,
    surfaceContainerHigh = White,
    surfaceContainerHighest = Paper,
    outline = Border,
    outlineVariant = BorderSoft,
    error = ErrorRed,
    onError = White,
    errorContainer = ErrorBg,
    onErrorContainer = ErrorRed
)

@Composable
fun SolarGridTheme(content: @Composable () -> Unit) {
    MaterialTheme(colorScheme = SgColors, typography = SgTypography, shapes = SgShapes, content = content)
}

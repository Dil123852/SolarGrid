/*
 * File: PendingActivationScreen.kt
 * Purpose: Shown when a deactivated prosumer signs in (or right after self-deactivation):
 *          the account waits for a Backoffice officer to reactivate it.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.auth

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.systemBarsPadding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.HourglassTop
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import com.solargrid.app.ui.common.LoadingButton
import com.solargrid.app.ui.theme.SolarAmber

@Composable
fun PendingActivationScreen(message: String, onBackToLogin: () -> Unit) {
    Column(
        Modifier.fillMaxSize().systemBarsPadding().padding(32.dp),
        verticalArrangement = Arrangement.Center,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Icon(Icons.Filled.HourglassTop, contentDescription = null, tint = SolarAmber, modifier = Modifier.size(64.dp))
        Spacer(Modifier.height(16.dp))
        Text("Account pending activation", style = MaterialTheme.typography.headlineSmall, fontWeight = FontWeight.SemiBold)
        Spacer(Modifier.height(8.dp))
        Text(
            message.ifBlank { "Your account is deactivated. A Backoffice officer must reactivate it before you can sign in." },
            textAlign = TextAlign.Center,
            color = MaterialTheme.colorScheme.onSurfaceVariant
        )
        Spacer(Modifier.height(24.dp))
        LoadingButton(text = "Back to sign in", loading = false, onClick = onBackToLogin)
    }
}

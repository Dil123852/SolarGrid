/*
 * File: PendingActivationScreen.kt
 * Purpose: Shown when a deactivated account signs in (or right after self-deactivation):
 *          the account waits for a Backoffice officer to reactivate it.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.auth

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.solargrid.app.ui.common.CircumIcons
import com.solargrid.app.ui.common.Eyebrow
import com.solargrid.app.ui.common.LoadingButton
import com.solargrid.app.ui.common.SgCard
import com.solargrid.app.ui.common.SgPageHeader
import com.solargrid.app.ui.common.SgTopBar
import com.solargrid.app.ui.theme.Ink
import com.solargrid.app.ui.theme.Muted

@Composable
fun PendingActivationScreen(message: String, onBackToLogin: () -> Unit) {
    Scaffold(topBar = { SgTopBar("SolarGrid") }) { padding ->
        Column(
            Modifier.padding(padding).fillMaxSize().padding(20.dp),
            verticalArrangement = Arrangement.spacedBy(20.dp)
        ) {
            SgPageHeader(eyebrow = "Account status", title = "Pending activation")
            SgCard(padding = 20.dp) {
                Icon(CircumIcons.Timer, contentDescription = null, tint = Ink, modifier = Modifier.size(40.dp))
                Eyebrow("Deactivated", modifier = Modifier.padding(top = 16.dp))
                Text(
                    message.ifBlank { "Your account is deactivated. A Backoffice officer must reactivate it before you can sign in." },
                    style = MaterialTheme.typography.bodyLarge,
                    modifier = Modifier.padding(top = 6.dp)
                )
                Text(
                    "Once it has been reactivated you can sign in with your NIC as usual.",
                    style = MaterialTheme.typography.bodyMedium,
                    color = Muted,
                    modifier = Modifier.padding(top = 10.dp)
                )
            }
            LoadingButton(text = "Back to sign in", loading = false, onClick = onBackToLogin)
        }
    }
}

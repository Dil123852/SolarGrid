/*
 * File: BookingSummaryScreen.kt
 * Purpose: Confirmation shown after every create / update / cancel - e.g.
 *          "Booking confirmed for 30 Sep 2026, 09:00 at Kandy Hub".
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.booking

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.solargrid.app.di.ServiceLocator
import com.solargrid.app.ui.common.CircumIcons
import com.solargrid.app.ui.common.CenteredLoading
import com.solargrid.app.ui.common.ErrorText
import com.solargrid.app.ui.common.LabelValue
import com.solargrid.app.ui.common.LoadingButton
import com.solargrid.app.ui.common.SgCard
import com.solargrid.app.ui.common.SgTopBar
import com.solargrid.app.ui.common.StatusChip
import com.solargrid.app.ui.common.factoryOf
import com.solargrid.app.ui.common.formatLocal
import com.solargrid.app.ui.navigation.SummaryAction
import com.solargrid.app.ui.theme.SolarGreen
import com.solargrid.app.ui.theme.StatusCancelled

@Composable
fun BookingSummaryScreen(
    action: SummaryAction,
    reservationId: String,
    onHome: () -> Unit,
    onViewBookings: () -> Unit,
    vm: BookingSummaryViewModel = viewModel(factory = factoryOf {
        BookingSummaryViewModel(reservationId, ServiceLocator.reservationRepository)
    })
) {
    val (title, icon, tint) = when (action) {
        SummaryAction.Created -> Triple("Booking confirmed", CircumIcons.Check, SolarGreen)
        SummaryAction.Updated -> Triple("Booking updated", CircumIcons.Repeat, SolarGreen)
        SummaryAction.Cancelled -> Triple("Booking cancelled", CircumIcons.Remove, StatusCancelled)
    }

    Scaffold(topBar = { SgTopBar("Summary") }) { padding ->
        Column(
            Modifier.padding(padding).fillMaxSize().padding(24.dp),
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            Icon(icon, contentDescription = null, tint = tint, modifier = Modifier.size(72.dp))
            Text(title, style = MaterialTheme.typography.headlineSmall, fontWeight = FontWeight.SemiBold)

            val r = vm.reservation
            when {
                r != null -> {
                    val verb = when (action) {
                        SummaryAction.Created -> "Your slot is reserved"
                        SummaryAction.Updated -> "Your slot has been moved"
                        SummaryAction.Cancelled -> "Your slot has been released"
                    }
                    Text(
                        "$verb for ${r.slotTime.formatLocal()} at ${r.nodeName}.",
                        textAlign = TextAlign.Center
                    )
                    SgCard {
                        LabelValue("Node", r.nodeName)
                        LabelValue("Slot", r.slotTime.formatLocal())
                        LabelValue("Reference", r.id.takeLast(8).uppercase())
                        Column(Modifier.fillMaxWidth(), horizontalAlignment = Alignment.End) { StatusChip(r.status) }
                    }
                    if (action != SummaryAction.Cancelled) {
                        Text(
                            "A Backoffice officer will approve it; the QR code appears in the booking once approved.",
                            style = MaterialTheme.typography.bodySmall,
                            textAlign = TextAlign.Center,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                    }
                }
                vm.error != null -> ErrorText(vm.error)
                else -> CenteredLoading()
            }

            LoadingButton(text = "Back to dashboard", loading = false, onClick = onHome)
            OutlinedButton(onClick = onViewBookings, modifier = Modifier.fillMaxWidth()) { Text("View my bookings") }
        }
    }
}

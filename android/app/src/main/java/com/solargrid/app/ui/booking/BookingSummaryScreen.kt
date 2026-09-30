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
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.solargrid.app.di.ServiceLocator
import com.solargrid.app.ui.common.CircumIcons
import com.solargrid.app.ui.common.CenteredLoading
import com.solargrid.app.ui.common.ErrorText
import com.solargrid.app.ui.common.Eyebrow
import com.solargrid.app.ui.common.LabelValue
import com.solargrid.app.ui.common.LoadingButton
import com.solargrid.app.ui.common.SgCard
import com.solargrid.app.ui.common.SgOutlinedButton
import com.solargrid.app.ui.common.SgPageHeader
import com.solargrid.app.ui.common.SgTopBar
import com.solargrid.app.ui.common.StatusChip
import com.solargrid.app.ui.common.factoryOf
import com.solargrid.app.ui.common.formatLocal
import com.solargrid.app.ui.navigation.SummaryAction
import com.solargrid.app.ui.theme.BorderSoft
import com.solargrid.app.ui.theme.Ink
import com.solargrid.app.ui.theme.Muted

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
    val (title, icon) = when (action) {
        SummaryAction.Created -> "Booking confirmed" to CircumIcons.Check
        SummaryAction.Updated -> "Booking updated" to CircumIcons.Repeat
        SummaryAction.Cancelled -> "Booking cancelled" to CircumIcons.Remove
    }

    Scaffold(topBar = { SgTopBar("Summary") }) { padding ->
        Column(
            Modifier.padding(padding).fillMaxSize().padding(20.dp),
            verticalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            Icon(icon, contentDescription = null, tint = Ink, modifier = Modifier.size(56.dp))
            SgPageHeader(eyebrow = "Summary", title = title)

            val r = vm.reservation
            when {
                r != null -> {
                    val verb = when (action) {
                        SummaryAction.Created -> "Your slot is reserved"
                        SummaryAction.Updated -> "Your slot has been moved"
                        SummaryAction.Cancelled -> "Your slot has been released"
                    }
                    Text("$verb for ${r.slotTime.formatLocal()} at ${r.nodeName}.", style = MaterialTheme.typography.bodyLarge)
                    SgCard {
                        Row(verticalAlignment = Alignment.CenterVertically) {
                            Eyebrow("Status", modifier = Modifier.weight(1f))
                            StatusChip(r.status)
                        }
                        HorizontalDivider(Modifier.padding(vertical = 10.dp), color = BorderSoft)
                        LabelValue("Node", r.nodeName)
                        LabelValue("Slot", r.slotTime.formatLocal())
                        LabelValue("Reference", r.id.takeLast(8).uppercase())
                    }
                    if (action != SummaryAction.Cancelled) {
                        Text(
                            "A Backoffice officer will approve it; the QR code appears in the booking once approved.",
                            style = MaterialTheme.typography.bodySmall,
                            color = Muted
                        )
                    }
                }
                vm.error != null -> ErrorText(vm.error)
                else -> CenteredLoading()
            }

            LoadingButton(text = "Back to dashboard", loading = false, onClick = onHome)
            SgOutlinedButton(text = "View my bookings", onClick = onViewBookings, icon = CircumIcons.List)
        }
    }
}

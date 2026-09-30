/*
 * File: BookingDetailScreen.kt
 * Purpose: Booking details with reschedule/cancel actions and the transaction QR code
 *          the prosumer shows to the Grid Operator once the booking is approved.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.booking

import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.asImageBitmap
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.solargrid.app.di.ServiceLocator
import com.solargrid.app.ui.common.CenteredLoading
import com.solargrid.app.ui.common.CircumIcons
import com.solargrid.app.ui.common.Eyebrow
import com.solargrid.app.ui.common.SgConfirmDialog
import com.solargrid.app.ui.common.SgOutlinedButton
import com.solargrid.app.ui.common.SgPageHeader
import com.solargrid.app.ui.common.ErrorText
import com.solargrid.app.ui.common.LabelValue
import com.solargrid.app.ui.common.LoadingButton
import com.solargrid.app.ui.common.SgCard
import com.solargrid.app.ui.common.SgTopBar
import com.solargrid.app.ui.common.StatusChip
import com.solargrid.app.ui.common.factoryOf
import com.solargrid.app.ui.common.formatLocal
import com.solargrid.app.ui.theme.BorderSoft
import com.solargrid.app.ui.theme.ErrorRed
import com.solargrid.app.ui.theme.Muted

@Composable
fun BookingDetailScreen(
    reservationId: String,
    onEdit: () -> Unit,
    onCancelled: () -> Unit,
    onBack: () -> Unit,
    vm: BookingDetailViewModel = viewModel(factory = factoryOf {
        BookingDetailViewModel(reservationId, ServiceLocator.reservationRepository)
    })
) {
    LaunchedEffect(Unit) { vm.load() }
    LaunchedEffect(vm.cancelled) { if (vm.cancelled) onCancelled() }
    var confirmCancel by remember { mutableStateOf(false) }

    Scaffold(topBar = { SgTopBar("Booking details", onBack = onBack) }) { padding ->
        val r = vm.reservation
        if (r == null) {
            Column(Modifier.padding(padding).padding(20.dp)) {
                if (vm.loading) CenteredLoading() else ErrorText(vm.error ?: "Booking not found.")
            }
            return@Scaffold
        }

        Column(
            Modifier.padding(padding).fillMaxSize().verticalScroll(rememberScrollState()).padding(20.dp),
            verticalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            SgPageHeader(eyebrow = "Booking", title = r.nodeName)
            SgCard {
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Eyebrow("Status", modifier = Modifier.weight(1f))
                    StatusChip(r.status)
                }
                HorizontalDivider(Modifier.padding(vertical = 10.dp), color = BorderSoft)
                LabelValue("Slot", r.slotTime.formatLocal())
                LabelValue("Booked on", r.createdAt.formatLocal())
                LabelValue("Reference", r.id.takeLast(8).uppercase())
            }

            val qr = vm.qrBitmap
            if (qr != null) {
                SgCard {
                    Eyebrow("Transaction QR")
                    Text(
                        "Show this to the Grid Operator at the node to finalise the energy transfer.",
                        style = MaterialTheme.typography.bodyMedium,
                        color = Muted,
                        modifier = Modifier.padding(top = 6.dp)
                    )
                    Column(Modifier.fillMaxWidth().padding(top = 16.dp), horizontalAlignment = Alignment.CenterHorizontally) {
                        Image(qr.asImageBitmap(), contentDescription = "Booking QR code", modifier = Modifier.size(240.dp))
                    }
                }
            }

            ErrorText(vm.error)

            if (r.isLive && r.isUpcoming) {
                LoadingButton(text = "Reschedule", loading = false, onClick = onEdit, icon = CircumIcons.Repeat)
                SgOutlinedButton(
                    text = if (vm.cancelling) "Cancelling…" else "Cancel booking",
                    onClick = { confirmCancel = true },
                    enabled = !vm.cancelling,
                    contentColor = ErrorRed,
                    icon = CircumIcons.Remove
                )
                Text(
                    "Changes and cancellations need at least 12 hours' notice.",
                    style = MaterialTheme.typography.bodySmall,
                    color = Muted
                )
            }
        }

        if (confirmCancel) {
            SgConfirmDialog(
                title = "Cancel booking?",
                text = "Cancel your slot at ${r.nodeName} on ${r.slotTime.formatLocal()}?",
                confirmLabel = "Cancel booking",
                dismissLabel = "Keep it",
                destructive = true,
                onConfirm = {
                    confirmCancel = false
                    vm.cancel()
                },
                onDismiss = { confirmCancel = false }
            )
        }
    }
}

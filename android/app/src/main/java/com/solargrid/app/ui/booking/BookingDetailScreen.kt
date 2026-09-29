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
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.asImageBitmap
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.solargrid.app.di.ServiceLocator
import com.solargrid.app.ui.common.CenteredLoading
import com.solargrid.app.ui.common.ErrorText
import com.solargrid.app.ui.common.LabelValue
import com.solargrid.app.ui.common.LoadingButton
import com.solargrid.app.ui.common.SgCard
import com.solargrid.app.ui.common.SgTopBar
import com.solargrid.app.ui.common.StatusChip
import com.solargrid.app.ui.common.factoryOf
import com.solargrid.app.ui.common.formatLocal

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
            Column(Modifier.padding(padding).padding(16.dp)) {
                if (vm.loading) CenteredLoading() else ErrorText(vm.error ?: "Booking not found.")
            }
            return@Scaffold
        }

        Column(
            Modifier.padding(padding).fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            SgCard {
                Text(r.nodeName, style = MaterialTheme.typography.titleLarge, fontWeight = FontWeight.SemiBold)
                LabelValue("Slot", r.slotTime.formatLocal())
                LabelValue("Booked on", r.createdAt.formatLocal())
                Column(Modifier.fillMaxWidth(), horizontalAlignment = Alignment.End) { StatusChip(r.status) }
            }

            val qr = vm.qrBitmap
            if (qr != null) {
                SgCard {
                    Text("Transaction QR", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.SemiBold)
                    Text(
                        "Show this to the Grid Operator at the node to finalise the energy transfer.",
                        style = MaterialTheme.typography.bodySmall
                    )
                    Column(Modifier.fillMaxWidth().padding(top = 12.dp), horizontalAlignment = Alignment.CenterHorizontally) {
                        Image(qr.asImageBitmap(), contentDescription = "Booking QR code", modifier = Modifier.size(240.dp))
                    }
                }
            }

            ErrorText(vm.error)

            if (r.isLive && r.isUpcoming) {
                LoadingButton(text = "Reschedule", loading = false, onClick = onEdit)
                OutlinedButton(
                    onClick = { confirmCancel = true },
                    enabled = !vm.cancelling,
                    colors = ButtonDefaults.outlinedButtonColors(contentColor = MaterialTheme.colorScheme.error),
                    modifier = Modifier.fillMaxWidth()
                ) { Text(if (vm.cancelling) "Cancelling…" else "Cancel booking") }
                Text(
                    "Changes and cancellations need at least 12 hours' notice.",
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
            }
        }

        if (confirmCancel) {
            AlertDialog(
                onDismissRequest = { confirmCancel = false },
                title = { Text("Cancel booking?") },
                text = { Text("Cancel your slot at ${r.nodeName} on ${r.slotTime.formatLocal()}?") },
                confirmButton = {
                    TextButton(onClick = {
                        confirmCancel = false
                        vm.cancel()
                    }) { Text("Cancel booking") }
                },
                dismissButton = { TextButton(onClick = { confirmCancel = false }) { Text("Keep it") } }
            )
        }
    }
}

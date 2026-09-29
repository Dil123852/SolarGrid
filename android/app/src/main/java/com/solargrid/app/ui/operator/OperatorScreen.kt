/*
 * File: OperatorScreen.kt
 * Purpose: Grid Operator Mode - scan a prosumer's booking QR with ZXing, verify it with the API
 *          and show whether the energy transfer was finalised. Manual entry is a fallback for
 *          devices/emulators without a usable camera.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.operator

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.Logout
import androidx.compose.material.icons.filled.CheckCircle
import androidx.compose.material.icons.filled.Error
import androidx.compose.material.icons.filled.Map
import androidx.compose.material.icons.filled.QrCodeScanner
import androidx.compose.material3.Button
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.LinearProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.journeyapps.barcodescanner.ScanContract
import com.journeyapps.barcodescanner.ScanOptions
import com.solargrid.app.di.ServiceLocator
import com.solargrid.app.ui.common.LabelValue
import com.solargrid.app.ui.common.SgCard
import com.solargrid.app.ui.common.SgTopBar
import com.solargrid.app.ui.common.factoryOf
import com.solargrid.app.ui.common.formatLocal
import com.solargrid.app.ui.theme.SolarGreen

@Composable
fun OperatorScreen(
    onMap: () -> Unit,
    onLogout: () -> Unit,
    vm: OperatorViewModel = viewModel(factory = factoryOf {
        OperatorViewModel(ServiceLocator.authRepository, ServiceLocator.reservationRepository)
    })
) {
    // ZXing returns null contents when the scan is cancelled.
    val scanLauncher = rememberLauncherForActivityResult(ScanContract()) { scan ->
        scan.contents?.let(vm::verify)
    }

    Scaffold(topBar = {
        SgTopBar("Operator Mode", actions = {
            IconButton(onClick = onMap) { Icon(Icons.Filled.Map, contentDescription = "Node map") }
            IconButton(onClick = onLogout) { Icon(Icons.AutoMirrored.Filled.Logout, contentDescription = "Sign out") }
        })
    }) { padding ->
        Column(
            Modifier.padding(padding).fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            Text("Signed in as ${vm.operatorName}", style = MaterialTheme.typography.titleMedium)

            SgCard {
                Column(Modifier.fillMaxWidth(), horizontalAlignment = Alignment.CenterHorizontally) {
                    Icon(Icons.Filled.QrCodeScanner, contentDescription = null, tint = SolarGreen, modifier = Modifier.size(72.dp))
                    Text("Scan the prosumer's booking QR to finalise the energy transfer.", modifier = Modifier.padding(vertical = 8.dp))
                    Button(
                        onClick = {
                            scanLauncher.launch(
                                ScanOptions()
                                    .setDesiredBarcodeFormats(ScanOptions.QR_CODE)
                                    .setPrompt("Align the booking QR inside the frame")
                                    .setBeepEnabled(true)
                                    .setOrientationLocked(false)
                            )
                        },
                        enabled = !vm.verifying,
                        modifier = Modifier.fillMaxWidth()
                    ) { Text("Scan QR code") }
                }
            }

            if (vm.verifying) LinearProgressIndicator(Modifier.fillMaxWidth())

            when (val r = vm.result) {
                is VerifyResult.Success -> ResultCard(success = true, title = "Transfer finalised") {
                    LabelValue("Node", r.reservation.nodeName)
                    LabelValue("Slot", r.reservation.slotTime.formatLocal())
                    LabelValue("Prosumer NIC", r.reservation.prosumerNic)
                    LabelValue("Status", r.reservation.status.name)
                }
                is VerifyResult.Failure -> ResultCard(success = false, title = "Verification failed") {
                    Text(r.message)
                }
                null -> Unit
            }

            SgCard {
                Text("Enter token manually", style = MaterialTheme.typography.titleSmall)
                Row(Modifier.fillMaxWidth().padding(top = 8.dp), horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    OutlinedTextField(
                        value = vm.manualToken,
                        onValueChange = { vm.manualToken = it },
                        label = { Text("QR token") },
                        singleLine = true,
                        modifier = Modifier.weight(1f)
                    )
                    OutlinedButton(
                        onClick = { vm.verify(vm.manualToken) },
                        enabled = !vm.verifying && vm.manualToken.isNotBlank(),
                        modifier = Modifier.align(Alignment.CenterVertically)
                    ) { Text("Verify") }
                }
            }
        }
    }
}

@Composable
private fun ResultCard(success: Boolean, title: String, content: @Composable () -> Unit) {
    val color = if (success) SolarGreen else MaterialTheme.colorScheme.error
    Card(colors = CardDefaults.cardColors(containerColor = color.copy(alpha = 0.1f)), modifier = Modifier.fillMaxWidth()) {
        Column(Modifier.padding(16.dp)) {
            Row(verticalAlignment = Alignment.CenterVertically) {
                Icon(if (success) Icons.Filled.CheckCircle else Icons.Filled.Error, contentDescription = null, tint = color)
                Text("  $title", color = color, fontWeight = FontWeight.SemiBold, style = MaterialTheme.typography.titleMedium)
            }
            content()
        }
    }
}

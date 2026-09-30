/*
 * File: OperatorScreen.kt
 * Purpose: Grid Operator Mode - scan a prosumer's booking QR with ZXing, verify it with the API
 *          and show whether the energy transfer was finalised. Manual entry is a fallback for
 *          devices/emulators without a usable camera.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.operator

import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.IntrinsicSize
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxHeight
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.LinearProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.journeyapps.barcodescanner.ScanContract
import com.journeyapps.barcodescanner.ScanOptions
import com.solargrid.app.di.ServiceLocator
import com.solargrid.app.ui.common.CircumIcons
import com.solargrid.app.ui.common.Eyebrow
import com.solargrid.app.ui.common.LabelValue
import com.solargrid.app.ui.common.LoadingButton
import com.solargrid.app.ui.common.SgCard
import com.solargrid.app.ui.common.SgOutlinedButton
import com.solargrid.app.ui.common.SgPageHeader
import com.solargrid.app.ui.common.SgTextField
import com.solargrid.app.ui.common.SgTopBar
import com.solargrid.app.ui.common.factoryOf
import com.solargrid.app.ui.common.formatLocal
import com.solargrid.app.ui.theme.Border
import com.solargrid.app.ui.theme.BorderSoft
import com.solargrid.app.ui.theme.ErrorRed
import com.solargrid.app.ui.theme.Ink
import com.solargrid.app.ui.theme.Muted
import com.solargrid.app.ui.theme.SuccessGreen
import com.solargrid.app.ui.theme.White

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
            IconButton(onClick = onMap) { Icon(CircumIcons.Map, contentDescription = "Node map") }
            IconButton(onClick = onLogout) { Icon(CircumIcons.Logout, contentDescription = "Sign out") }
        })
    }) { padding ->
        Column(
            Modifier.padding(padding).fillMaxSize().verticalScroll(rememberScrollState()).padding(20.dp),
            verticalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            SgPageHeader(eyebrow = "Grid operator", title = "Verify a transfer", subtitle = "Signed in as ${vm.operatorName}")

            SgCard(padding = 20.dp) {
                Icon(CircumIcons.Scan, contentDescription = null, tint = Ink, modifier = Modifier.size(48.dp))
                Text(
                    "Scan the prosumer's booking QR to finalise the energy transfer.",
                    style = MaterialTheme.typography.bodyLarge,
                    modifier = Modifier.padding(top = 12.dp, bottom = 16.dp)
                )
                LoadingButton(
                    text = "Scan QR code",
                    loading = vm.verifying,
                    icon = CircumIcons.Scan,
                    onClick = {
                        scanLauncher.launch(
                            ScanOptions()
                                .setDesiredBarcodeFormats(ScanOptions.QR_CODE)
                                .setPrompt("Align the booking QR inside the frame")
                                .setBeepEnabled(true)
                                .setOrientationLocked(false)
                        )
                    }
                )
            }

            if (vm.verifying) LinearProgressIndicator(Modifier.fillMaxWidth().height(2.dp), color = Ink, trackColor = BorderSoft)

            when (val r = vm.result) {
                is VerifyResult.Success -> ResultCard(success = true, title = "Transfer finalised") {
                    LabelValue("Node", r.reservation.nodeName)
                    LabelValue("Slot", r.reservation.slotTime.formatLocal())
                    LabelValue("Prosumer NIC", r.reservation.prosumerNic)
                    LabelValue("Status", r.reservation.status.name)
                }
                is VerifyResult.Failure -> ResultCard(success = false, title = "Verification failed") {
                    Text(r.message, style = MaterialTheme.typography.bodyMedium)
                }
                null -> Unit
            }

            SgCard {
                Eyebrow("No camera? Enter the token")
                Text(
                    "The token is printed under the QR code in the web portal.",
                    style = MaterialTheme.typography.bodySmall,
                    color = Muted,
                    modifier = Modifier.padding(top = 4.dp, bottom = 12.dp)
                )
                SgTextField("QR token", vm.manualToken, { vm.manualToken = it }, imeAction = ImeAction.Done)
                Spacer(Modifier.height(12.dp))
                SgOutlinedButton(
                    text = "Verify token",
                    onClick = { vm.verify(vm.manualToken) },
                    enabled = !vm.verifying && vm.manualToken.isNotBlank(),
                    icon = CircumIcons.Check
                )
            }
        }
    }
}

// Result panel: white with a coloured left edge, like the web notices.
@Composable
private fun ResultCard(success: Boolean, title: String, content: @Composable () -> Unit) {
    val color: Color = if (success) SuccessGreen else ErrorRed
    Row(Modifier.fillMaxWidth().height(IntrinsicSize.Min).background(White).border(1.dp, Border)) {
        Box(Modifier.width(3.dp).fillMaxHeight().background(color))
        Column(Modifier.padding(16.dp)) {
            Row(verticalAlignment = Alignment.CenterVertically) {
                Icon(if (success) CircumIcons.Check else CircumIcons.Alert, contentDescription = null, tint = color)
                Spacer(Modifier.width(8.dp))
                Text(title, color = color, style = MaterialTheme.typography.titleMedium)
            }
            Spacer(Modifier.height(8.dp))
            content()
        }
    }
}

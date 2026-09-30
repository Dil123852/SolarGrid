/*
 * File: BookingFormScreen.kt
 * Purpose: Create or reschedule an energy slot booking - node, date (next 7 days) and time.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.booking

import android.app.DatePickerDialog
import android.app.TimePickerDialog
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.solargrid.app.di.ServiceLocator
import com.solargrid.app.domain.model.MicrogridNode
import com.solargrid.app.ui.common.CircumIcons
import com.solargrid.app.ui.common.CenteredLoading
import com.solargrid.app.ui.common.Eyebrow
import com.solargrid.app.ui.common.SgOutlinedButton
import com.solargrid.app.ui.common.SgPageHeader
import com.solargrid.app.ui.common.ErrorText
import com.solargrid.app.ui.common.LoadingButton
import com.solargrid.app.ui.common.SgCard
import com.solargrid.app.ui.common.SgTopBar
import com.solargrid.app.ui.common.factoryOf
import java.time.LocalDate
import java.time.LocalTime
import java.time.ZoneId
import java.time.format.DateTimeFormatter
import java.time.format.FormatStyle

@Composable
fun BookingFormScreen(
    reservationId: String?,
    onSaved: (String) -> Unit,
    onBack: () -> Unit,
    vm: BookingFormViewModel = viewModel(factory = factoryOf {
        BookingFormViewModel(reservationId, ServiceLocator.reservationRepository, ServiceLocator.nodeRepository)
    })
) {
    LaunchedEffect(vm.savedId) { vm.savedId?.let(onSaved) }
    val context = LocalContext.current

    Scaffold(topBar = { SgTopBar(if (vm.isEdit) "Reschedule booking" else "New booking", onBack = onBack) }) { padding ->
        if (vm.loading) {
            Column(Modifier.padding(padding)) { CenteredLoading() }
            return@Scaffold
        }
        Column(
            Modifier.padding(padding).fillMaxSize().verticalScroll(rememberScrollState()).padding(20.dp),
            verticalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            SgPageHeader(
                eyebrow = if (vm.isEdit) "Reschedule" else "New booking",
                title = if (vm.isEdit) "Change your slot" else "Reserve an energy slot",
                subtitle = "Choose a microgrid node and a time within the next 7 days."
            )
            SgCard {
                SimpleDropdown(
                    label = "Microgrid node",
                    options = vm.nodeOptions,
                    selected = vm.selectedNode ?: vm.nodeOptions.firstOrNull() ?: placeholderNode,
                    optionLabel = { if (it === placeholderNode) "No active nodes" else "${it.name} · ${it.batterySlots} slots" },
                    onSelected = { vm.selectedNode = it },
                    modifier = Modifier.fillMaxWidth()
                )
            }

            SgCard {
                Eyebrow("Slot date & time", modifier = Modifier.padding(bottom = 6.dp))
                Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.spacedBy(10.dp)) {
                    SgOutlinedButton(
                        text = vm.date.format(DateTimeFormatter.ofLocalizedDate(FormatStyle.MEDIUM)),
                        icon = CircumIcons.Calendar,
                        onClick = {
                            DatePickerDialog(
                                context,
                                { _, y, m, d -> vm.date = LocalDate.of(y, m + 1, d) },
                                vm.date.year, vm.date.monthValue - 1, vm.date.dayOfMonth
                            ).apply {
                                val today = LocalDate.now().atStartOfDay(ZoneId.systemDefault()).toInstant().toEpochMilli()
                                datePicker.minDate = today
                                datePicker.maxDate = today + 7L * 24 * 60 * 60 * 1000
                            }.show()
                        },
                        modifier = Modifier.weight(1f)
                    )
                    SgOutlinedButton(
                        text = vm.time.format(DateTimeFormatter.ofPattern("HH:mm")),
                        icon = CircumIcons.Clock,
                        onClick = {
                            TimePickerDialog(
                                context,
                                { _, h, min -> vm.time = LocalTime.of(h, min) },
                                vm.time.hour, vm.time.minute, true
                            ).show()
                        },
                        modifier = Modifier.weight(1f)
                    )
                }
                Text(
                    "Bookings can be made up to 7 days ahead. Changes and cancellations need 12 hours' notice.",
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                    modifier = Modifier.padding(top = 12.dp)
                )
            }

            ErrorText(vm.error)
            LoadingButton(
                text = if (vm.isEdit) "Save changes" else "Confirm booking",
                loading = vm.saving,
                onClick = vm::save,
                enabled = vm.nodeOptions.isNotEmpty()
            )
        }
    }
}

private val placeholderNode = MicrogridNode("", "", 0.0, 0.0, 0.0, 0, false)

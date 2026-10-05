/*
 * File: BookingFormScreen.kt
 * Purpose: Create or reschedule an energy slot booking - station, date (next 7 days), then one of the
 *          station's published booking slots, or any time within its hours when it publishes none.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.booking

import android.app.DatePickerDialog
import android.app.TimePickerDialog
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
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
import com.solargrid.app.ui.theme.Muted
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
                subtitle = "Choose a solar station, a day within the next 7 days and one of its booking slots."
            )
            SgCard {
                SimpleDropdown(
                    label = "Solar station",
                    options = vm.nodeOptions,
                    selected = vm.selectedNode ?: vm.nodeOptions.firstOrNull() ?: placeholderNode,
                    optionLabel = { if (it === placeholderNode) "No active nodes" else "${it.name} · ${it.hoursLabel}" },
                    onSelected = { if (it !== placeholderNode) vm.selectNode(it) },
                    modifier = Modifier.fillMaxWidth()
                )
            }

            SgCard {
                Eyebrow("Day", modifier = Modifier.padding(bottom = 6.dp))
                SgOutlinedButton(
                    text = vm.date.format(DateTimeFormatter.ofLocalizedDate(FormatStyle.FULL)),
                    icon = CircumIcons.Calendar,
                    onClick = {
                        DatePickerDialog(
                            context,
                            { _, y, m, d -> vm.selectDate(LocalDate.of(y, m + 1, d)) },
                            vm.date.year, vm.date.monthValue - 1, vm.date.dayOfMonth
                        ).apply {
                            val today = LocalDate.now().atStartOfDay(ZoneId.systemDefault()).toInstant().toEpochMilli()
                            datePicker.minDate = today
                            datePicker.maxDate = today + 7L * 24 * 60 * 60 * 1000
                        }.show()
                    }
                )
            }

            SgCard {
                if (vm.slotsLoading && vm.slots.isEmpty()) {
                    Eyebrow("Booking slots")
                    Text("Loading the station's slots…", style = MaterialTheme.typography.bodySmall, color = Muted, modifier = Modifier.padding(top = 6.dp))
                } else if (vm.slots.isNotEmpty()) {
                    Eyebrow("Booking slots", modifier = Modifier.padding(bottom = 8.dp))
                    // Two equal columns of slot tiles.
                    Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                        vm.slots.chunked(2).forEach { pair ->
                            Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                pair.forEach { slot ->
                                    SlotChip(
                                        slot = slot,
                                        selected = slot.id == vm.selectedSlotId,
                                        enabled = vm.isSelectable(slot),
                                        onClick = { vm.selectSlot(slot) },
                                        modifier = Modifier.weight(1f)
                                    )
                                }
                                if (pair.size == 1) Spacer(Modifier.weight(1f))
                            }
                        }
                    }
                    Text(
                        "This station takes bookings only in its published slots. Each slot holds a limited number of bookings.",
                        style = MaterialTheme.typography.bodySmall,
                        color = Muted,
                        modifier = Modifier.padding(top = 12.dp)
                    )
                } else {
                    Eyebrow("Time", modifier = Modifier.padding(bottom = 6.dp))
                    SgOutlinedButton(
                        text = vm.time.format(DateTimeFormatter.ofPattern("HH:mm")),
                        icon = CircumIcons.Clock,
                        onClick = {
                            TimePickerDialog(
                                context,
                                { _, h, min -> vm.selectTime(LocalTime.of(h, min)) },
                                vm.time.hour, vm.time.minute, true
                            ).show()
                        }
                    )
                    Text(
                        "No published slots this day - choose any time within the station's hours (${vm.selectedNode?.hoursLabel ?: "24 hours"}, Sri Lanka time).",
                        style = MaterialTheme.typography.bodySmall,
                        color = Muted,
                        modifier = Modifier.padding(top = 12.dp)
                    )
                }
            }

            Text(
                "Bookings can be made up to 7 days ahead. Changes and cancellations need 12 hours' notice.",
                style = MaterialTheme.typography.bodySmall,
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )

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

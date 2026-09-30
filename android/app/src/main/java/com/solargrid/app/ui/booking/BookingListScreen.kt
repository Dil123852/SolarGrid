/*
 * File: BookingListScreen.kt
 * Purpose: Current bookings and full booking history with node and status filters.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.booking

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.FloatingActionButton
import androidx.compose.material3.Icon
import androidx.compose.material3.LinearProgressIndicator
import androidx.compose.material3.Scaffold
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.RectangleShape
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.solargrid.app.di.ServiceLocator
import com.solargrid.app.domain.model.MicrogridNode
import com.solargrid.app.domain.model.ReservationStatus
import com.solargrid.app.ui.common.CircumIcons
import com.solargrid.app.ui.common.EmptyState
import com.solargrid.app.ui.common.ErrorText
import com.solargrid.app.ui.common.SgTabRow
import com.solargrid.app.ui.common.SgTopBar
import com.solargrid.app.ui.common.factoryOf
import com.solargrid.app.ui.theme.Border
import com.solargrid.app.ui.theme.BorderSoft
import com.solargrid.app.ui.theme.Ink
import com.solargrid.app.ui.theme.White

@Composable
fun BookingListScreen(
    onBookingClick: (String) -> Unit,
    onNewBooking: () -> Unit,
    onBack: () -> Unit,
    vm: BookingListViewModel = viewModel(factory = factoryOf {
        BookingListViewModel(ServiceLocator.reservationRepository, ServiceLocator.nodeRepository)
    })
) {
    LaunchedEffect(Unit) { vm.load() }

    Scaffold(
        topBar = { SgTopBar("My bookings", onBack = onBack) },
        floatingActionButton = {
            FloatingActionButton(onClick = onNewBooking, shape = RectangleShape, containerColor = Ink, contentColor = White) {
                Icon(CircumIcons.Plus, contentDescription = "New booking")
            }
        }
    ) { padding ->
        Column(Modifier.padding(padding).fillMaxSize()) {
            SgTabRow(
                tabs = listOf("Current", "History"),
                selected = vm.tab.ordinal,
                onSelect = { vm.selectTab(BookingTab.entries[it]) },
                modifier = Modifier.padding(horizontal = 20.dp, vertical = 4.dp)
            )

            Row(Modifier.fillMaxWidth().padding(horizontal = 20.dp, vertical = 12.dp), horizontalArrangement = Arrangement.spacedBy(10.dp)) {
                val nodeOptions: List<MicrogridNode?> = listOf<MicrogridNode?>(null) + vm.nodeOptions
                SimpleDropdown(
                    label = "Node",
                    options = nodeOptions,
                    selected = vm.nodeFilter,
                    optionLabel = { it?.name ?: "All nodes" },
                    onSelected = vm::filterByNode,
                    modifier = Modifier.weight(1f)
                )
                val statusOptions: List<ReservationStatus?> = listOf<ReservationStatus?>(null) + ReservationStatus.entries
                SimpleDropdown(
                    label = "Status",
                    options = statusOptions,
                    selected = vm.statusFilter,
                    optionLabel = { it?.name ?: "Any status" },
                    onSelected = vm::filterByStatus,
                    modifier = Modifier.weight(1f)
                )
            }

            if (vm.loading) LinearProgressIndicator(Modifier.fillMaxWidth().height(2.dp), color = Ink, trackColor = BorderSoft)
            Column(Modifier.padding(horizontal = 20.dp)) { ErrorText(vm.error) }

            val visible = vm.visible
            if (visible.isEmpty() && !vm.loading) {
                EmptyState(if (vm.tab == BookingTab.Current) "No current bookings." else "No bookings match these filters.")
            } else {
                // The list sits in one bordered white panel, like a web table.
                LazyColumn(
                    Modifier
                        .fillMaxSize()
                        .padding(horizontal = 20.dp)
                        .padding(bottom = 16.dp)
                        .background(White)
                        .border(1.dp, Border)
                        .padding(horizontal = 16.dp)
                ) {
                    items(visible, key = { it.id }) { r -> ReservationRow(r, onClick = { onBookingClick(r.id) }) }
                }
            }
        }
    }
}

/*
 * File: HomeScreen.kt
 * Purpose: Prosumer dashboard - live reservation counts, the next upcoming bookings,
 *          and shortcuts to booking, history, the node map and profile.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.dashboard

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.RowScope
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.List
import androidx.compose.material.icons.automirrored.filled.Logout
import androidx.compose.material.icons.filled.AddCircle
import androidx.compose.material.icons.filled.CalendarMonth
import androidx.compose.material.icons.filled.HourglassTop
import androidx.compose.material.icons.filled.Map
import androidx.compose.material.icons.filled.Person
import androidx.compose.material.icons.filled.Refresh
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.LinearProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.solargrid.app.di.ServiceLocator
import com.solargrid.app.ui.booking.ReservationRow
import com.solargrid.app.ui.common.EmptyState
import com.solargrid.app.ui.common.ErrorText
import com.solargrid.app.ui.common.SgCard
import com.solargrid.app.ui.common.SgTopBar
import com.solargrid.app.ui.common.factoryOf
import com.solargrid.app.ui.theme.StatusApproved
import com.solargrid.app.ui.theme.StatusPending

@Composable
fun HomeScreen(
    onNewBooking: () -> Unit,
    onBookings: () -> Unit,
    onBookingClick: (String) -> Unit,
    onMap: () -> Unit,
    onProfile: () -> Unit,
    onLogout: () -> Unit,
    vm: HomeViewModel = viewModel(factory = factoryOf {
        HomeViewModel(ServiceLocator.authRepository, ServiceLocator.reservationRepository)
    })
) {
    // Runs each time the dashboard comes back into view, so counts are always current.
    LaunchedEffect(Unit) { vm.load() }

    Scaffold(topBar = {
        SgTopBar("SolarGrid", actions = {
            IconButton(onClick = vm::load) { Icon(Icons.Filled.Refresh, contentDescription = "Refresh") }
            IconButton(onClick = onLogout) { Icon(Icons.AutoMirrored.Filled.Logout, contentDescription = "Sign out") }
        })
    }) { padding ->
        Column(
            Modifier.padding(padding).fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            if (vm.loading) LinearProgressIndicator(Modifier.fillMaxWidth())
            Text("Hello, ${vm.displayName}", style = MaterialTheme.typography.titleLarge, fontWeight = FontWeight.SemiBold)
            ErrorText(vm.error)

            Row(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                StatCard("Pending", vm.counts?.pending, Icons.Filled.HourglassTop, StatusPending)
                StatCard("Approved upcoming", vm.counts?.approvedFuture, Icons.Filled.CalendarMonth, StatusApproved)
            }

            Row(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                ActionTile("New booking", Icons.Filled.AddCircle, onNewBooking)
                ActionTile("My bookings", Icons.AutoMirrored.Filled.List, onBookings)
            }
            Row(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                ActionTile("Nearby nodes", Icons.Filled.Map, onMap)
                ActionTile("My profile", Icons.Filled.Person, onProfile)
            }

            SgCard {
                Text("Upcoming bookings", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.SemiBold)
                if (vm.upcoming.isEmpty() && !vm.loading) {
                    EmptyState("No upcoming bookings. Tap New booking to reserve an energy slot.")
                }
                vm.upcoming.forEach { r -> ReservationRow(r, onClick = { onBookingClick(r.id) }) }
            }
        }
    }
}

@Composable
private fun RowScope.StatCard(label: String, value: Long?, icon: ImageVector, color: Color) {
    Card(
        modifier = Modifier.weight(1f),
        colors = CardDefaults.cardColors(containerColor = color.copy(alpha = 0.12f))
    ) {
        Column(Modifier.padding(16.dp)) {
            Icon(icon, contentDescription = null, tint = color)
            Text(value?.toString() ?: "–", style = MaterialTheme.typography.headlineLarge, fontWeight = FontWeight.Bold, color = color)
            Text(label, style = MaterialTheme.typography.bodySmall)
        }
    }
}

@Composable
private fun RowScope.ActionTile(label: String, icon: ImageVector, onClick: () -> Unit) {
    Card(
        modifier = Modifier.weight(1f).clickable(onClick = onClick),
        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
        elevation = CardDefaults.cardElevation(defaultElevation = 2.dp)
    ) {
        Column(Modifier.fillMaxWidth().padding(16.dp), horizontalAlignment = Alignment.CenterHorizontally) {
            Icon(icon, contentDescription = null, tint = MaterialTheme.colorScheme.primary, modifier = Modifier.size(32.dp))
            Text(label, fontWeight = FontWeight.Medium)
        }
    }
}

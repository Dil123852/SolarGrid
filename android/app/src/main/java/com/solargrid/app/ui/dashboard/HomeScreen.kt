/*
 * File: HomeScreen.kt
 * Purpose: Prosumer dashboard, styled like the web dashboard - live reservation counts in
 *          bordered stat tiles, square shortcut tiles, and the next upcoming bookings.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.dashboard

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.RowScope
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.LinearProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.solargrid.app.di.ServiceLocator
import com.solargrid.app.ui.booking.ReservationRow
import com.solargrid.app.ui.common.CircumIcons
import com.solargrid.app.ui.common.EmptyState
import com.solargrid.app.ui.common.ErrorText
import com.solargrid.app.ui.common.Eyebrow
import com.solargrid.app.ui.common.SgPageHeader
import com.solargrid.app.ui.common.SgTopBar
import com.solargrid.app.ui.common.factoryOf
import com.solargrid.app.ui.theme.Border
import com.solargrid.app.ui.theme.BorderSoft
import com.solargrid.app.ui.theme.Ink
import com.solargrid.app.ui.theme.SpaceGrotesk
import com.solargrid.app.ui.theme.White

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
            IconButton(onClick = vm::load) { Icon(CircumIcons.Repeat, contentDescription = "Refresh") }
            IconButton(onClick = onLogout) { Icon(CircumIcons.Logout, contentDescription = "Sign out") }
        })
    }) { padding ->
        Column(
            Modifier.padding(padding).fillMaxSize().verticalScroll(rememberScrollState()).padding(20.dp),
            verticalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            if (vm.loading) LinearProgressIndicator(Modifier.fillMaxWidth().height(2.dp), color = Ink, trackColor = BorderSoft)
            SgPageHeader(eyebrow = "Overview", title = "Hello, ${vm.displayName}", subtitle = "Your live reservation overview.")
            ErrorText(vm.error)

            Row(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                StatTile("Pending approval", vm.counts?.pending, CircumIcons.Timer)
                StatTile("Approved upcoming", vm.counts?.approvedFuture, CircumIcons.Calendar)
            }

            Eyebrow("Quick actions", modifier = Modifier.padding(top = 8.dp))
            Row(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                ActionTile("New booking", CircumIcons.Plus, onNewBooking)
                ActionTile("My bookings", CircumIcons.List, onBookings)
            }
            Row(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                ActionTile("Nearby nodes", CircumIcons.Map, onMap)
                ActionTile("My profile", CircumIcons.User, onProfile)
            }

            // Upcoming bookings, as a bordered card with a header strip (like the web dashboard table).
            Column(Modifier.fillMaxWidth().background(White).border(1.dp, Border)) {
                Row(Modifier.fillMaxWidth().padding(horizontal = 16.dp, vertical = 14.dp)) {
                    Eyebrow("Upcoming bookings", modifier = Modifier.weight(1f))
                    Eyebrow("View all", color = Ink, modifier = Modifier.clickable(onClick = onBookings))
                }
                HorizontalDivider(color = BorderSoft)
                Column(Modifier.padding(horizontal = 16.dp)) {
                    if (vm.upcoming.isEmpty() && !vm.loading) {
                        EmptyState("No upcoming bookings. Tap New booking to reserve an energy slot.")
                    }
                    vm.upcoming.forEach { r -> ReservationRow(r, onClick = { onBookingClick(r.id) }) }
                }
            }
        }
    }
}

@Composable
private fun RowScope.StatTile(label: String, value: Long?, icon: ImageVector) {
    Column(
        Modifier.weight(1f).background(White).border(1.dp, Border).padding(16.dp)
    ) {
        Icon(icon, contentDescription = null, tint = Ink, modifier = Modifier.size(30.dp))
        Spacer(Modifier.height(12.dp))
        Text(
            value?.toString() ?: "–",
            fontFamily = SpaceGrotesk,
            fontWeight = FontWeight.SemiBold,
            fontSize = 38.sp,
            lineHeight = 40.sp
        )
        Text(label, style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
    }
}

@Composable
private fun RowScope.ActionTile(label: String, icon: ImageVector, onClick: () -> Unit) {
    Column(
        Modifier
            .weight(1f)
            .background(White)
            .border(1.dp, Border)
            .clickable(onClick = onClick)
            .padding(horizontal = 16.dp, vertical = 18.dp)
    ) {
        Icon(icon, contentDescription = null, tint = Ink, modifier = Modifier.size(28.dp))
        Spacer(Modifier.height(14.dp))
        Text(label, style = MaterialTheme.typography.titleSmall)
    }
}

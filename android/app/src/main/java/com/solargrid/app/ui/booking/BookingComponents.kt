/*
 * File: BookingComponents.kt
 * Purpose: Reusable booking UI pieces (list row, simple dropdown) shared by booking screens and the dashboard.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.booking

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.DropdownMenu
import androidx.compose.material3.DropdownMenuItem
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import com.solargrid.app.domain.model.Reservation
import com.solargrid.app.ui.common.CircumIcons
import com.solargrid.app.ui.common.Eyebrow
import com.solargrid.app.ui.common.StatusChip
import com.solargrid.app.ui.common.formatLocal
import com.solargrid.app.ui.theme.Border
import com.solargrid.app.ui.theme.BorderSoft
import com.solargrid.app.ui.theme.Ink
import com.solargrid.app.ui.theme.Muted
import com.solargrid.app.ui.theme.White

@Composable
fun ReservationRow(reservation: Reservation, onClick: () -> Unit) {
    Column {
        Row(
            Modifier.fillMaxWidth().clickable(onClick = onClick).padding(vertical = 12.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Icon(CircumIcons.Node, contentDescription = null, tint = Ink)
            Column(Modifier.weight(1f).padding(horizontal = 12.dp)) {
                Text(reservation.nodeName, fontWeight = FontWeight.SemiBold, maxLines = 1, overflow = TextOverflow.Ellipsis)
                Text(reservation.slotTime.formatLocal(), style = MaterialTheme.typography.bodySmall)
            }
            StatusChip(reservation.status)
        }
        HorizontalDivider(color = BorderSoft)
    }
}

// A labelled button that opens a menu - avoids the experimental ExposedDropdownMenuBox API.
@Composable
fun <T> SimpleDropdown(
    label: String,
    options: List<T>,
    selected: T,
    optionLabel: (T) -> String,
    onSelected: (T) -> Unit,
    modifier: Modifier = Modifier
) {
    var expanded by remember { mutableStateOf(false) }
    Box(modifier) {
        // Square select box: uppercase label above, current value and a chevron inside.
        Column(Modifier.fillMaxWidth()) {
            Eyebrow(label, modifier = Modifier.padding(bottom = 6.dp))
            Row(
                Modifier
                    .fillMaxWidth()
                    .background(White)
                    .border(1.dp, Border)
                    .clickable { expanded = true }
                    .padding(horizontal = 12.dp, vertical = 12.dp),
                verticalAlignment = Alignment.CenterVertically
            ) {
                Text(
                    optionLabel(selected),
                    modifier = Modifier.weight(1f),
                    style = MaterialTheme.typography.bodyMedium,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis
                )
                Icon(CircumIcons.ChevronDown, contentDescription = null, tint = Muted)
            }
        }
        DropdownMenu(expanded = expanded, onDismissRequest = { expanded = false }, containerColor = White) {
            options.forEach { option ->
                DropdownMenuItem(
                    text = { Text(optionLabel(option)) },
                    onClick = {
                        expanded = false
                        onSelected(option)
                    }
                )
            }
        }
    }
}

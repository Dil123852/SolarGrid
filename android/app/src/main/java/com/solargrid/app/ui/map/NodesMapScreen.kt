/*
 * File: NodesMapScreen.kt
 * Purpose: Google Map with a marker per microgrid node at its stored latitude/longitude.
 *          Tapping a marker shows its name, capacity and battery slots.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.map

import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.key
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.google.android.gms.maps.CameraUpdateFactory
import com.google.android.gms.maps.model.BitmapDescriptorFactory
import com.google.android.gms.maps.model.CameraPosition
import com.google.android.gms.maps.model.LatLng
import com.google.android.gms.maps.model.LatLngBounds
import com.google.maps.android.compose.GoogleMap
import com.google.maps.android.compose.Marker
import com.google.maps.android.compose.MarkerState
import com.google.maps.android.compose.rememberCameraPositionState
import com.solargrid.app.di.ServiceLocator
import com.solargrid.app.ui.common.ErrorText
import com.solargrid.app.ui.common.LabelValue
import com.solargrid.app.ui.common.SgCard
import com.solargrid.app.ui.common.SgTopBar
import com.solargrid.app.ui.common.factoryOf

// Centre of Sri Lanka - used until nodes load.
private val DefaultCenter = LatLng(7.8731, 80.7718)

@Composable
fun NodesMapScreen(
    onBack: () -> Unit,
    vm: NodesMapViewModel = viewModel(factory = factoryOf { NodesMapViewModel(ServiceLocator.nodeRepository) })
) {
    val cameraState = rememberCameraPositionState {
        position = CameraPosition.fromLatLngZoom(DefaultCenter, 7f)
    }
    var mapLoaded by remember { mutableStateOf(false) }

    // Frame all nodes once both the map and the data are ready.
    LaunchedEffect(mapLoaded, vm.items) {
        if (!mapLoaded || vm.items.isEmpty()) return@LaunchedEffect
        val update = if (vm.items.size == 1) {
            CameraUpdateFactory.newLatLngZoom(LatLng(vm.items[0].latitude, vm.items[0].longitude), 12f)
        } else {
            val bounds = LatLngBounds.builder().apply {
                vm.items.forEach { include(LatLng(it.latitude, it.longitude)) }
            }.build()
            CameraUpdateFactory.newLatLngBounds(bounds, 120)
        }
        runCatching { cameraState.animate(update) }
    }

    Scaffold(topBar = { SgTopBar("Microgrid nodes", onBack = onBack) }) { padding ->
        Box(Modifier.padding(padding).fillMaxSize()) {
            GoogleMap(
                modifier = Modifier.fillMaxSize(),
                cameraPositionState = cameraState,
                onMapLoaded = { mapLoaded = true },
                onMapClick = { vm.selected = null }
            ) {
                vm.items.forEach { node ->
                    key(node.id) {
                        val markerState = remember { MarkerState(position = LatLng(node.latitude, node.longitude)) }
                        Marker(
                            state = markerState,
                            title = node.name,
                            snippet = "${node.capacityKWh} kWh · ${node.batterySlots} battery slots",
                            icon = BitmapDescriptorFactory.defaultMarker(
                                if (node.isActive) BitmapDescriptorFactory.HUE_GREEN else BitmapDescriptorFactory.HUE_ORANGE
                            ),
                            onClick = {
                                vm.selected = node
                                false // keep the default info window behaviour
                            }
                        )
                    }
                }
            }

            Box(Modifier.align(Alignment.BottomCenter).padding(16.dp).fillMaxWidth()) {
                val node = vm.selected
                when {
                    node != null -> SgCard {
                        Text(node.name, style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.SemiBold)
                        LabelValue("Capacity", "${node.capacityKWh} kWh")
                        LabelValue("Battery slots", node.batterySlots.toString())
                        LabelValue("Status", if (node.isActive) "Accepting bookings" else "Inactive")
                        LabelValue("Location", "%.5f, %.5f".format(node.latitude, node.longitude))
                    }
                    vm.error != null -> SgCard { ErrorText(vm.error) }
                    vm.items.isNotEmpty() -> SgCard {
                        Text("${vm.items.size} nodes · tap a marker for details", style = MaterialTheme.typography.bodyMedium)
                    }
                }
            }
        }
    }
}

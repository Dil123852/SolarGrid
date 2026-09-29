/*
 * File: NodesMapViewModel.kt
 * Purpose: Loads microgrid nodes (API, or the SQLite cache when offline) for the map.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.map

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.solargrid.app.domain.model.MicrogridNode
import com.solargrid.app.domain.repository.NodeRepository
import kotlinx.coroutines.launch

class NodesMapViewModel(private val nodes: NodeRepository) : ViewModel() {

    var items by mutableStateOf<List<MicrogridNode>>(emptyList())
        private set
    var selected by mutableStateOf<MicrogridNode?>(null)
    var error by mutableStateOf<String?>(null)
        private set

    init {
        viewModelScope.launch {
            nodes.getNodes(activeOnly = false)
                .onSuccess { items = it }
                .onFailure { error = it.message }
        }
    }
}

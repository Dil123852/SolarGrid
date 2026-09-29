/*
 * File: ProfileScreen.kt
 * Purpose: Edit prosumer profile details and deactivate the account (with confirmation).
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.profile

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.imePadding
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.solargrid.app.di.ServiceLocator
import com.solargrid.app.ui.common.CenteredLoading
import com.solargrid.app.ui.common.ErrorText
import com.solargrid.app.ui.common.LoadingButton
import com.solargrid.app.ui.common.SgCard
import com.solargrid.app.ui.common.SgTopBar
import com.solargrid.app.ui.common.factoryOf
import com.solargrid.app.ui.theme.SolarGreen

@Composable
fun ProfileScreen(
    onDeactivated: (String) -> Unit,
    onBack: () -> Unit,
    vm: ProfileViewModel = viewModel(factory = factoryOf {
        ProfileViewModel(ServiceLocator.authRepository, ServiceLocator.prosumerRepository)
    })
) {
    LaunchedEffect(vm.deactivatedMessage) { vm.deactivatedMessage?.let(onDeactivated) }
    var confirmDeactivate by remember { mutableStateOf(false) }

    Scaffold(topBar = { SgTopBar("My profile", onBack = onBack) }) { padding ->
        if (vm.loading) {
            Column(Modifier.padding(padding)) { CenteredLoading() }
            return@Scaffold
        }
        Column(
            Modifier.padding(padding).fillMaxSize().imePadding().verticalScroll(rememberScrollState()).padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(8.dp)
        ) {
            OutlinedTextField(
                value = vm.nic, onValueChange = {}, readOnly = true, enabled = false,
                label = { Text("NIC (account ID)") }, modifier = Modifier.fillMaxWidth()
            )
            OutlinedTextField(vm.name, { vm.name = it }, label = { Text("Full name") }, singleLine = true, modifier = Modifier.fillMaxWidth())
            OutlinedTextField(
                vm.email, { vm.email = it }, label = { Text("Email") }, singleLine = true,
                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Email), modifier = Modifier.fillMaxWidth()
            )
            OutlinedTextField(
                vm.phone, { vm.phone = it }, label = { Text("Phone") }, singleLine = true,
                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Phone), modifier = Modifier.fillMaxWidth()
            )
            OutlinedTextField(vm.address, { vm.address = it }, label = { Text("Address") }, modifier = Modifier.fillMaxWidth())

            ErrorText(vm.error)
            vm.message?.let { Text(it, color = SolarGreen) }
            LoadingButton(text = "Save changes", loading = vm.saving, onClick = vm::save)

            SgCard(Modifier.padding(top = 24.dp)) {
                Text("Deactivate account", style = MaterialTheme.typography.titleMedium)
                Text(
                    "You will be signed out and cannot sign in again until a Backoffice officer reactivates your account.",
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
                OutlinedButton(
                    onClick = { confirmDeactivate = true },
                    enabled = !vm.saving,
                    colors = ButtonDefaults.outlinedButtonColors(contentColor = MaterialTheme.colorScheme.error),
                    modifier = Modifier.fillMaxWidth().padding(top = 8.dp)
                ) { Text("Deactivate my account") }
            }
        }

        if (confirmDeactivate) {
            AlertDialog(
                onDismissRequest = { confirmDeactivate = false },
                title = { Text("Deactivate account?") },
                text = { Text("Only a Backoffice officer can reactivate it afterwards.") },
                confirmButton = {
                    TextButton(onClick = {
                        confirmDeactivate = false
                        vm.deactivate()
                    }) { Text("Deactivate") }
                },
                dismissButton = { TextButton(onClick = { confirmDeactivate = false }) { Text("Cancel") } }
            )
        }
    }
}

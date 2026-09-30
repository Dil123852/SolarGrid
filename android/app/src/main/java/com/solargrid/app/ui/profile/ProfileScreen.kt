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
import androidx.compose.foundation.layout.imePadding
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.solargrid.app.di.ServiceLocator
import com.solargrid.app.ui.common.CenteredLoading
import com.solargrid.app.ui.common.ErrorText
import com.solargrid.app.ui.common.Eyebrow
import com.solargrid.app.ui.common.LoadingButton
import com.solargrid.app.ui.common.SgCard
import com.solargrid.app.ui.common.SgConfirmDialog
import com.solargrid.app.ui.common.SgOutlinedButton
import com.solargrid.app.ui.common.SgPageHeader
import com.solargrid.app.ui.common.SgTextField
import com.solargrid.app.ui.common.SgTopBar
import com.solargrid.app.ui.common.SuccessText
import com.solargrid.app.ui.common.factoryOf
import com.solargrid.app.ui.theme.ErrorRed
import com.solargrid.app.ui.theme.Muted

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
            Modifier.padding(padding).fillMaxSize().imePadding().verticalScroll(rememberScrollState()).padding(20.dp),
            verticalArrangement = Arrangement.spacedBy(18.dp)
        ) {
            SgPageHeader(eyebrow = "Account", title = "My profile", subtitle = "Keep your contact details up to date.")
            SgTextField("NIC (account ID)", vm.nic, {}, enabled = false)
            SgTextField("Full name", vm.name, { vm.name = it })
            SgTextField("Email", vm.email, { vm.email = it }, keyboardType = KeyboardType.Email)
            SgTextField("Phone", vm.phone, { vm.phone = it }, keyboardType = KeyboardType.Phone)
            SgTextField("Address", vm.address, { vm.address = it }, imeAction = ImeAction.Done)

            ErrorText(vm.error)
            SuccessText(vm.message)
            LoadingButton(text = "Save changes", loading = vm.saving, onClick = vm::save)

            SgCard(Modifier.padding(top = 12.dp)) {
                Eyebrow("Danger zone", color = ErrorRed)
                Text("Deactivate account", style = MaterialTheme.typography.titleMedium, modifier = Modifier.padding(top = 6.dp))
                Text(
                    "You will be signed out and cannot sign in again until a Backoffice officer reactivates your account.",
                    style = MaterialTheme.typography.bodyMedium,
                    color = Muted,
                    modifier = Modifier.padding(top = 6.dp, bottom = 14.dp)
                )
                SgOutlinedButton(
                    text = "Deactivate my account",
                    onClick = { confirmDeactivate = true },
                    enabled = !vm.saving,
                    contentColor = ErrorRed
                )
            }
        }

        if (confirmDeactivate) {
            SgConfirmDialog(
                title = "Deactivate account?",
                text = "Only a Backoffice officer can reactivate it afterwards.",
                confirmLabel = "Deactivate",
                destructive = true,
                onConfirm = {
                    confirmDeactivate = false
                    vm.deactivate()
                },
                onDismiss = { confirmDeactivate = false }
            )
        }
    }
}

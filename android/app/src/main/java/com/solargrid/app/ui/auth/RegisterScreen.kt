/*
 * File: RegisterScreen.kt
 * Purpose: Prosumer registration form (NIC is the account key). Signs the user in on success.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.auth

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.imePadding
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Scaffold
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.solargrid.app.di.ServiceLocator
import com.solargrid.app.ui.common.ErrorText
import com.solargrid.app.ui.common.LoadingButton
import com.solargrid.app.ui.common.SgPageHeader
import com.solargrid.app.ui.common.SgTextField
import com.solargrid.app.ui.common.SgTopBar
import com.solargrid.app.ui.common.factoryOf

@Composable
fun RegisterScreen(
    onRegistered: () -> Unit,
    onBack: () -> Unit,
    vm: RegisterViewModel = viewModel(factory = factoryOf { RegisterViewModel(ServiceLocator.authRepository) })
) {
    LaunchedEffect(vm.registered) { if (vm.registered) onRegistered() }

    Scaffold(topBar = { SgTopBar("Create account", onBack = onBack) }) { padding ->
        Column(
            Modifier
                .padding(padding)
                .fillMaxSize()
                .imePadding()
                .verticalScroll(rememberScrollState())
                .padding(20.dp),
            verticalArrangement = Arrangement.spacedBy(18.dp)
        ) {
            SgPageHeader(
                eyebrow = "New prosumer",
                title = "Create your account",
                subtitle = "Your NIC is your account ID - you will sign in with it."
            )
            SgTextField("NIC number", vm.nic, { vm.nic = it }, placeholder = "200012345678 or 991234567V")
            SgTextField("Full name", vm.name, { vm.name = it })
            SgTextField("Email", vm.email, { vm.email = it }, keyboardType = KeyboardType.Email)
            SgTextField("Phone", vm.phone, { vm.phone = it }, keyboardType = KeyboardType.Phone)
            SgTextField("Address", vm.address, { vm.address = it })
            SgTextField("Password", vm.password, { vm.password = it }, keyboardType = KeyboardType.Password, secret = true)
            SgTextField(
                "Confirm password",
                vm.confirmPassword,
                { vm.confirmPassword = it },
                keyboardType = KeyboardType.Password,
                imeAction = ImeAction.Done,
                secret = true
            )
            ErrorText(vm.error)
            LoadingButton(text = "Create account", loading = vm.loading, onClick = vm::submit)
        }
    }
}

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
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.imePadding
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.text.input.VisualTransformation
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.solargrid.app.di.ServiceLocator
import com.solargrid.app.ui.common.ErrorText
import com.solargrid.app.ui.common.LoadingButton
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
                .padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(8.dp)
        ) {
            Field("NIC number", vm.nic, { vm.nic = it })
            Field("Full name", vm.name, { vm.name = it })
            Field("Email", vm.email, { vm.email = it }, KeyboardType.Email)
            Field("Phone", vm.phone, { vm.phone = it }, KeyboardType.Phone)
            Field("Address", vm.address, { vm.address = it })
            Field("Password", vm.password, { vm.password = it }, KeyboardType.Password, secret = true)
            Field("Confirm password", vm.confirmPassword, { vm.confirmPassword = it }, KeyboardType.Password, secret = true)
            ErrorText(vm.error)
            LoadingButton(text = "Register", loading = vm.loading, onClick = vm::submit)
        }
    }
}

@Composable
private fun Field(
    label: String,
    value: String,
    onChange: (String) -> Unit,
    keyboardType: KeyboardType = KeyboardType.Text,
    secret: Boolean = false
) {
    OutlinedTextField(
        value = value,
        onValueChange = onChange,
        label = { Text(label) },
        singleLine = true,
        visualTransformation = if (secret) PasswordVisualTransformation() else VisualTransformation.None,
        keyboardOptions = KeyboardOptions(keyboardType = keyboardType),
        modifier = Modifier.fillMaxWidth()
    )
}

/*
 * File: LoginScreen.kt
 * Purpose: Sign-in screen with a Prosumer / Grid Operator switch and a link to registration.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.auth

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.imePadding
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.systemBarsPadding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.SegmentedButton
import androidx.compose.material3.SegmentedButtonDefaults
import androidx.compose.material3.SingleChoiceSegmentedButtonRow
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.solargrid.app.di.ServiceLocator
import com.solargrid.app.domain.model.Role
import com.solargrid.app.ui.common.CircumIcons
import com.solargrid.app.ui.common.ErrorText
import com.solargrid.app.ui.common.LoadingButton
import com.solargrid.app.ui.common.SgCard
import com.solargrid.app.ui.common.factoryOf
import com.solargrid.app.ui.theme.SolarAmber
import com.solargrid.app.ui.theme.SolarGreen
import com.solargrid.app.ui.theme.SolarGreenDark

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun LoginScreen(
    onLoggedIn: (Role) -> Unit,
    onPendingActivation: (String) -> Unit,
    onRegister: () -> Unit,
    vm: LoginViewModel = viewModel(factory = factoryOf { LoginViewModel(ServiceLocator.authRepository) })
) {
    LaunchedEffect(vm.event) {
        when (val e = vm.event) {
            is LoginEvent.LoggedIn -> onLoggedIn(e.role)
            is LoginEvent.PendingActivation -> onPendingActivation(e.message)
            null -> Unit
        }
        vm.consumeEvent()
    }

    Box(
        Modifier
            .fillMaxSize()
            .background(Brush.verticalGradient(listOf(SolarGreenDark, SolarGreen)))
            .systemBarsPadding()
            .imePadding(),
        contentAlignment = Alignment.Center
    ) {
        Column(
            Modifier.verticalScroll(rememberScrollState()).padding(24.dp),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Icon(CircumIcons.Sun, contentDescription = null, tint = SolarAmber, modifier = Modifier.size(56.dp))
            Text("SolarGrid", style = MaterialTheme.typography.headlineMedium, color = Color.White, fontWeight = FontWeight.Bold)
            Text("Smart Solar Microgrid Trading", color = Color.White.copy(alpha = 0.8f))
            Spacer(Modifier.height(24.dp))

            SgCard {
                SingleChoiceSegmentedButtonRow(Modifier.fillMaxWidth()) {
                    LoginMode.entries.forEachIndexed { index, mode ->
                        SegmentedButton(
                            selected = vm.mode == mode,
                            onClick = { vm.switchMode(mode) },
                            shape = SegmentedButtonDefaults.itemShape(index, LoginMode.entries.size)
                        ) { Text(if (mode == LoginMode.Prosumer) "Prosumer" else "Grid Operator") }
                    }
                }
                Spacer(Modifier.height(16.dp))

                val isProsumer = vm.mode == LoginMode.Prosumer
                OutlinedTextField(
                    value = vm.identifier,
                    onValueChange = { vm.identifier = it },
                    label = { Text(if (isProsumer) "NIC number" else "Username") },
                    singleLine = true,
                    keyboardOptions = KeyboardOptions(imeAction = ImeAction.Next),
                    modifier = Modifier.fillMaxWidth()
                )
                Spacer(Modifier.height(8.dp))
                OutlinedTextField(
                    value = vm.password,
                    onValueChange = { vm.password = it },
                    label = { Text("Password") },
                    singleLine = true,
                    visualTransformation = PasswordVisualTransformation(),
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Password, imeAction = ImeAction.Done),
                    modifier = Modifier.fillMaxWidth()
                )
                ErrorText(vm.error)
                Spacer(Modifier.height(12.dp))
                LoadingButton(text = "Sign in", loading = vm.loading, onClick = vm::submit)

                if (isProsumer) {
                    Spacer(Modifier.height(8.dp))
                    Column(Modifier.fillMaxWidth(), horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.Center) {
                        TextButton(onClick = onRegister) { Text("New prosumer? Create an account") }
                    }
                }
            }
        }
    }
}

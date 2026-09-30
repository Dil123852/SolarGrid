/*
 * File: LoginScreen.kt
 * Purpose: Single sign-in for the app, styled like the web sign-in page: a dark gridded brand
 *          block with the scanner light, then one "NIC or username" form. Prosumers sign in with
 *          their NIC and Grid Operators with their username; the API tells them apart.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.auth

import androidx.compose.animation.AnimatedVisibility
import androidx.compose.animation.core.tween
import androidx.compose.animation.fadeIn
import androidx.compose.animation.slideInVertically
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.imePadding
import androidx.compose.foundation.layout.navigationBarsPadding
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.statusBarsPadding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.solargrid.app.di.ServiceLocator
import com.solargrid.app.domain.model.Role
import com.solargrid.app.ui.common.BrandMark
import com.solargrid.app.ui.common.ErrorText
import com.solargrid.app.ui.common.Eyebrow
import com.solargrid.app.ui.common.LoadingButton
import com.solargrid.app.ui.common.ScannerLine
import com.solargrid.app.ui.common.SgOutlinedButton
import com.solargrid.app.ui.common.SgTextField
import com.solargrid.app.ui.common.factoryOf
import com.solargrid.app.ui.common.gridBackground
import com.solargrid.app.ui.theme.Border
import com.solargrid.app.ui.theme.Muted
import com.solargrid.app.ui.theme.Paper
import com.solargrid.app.ui.theme.SpaceGrotesk
import com.solargrid.app.ui.theme.White

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

    // Staggered entrance, like the web sign-in page.
    var shown by remember { mutableStateOf(false) }
    LaunchedEffect(Unit) { shown = true }

    Column(
        Modifier
            .fillMaxSize()
            .background(Paper)
            .imePadding()
            .verticalScroll(rememberScrollState())
    ) {
        // ---- Brand block ----
        Column(Modifier.fillMaxWidth().gridBackground()) {
            Column(Modifier.statusBarsPadding().padding(start = 24.dp, end = 24.dp, top = 28.dp)) {
                Reveal(shown, delay = 0) {
                    Column {
                        Row(verticalAlignment = Alignment.CenterVertically) {
                            BrandMark(size = 34.dp)
                            Spacer(Modifier.width(12.dp))
                            Text("SolarGrid", color = White, fontFamily = SpaceGrotesk, fontWeight = FontWeight.SemiBold, fontSize = 19.sp)
                        }
                        Eyebrow("Prosumer & operator access", color = White.copy(alpha = 0.62f), modifier = Modifier.padding(top = 18.dp))
                    }
                }
            }

            Spacer(Modifier.height(40.dp))
            Reveal(shown, delay = 120) { ScannerLine() }

            Column(Modifier.padding(start = 24.dp, end = 24.dp, top = 22.dp, bottom = 30.dp)) {
                Reveal(shown, delay = 160) {
                    Text(
                        "Smart Solar Microgrid Trading System",
                        color = White,
                        fontFamily = SpaceGrotesk,
                        fontWeight = FontWeight.Bold,
                        fontSize = 32.sp,
                        lineHeight = 36.sp
                    )
                }
                Reveal(shown, delay = 260) {
                    Text(
                        "Book energy slots, track your transfers and verify handovers at the node - all from one app.",
                        color = White.copy(alpha = 0.78f),
                        style = MaterialTheme.typography.bodyMedium,
                        modifier = Modifier.padding(top = 18.dp)
                    )
                }
                Reveal(shown, delay = 360) {
                    Row(Modifier.padding(top = 28.dp), verticalAlignment = Alignment.CenterVertically) {
                        Eyebrow("Prosumers", color = White.copy(alpha = 0.62f))
                        Box(Modifier.padding(horizontal = 14.dp).width(24.dp).height(1.dp).background(White.copy(alpha = 0.35f)))
                        Eyebrow("Grid operators", color = White.copy(alpha = 0.62f))
                    }
                }
            }
        }

        // ---- Sign-in form ----
        Column(Modifier.fillMaxWidth().navigationBarsPadding().padding(horizontal = 24.dp, vertical = 32.dp)) {
            Reveal(shown, delay = 250) {
                Column {
                    Eyebrow("Sign in")
                    Text(
                        "Account access",
                        style = MaterialTheme.typography.headlineSmall,
                        fontWeight = FontWeight.Medium,
                        modifier = Modifier.padding(top = 10.dp, bottom = 28.dp)
                    )
                }
            }
            Reveal(shown, delay = 330) {
                Column {
                    SgTextField(
                        label = "NIC or username",
                        value = vm.identifier,
                        onValueChange = { vm.identifier = it },
                        placeholder = "200012345678 or operator.id"
                    )
                    Spacer(Modifier.height(20.dp))
                    SgTextField(
                        label = "Password",
                        value = vm.password,
                        onValueChange = { vm.password = it },
                        placeholder = "••••••••",
                        keyboardType = KeyboardType.Password,
                        imeAction = ImeAction.Done,
                        secret = true
                    )
                    Spacer(Modifier.height(12.dp))
                    ErrorText(vm.error)
                    Spacer(Modifier.height(12.dp))
                    LoadingButton(text = "Sign in", loading = vm.loading, onClick = vm::submit)
                }
            }
            Reveal(shown, delay = 420) {
                Column {
                    HorizontalDivider(Modifier.padding(top = 28.dp, bottom = 18.dp), color = Border)
                    Text(
                        "Prosumers sign in with their NIC. Grid operators use the username issued by the Backoffice.",
                        style = MaterialTheme.typography.bodyMedium,
                        color = Muted
                    )
                    Spacer(Modifier.height(18.dp))
                    SgOutlinedButton(text = "New prosumer? Create an account", onClick = onRegister)
                }
            }
        }
    }
}

/** Fades and slides content up into place after [delay] ms. */
@Composable
private fun Reveal(visible: Boolean, delay: Int, content: @Composable () -> Unit) {
    AnimatedVisibility(
        visible = visible,
        enter = fadeIn(tween(600, delayMillis = delay)) + slideInVertically(tween(600, delayMillis = delay)) { it / 5 }
    ) { content() }
}

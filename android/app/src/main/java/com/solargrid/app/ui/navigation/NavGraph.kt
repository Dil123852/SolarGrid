/*
 * File: NavGraph.kt
 * Purpose: App navigation. The start screen comes from the SQLite-cached session:
 *          prosumers land on their dashboard, grid operators on Operator Mode.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.navigation

import android.net.Uri
import androidx.compose.runtime.Composable
import androidx.compose.runtime.remember
import androidx.navigation.NavHostController
import androidx.navigation.NavType
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import androidx.navigation.navArgument
import com.solargrid.app.di.ServiceLocator
import com.solargrid.app.domain.model.Role
import com.solargrid.app.ui.auth.LoginScreen
import com.solargrid.app.ui.auth.PendingActivationScreen
import com.solargrid.app.ui.auth.RegisterScreen
import com.solargrid.app.ui.booking.BookingDetailScreen
import com.solargrid.app.ui.booking.BookingFormScreen
import com.solargrid.app.ui.booking.BookingListScreen
import com.solargrid.app.ui.booking.BookingSummaryScreen
import com.solargrid.app.ui.dashboard.HomeScreen
import com.solargrid.app.ui.map.NodesMapScreen
import com.solargrid.app.ui.operator.OperatorScreen
import com.solargrid.app.ui.profile.ProfileScreen

object Routes {
    const val LOGIN = "login"
    const val REGISTER = "register"
    const val PENDING = "pending?message={message}"
    const val HOME = "home"
    const val BOOKINGS = "bookings"
    const val BOOKING_NEW = "booking/new"
    const val BOOKING_EDIT = "booking/{id}/edit"
    const val BOOKING_DETAIL = "booking/{id}"
    const val SUMMARY = "summary/{action}/{id}"
    const val PROFILE = "profile"
    const val MAP = "map"
    const val OPERATOR = "operator"

    fun pending(message: String) = "pending?message=${Uri.encode(message)}"
    fun bookingEdit(id: String) = "booking/$id/edit"
    fun bookingDetail(id: String) = "booking/$id"
    fun summary(action: SummaryAction, id: String) = "summary/${action.name}/$id"

    fun homeFor(role: Role) = if (role == Role.GridOperator) OPERATOR else HOME
}

enum class SummaryAction { Created, Updated, Cancelled }

// Clears the whole back stack and opens a root screen (after login / logout).
fun NavHostController.resetTo(route: String) {
    navigate(route) {
        popUpTo(graph.id) { inclusive = true }
        launchSingleTop = true
    }
}

@Composable
fun SolarGridNavGraph() {
    val navController = rememberNavController()
    val start = remember {
        ServiceLocator.authRepository.currentSession()?.let { Routes.homeFor(it.role) } ?: Routes.LOGIN
    }

    val logout: () -> Unit = {
        ServiceLocator.authRepository.logout()
        navController.resetTo(Routes.LOGIN)
    }

    NavHost(navController = navController, startDestination = start) {
        composable(Routes.LOGIN) {
            LoginScreen(
                onLoggedIn = { role -> navController.resetTo(Routes.homeFor(role)) },
                onPendingActivation = { message -> navController.navigate(Routes.pending(message)) },
                onRegister = { navController.navigate(Routes.REGISTER) }
            )
        }
        composable(Routes.REGISTER) {
            RegisterScreen(
                onRegistered = { navController.resetTo(Routes.HOME) },
                onBack = { navController.popBackStack() }
            )
        }
        composable(
            Routes.PENDING,
            arguments = listOf(navArgument("message") { type = NavType.StringType; defaultValue = "" })
        ) { entry ->
            PendingActivationScreen(
                message = entry.arguments?.getString("message").orEmpty(),
                onBackToLogin = { navController.resetTo(Routes.LOGIN) }
            )
        }
        composable(Routes.HOME) {
            HomeScreen(
                onNewBooking = { navController.navigate(Routes.BOOKING_NEW) },
                onBookings = { navController.navigate(Routes.BOOKINGS) },
                onBookingClick = { id -> navController.navigate(Routes.bookingDetail(id)) },
                onMap = { navController.navigate(Routes.MAP) },
                onProfile = { navController.navigate(Routes.PROFILE) },
                onLogout = logout
            )
        }
        composable(Routes.BOOKINGS) {
            BookingListScreen(
                onBookingClick = { id -> navController.navigate(Routes.bookingDetail(id)) },
                onNewBooking = { navController.navigate(Routes.BOOKING_NEW) },
                onBack = { navController.popBackStack() }
            )
        }
        composable(Routes.BOOKING_NEW) {
            BookingFormScreen(
                reservationId = null,
                onSaved = { id ->
                    navController.navigate(Routes.summary(SummaryAction.Created, id)) {
                        popUpTo(Routes.HOME)
                    }
                },
                onBack = { navController.popBackStack() }
            )
        }
        composable(Routes.BOOKING_EDIT, arguments = listOf(navArgument("id") { type = NavType.StringType })) { entry ->
            BookingFormScreen(
                reservationId = entry.arguments?.getString("id"),
                onSaved = { id ->
                    navController.navigate(Routes.summary(SummaryAction.Updated, id)) {
                        popUpTo(Routes.HOME)
                    }
                },
                onBack = { navController.popBackStack() }
            )
        }
        composable(Routes.BOOKING_DETAIL, arguments = listOf(navArgument("id") { type = NavType.StringType })) { entry ->
            val id = entry.arguments?.getString("id").orEmpty()
            BookingDetailScreen(
                reservationId = id,
                onEdit = { navController.navigate(Routes.bookingEdit(id)) },
                onCancelled = {
                    navController.navigate(Routes.summary(SummaryAction.Cancelled, id)) {
                        popUpTo(Routes.HOME)
                    }
                },
                onBack = { navController.popBackStack() }
            )
        }
        composable(
            Routes.SUMMARY,
            arguments = listOf(
                navArgument("action") { type = NavType.StringType },
                navArgument("id") { type = NavType.StringType }
            )
        ) { entry ->
            val action = runCatching { SummaryAction.valueOf(entry.arguments?.getString("action").orEmpty()) }
                .getOrDefault(SummaryAction.Created)
            BookingSummaryScreen(
                action = action,
                reservationId = entry.arguments?.getString("id").orEmpty(),
                onHome = { navController.popBackStack(Routes.HOME, inclusive = false) },
                onViewBookings = {
                    navController.navigate(Routes.BOOKINGS) { popUpTo(Routes.HOME) }
                }
            )
        }
        composable(Routes.PROFILE) {
            ProfileScreen(
                onDeactivated = { message ->
                    ServiceLocator.authRepository.logout()
                    navController.resetTo(Routes.pending(message))
                },
                onBack = { navController.popBackStack() }
            )
        }
        composable(Routes.MAP) {
            NodesMapScreen(onBack = { navController.popBackStack() })
        }
        composable(Routes.OPERATOR) {
            OperatorScreen(
                onMap = { navController.navigate(Routes.MAP) },
                onLogout = logout
            )
        }
    }
}

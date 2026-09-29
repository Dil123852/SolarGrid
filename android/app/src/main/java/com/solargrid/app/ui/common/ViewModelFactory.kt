/*
 * File: ViewModelFactory.kt
 * Purpose: One-line ViewModel factories so screens can hand repositories from the
 *          ServiceLocator to their ViewModels without a DI framework.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.common

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewmodel.initializer
import androidx.lifecycle.viewmodel.viewModelFactory

inline fun <reified VM : ViewModel> factoryOf(crossinline create: () -> VM): ViewModelProvider.Factory =
    viewModelFactory { initializer { create() } }

/*
 * File: ApiClient.kt
 * Purpose: Builds the Retrofit client - base URL from BuildConfig, a bearer-token interceptor
 *          reading the cached session, and request logging for debugging.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.data.remote

import okhttp3.Interceptor
import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import java.util.concurrent.TimeUnit

object ApiClient {

    fun create(baseUrl: String, tokenProvider: () -> String?, debug: Boolean): ApiService {
        // Adds "Authorization: Bearer <jwt>" whenever a session exists.
        val authInterceptor = Interceptor { chain ->
            val token = tokenProvider()
            val request = if (token.isNullOrEmpty()) {
                chain.request()
            } else {
                chain.request().newBuilder().header("Authorization", "Bearer $token").build()
            }
            chain.proceed(request)
        }

        val logging = HttpLoggingInterceptor().apply {
            level = if (debug) HttpLoggingInterceptor.Level.BODY else HttpLoggingInterceptor.Level.NONE
        }

        val client = OkHttpClient.Builder()
            .addInterceptor(authInterceptor)
            .addInterceptor(logging)
            .connectTimeout(15, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
            .build()

        return Retrofit.Builder()
            .baseUrl(if (baseUrl.endsWith("/")) baseUrl else "$baseUrl/")
            .client(client)
            .addConverterFactory(GsonConverterFactory.create())
            .build()
            .create(ApiService::class.java)
    }
}

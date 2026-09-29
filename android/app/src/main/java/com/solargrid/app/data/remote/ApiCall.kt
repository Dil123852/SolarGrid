/*
 * File: ApiCall.kt
 * Purpose: Runs a Retrofit call and converts the outcome to a Kotlin Result. API failures carry
 *          the server's { message } so screens show the exact business-rule text.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.data.remote

import com.google.gson.Gson
import com.google.gson.JsonParseException
import com.solargrid.app.data.remote.dto.MessageDto
import retrofit2.Response
import java.io.IOException
import kotlin.coroutines.cancellation.CancellationException

class ApiException(val code: Int, message: String) : Exception(message)

private val gson = Gson()

suspend fun <T> apiCall(call: suspend () -> Response<T>): Result<T> =
    try {
        val response = call()
        val body = response.body()
        if (response.isSuccessful && body != null) {
            Result.success(body)
        } else {
            Result.failure(ApiException(response.code(), errorMessage(response)))
        }
    } catch (e: IOException) {
        Result.failure(ApiException(0, "Cannot reach the SolarGrid server. Check your connection and try again."))
    } catch (e: JsonParseException) {
        Result.failure(ApiException(-1, "The server sent an unexpected response."))
    } catch (e: CancellationException) {
        throw e
    } catch (e: Exception) {
        Result.failure(ApiException(-1, e.message ?: "Unexpected error."))
    }

private fun errorMessage(response: Response<*>): String {
    val serverMessage = try {
        response.errorBody()?.string()?.let { gson.fromJson(it, MessageDto::class.java)?.message }
    } catch (e: Exception) {
        null
    }
    return serverMessage ?: when (response.code()) {
        401 -> "Your session has expired. Please sign in again."
        403 -> "You are not allowed to do that."
        404 -> "Not found."
        else -> "Request failed (${response.code()})."
    }
}

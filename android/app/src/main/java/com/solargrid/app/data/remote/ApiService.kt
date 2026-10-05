/*
 * File: ApiService.kt
 * Purpose: Retrofit definition of every SolarGrid API endpoint the mobile app calls.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.data.remote

import com.solargrid.app.data.remote.dto.AuthResponseDto
import com.solargrid.app.data.remote.dto.BookingSlotDto
import com.solargrid.app.data.remote.dto.CreateReservationRequest
import com.solargrid.app.data.remote.dto.DashboardDto
import com.solargrid.app.data.remote.dto.LoginRequest
import com.solargrid.app.data.remote.dto.MessageDto
import com.solargrid.app.data.remote.dto.MobileLoginRequest
import com.solargrid.app.data.remote.dto.NodeDto
import com.solargrid.app.data.remote.dto.ProsumerDto
import com.solargrid.app.data.remote.dto.ProsumerLoginRequest
import com.solargrid.app.data.remote.dto.RegisterProsumerRequest
import com.solargrid.app.data.remote.dto.ReservationDto
import com.solargrid.app.data.remote.dto.UpdateProsumerRequest
import com.solargrid.app.data.remote.dto.UpdateReservationRequest
import com.solargrid.app.data.remote.dto.VerifyQrRequest
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.DELETE
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.PUT
import retrofit2.http.Path
import retrofit2.http.Query

interface ApiService {

    // ---- Auth ----
    @POST("api/auth/login")
    suspend fun login(@Body body: LoginRequest): Response<AuthResponseDto>

    @POST("api/auth/prosumer-login")
    suspend fun prosumerLogin(@Body body: ProsumerLoginRequest): Response<AuthResponseDto>

    // Single sign-in for the app; the API decides whether the identifier is a NIC or a username.
    @POST("api/auth/mobile-login")
    suspend fun mobileLogin(@Body body: MobileLoginRequest): Response<AuthResponseDto>

    // ---- Prosumer account ----
    @POST("api/prosumers")
    suspend fun registerProsumer(@Body body: RegisterProsumerRequest): Response<ProsumerDto>

    @GET("api/prosumers/{nic}")
    suspend fun getProsumer(@Path("nic") nic: String): Response<ProsumerDto>

    @PUT("api/prosumers/{nic}")
    suspend fun updateProsumer(@Path("nic") nic: String, @Body body: UpdateProsumerRequest): Response<ProsumerDto>

    @PUT("api/prosumers/{nic}/deactivate")
    suspend fun deactivateProsumer(@Path("nic") nic: String): Response<MessageDto>

    // ---- Nodes ----
    @GET("api/nodes")
    suspend fun getNodes(@Query("active") active: Boolean?): Response<List<NodeDto>>

    // ---- Energy booking slots ----
    // Slots overlapping [from, to) at a station, with live availability.
    @GET("api/slots")
    suspend fun getSlots(
        @Query("nodeId") nodeId: String?,
        @Query("from") from: String?,
        @Query("to") to: String?
    ): Response<List<BookingSlotDto>>

    // ---- Reservations ----
    @GET("api/reservations")
    suspend fun getReservations(
        @Query("nodeId") nodeId: String?,
        @Query("status") status: String?,
        @Query("from") from: String?,
        @Query("to") to: String?,
        @Query("search") search: String?
    ): Response<List<ReservationDto>>

    @GET("api/reservations/{id}")
    suspend fun getReservation(@Path("id") id: String): Response<ReservationDto>

    @POST("api/reservations")
    suspend fun createReservation(@Body body: CreateReservationRequest): Response<ReservationDto>

    @PUT("api/reservations/{id}")
    suspend fun updateReservation(@Path("id") id: String, @Body body: UpdateReservationRequest): Response<ReservationDto>

    @DELETE("api/reservations/{id}")
    suspend fun cancelReservation(@Path("id") id: String): Response<ReservationDto>

    @POST("api/reservations/verify-qr")
    suspend fun verifyQr(@Body body: VerifyQrRequest): Response<ReservationDto>

    // ---- Dashboard ----
    @GET("api/dashboard")
    suspend fun dashboard(): Response<DashboardDto>
}

/*
 * File: types.ts
 * Purpose: TypeScript mirrors of the SolarGrid API's JSON contracts (see SolarGrid.Application/DTOs).
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

export type Role = "Backoffice" | "GridOperator" | "Prosumer";
export type StaffRole = Exclude<Role, "Prosumer">;
export type ReservationStatus = "Pending" | "Approved" | "Cancelled" | "Completed";

export interface AuthResponse {
  token: string;
  expiresAt: string;
  role: Role;
  displayName: string;
  nic: string | null;
}

export interface MessageResponse {
  message: string;
}

export interface StaffUser {
  id: string;
  username: string;
  email: string;
  role: StaffRole;
  isActive: boolean;
  createdAt: string;
}

export interface RegisterStaffRequest {
  username: string;
  email: string;
  password: string;
  role: StaffRole;
}

export interface Prosumer {
  nic: string;
  name: string;
  email: string;
  phone: string;
  address: string;
  isActive: boolean;
  createdAt: string;
}

export interface MicrogridNode {
  id: string;
  name: string;
  latitude: number;
  longitude: number;
  capacityKWh: number;
  batterySlots: number;
  isActive: boolean;
  // Operating hours, "HH:mm" Sri Lanka time; null means open around the clock.
  openTime: string | null;
  closeTime: string | null;
}

export type NodeRequest = Omit<MicrogridNode, "id" | "isActive">;

export interface RegisterProsumerRequest {
  nic: string;
  name: string;
  email: string;
  phone: string;
  address: string;
  password: string;
}

export type UpdateProsumerRequest = Omit<RegisterProsumerRequest, "nic" | "password">;

export interface Reservation {
  id: string;
  prosumerNic: string;
  nodeId: string;
  nodeName: string;
  slotTime: string;
  status: ReservationStatus;
  qrToken: string | null;
  createdAt: string;
  updatedAt: string | null;
  // The energy booking slot this reservation was made in (null when the station publishes none).
  slotId: string | null;
}

// A bookable time window published at a solar station, with live booked/available counts.
export interface BookingSlot {
  id: string;
  nodeId: string;
  nodeName: string;
  startTime: string;
  endTime: string;
  capacity: number;
  booked: number;
  available: number;
  isActive: boolean;
}

export interface BookingSlotRequest {
  nodeId: string;
  startTime: string;
  endTime: string;
  capacity: number;
}

export interface ReservationFilter {
  status?: ReservationStatus | "";
  nodeId?: string;
  nic?: string;
  from?: string;
  to?: string;
  // Free text matched by the API against node name, NIC, booking reference and status.
  search?: string;
}

export interface CreateReservationRequest {
  nodeId: string;
  slotTime: string;
  prosumerNic: string;
}

export interface UpdateReservationRequest {
  slotTime: string;
  nodeId: string;
}

export interface DashboardCounts {
  pendingCount: number;
  approvedFutureCount: number;
  completedCount: number;
}

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
}

export type NodeRequest = Omit<MicrogridNode, "id" | "isActive">;

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
}

export interface ReservationFilter {
  status?: ReservationStatus | "";
  nodeId?: string;
  nic?: string;
  from?: string;
  to?: string;
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

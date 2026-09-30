/*
 * File: endpoints.ts
 * Purpose: Endpoint wrappers - one object per API resource. Pages call these, never fetch()
 *          directly, so every URL the web app uses is listed in one place.
 * Project: Smart Solar Microgrid Trading System - Web Application
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

import { http } from "./client";
import type {
  AuthResponse,
  CreateReservationRequest,
  DashboardCounts,
  MessageResponse,
  MicrogridNode,
  NodeRequest,
  Prosumer,
  RegisterStaffRequest,
  Reservation,
  ReservationFilter,
  StaffUser,
  UpdateReservationRequest,
} from "./types";

const enc = encodeURIComponent;

export const authApi = {
  login: (username: string, password: string) => http.post<AuthResponse>("/api/auth/login", { username, password }),
};

export const usersApi = {
  list: () => http.get<StaffUser[]>("/api/users"),
  create: (user: RegisterStaffRequest) => http.post<StaffUser>("/api/auth/register", user),
  activate: (id: string) => http.put<MessageResponse>(`/api/users/${enc(id)}/activate`),
  deactivate: (id: string) => http.put<MessageResponse>(`/api/users/${enc(id)}/deactivate`),
};

export const nodesApi = {
  list: (active?: boolean) => http.get<MicrogridNode[]>("/api/nodes", { active }),
  create: (node: NodeRequest) => http.post<MicrogridNode>("/api/nodes", node),
  update: (id: string, node: NodeRequest) => http.put<MicrogridNode>(`/api/nodes/${enc(id)}`, node),
  activate: (id: string) => http.put<MessageResponse>(`/api/nodes/${enc(id)}/activate`),
  deactivate: (id: string) => http.put<MessageResponse>(`/api/nodes/${enc(id)}/deactivate`),
};

export const prosumersApi = {
  list: (active?: boolean) => http.get<Prosumer[]>("/api/prosumers", { active }),
  reactivate: (nic: string) => http.put<MessageResponse>(`/api/prosumers/${enc(nic)}/reactivate`),
  deactivate: (nic: string) => http.put<MessageResponse>(`/api/prosumers/${enc(nic)}/deactivate`),
};

export const reservationsApi = {
  list: (filter: ReservationFilter = {}) => http.get<Reservation[]>("/api/reservations", { ...filter }),
  create: (booking: CreateReservationRequest) => http.post<Reservation>("/api/reservations", booking),
  update: (id: string, change: UpdateReservationRequest) => http.put<Reservation>(`/api/reservations/${enc(id)}`, change),
  cancel: (id: string) => http.del<Reservation>(`/api/reservations/${enc(id)}`),
  approve: (id: string) => http.post<Reservation>(`/api/reservations/${enc(id)}/approve`),
};

export const dashboardApi = {
  get: () => http.get<DashboardCounts>("/api/dashboard"),
};

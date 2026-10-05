export type UserRole = 'Admin' | 'VehicleManager';

export interface AuthResponse {
  token: string;
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
}

export interface CurrentUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  isActive: boolean;
}

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  isActive: boolean;
  createdAt: string;
  vehicleIds: string[];
}

export interface VehicleManager {
  id: string;
  email: string;
  fullName: string;
}

export interface VehiclePhoto {
  id: string;
  url: string;
  contentType: string;
}

export type DocumentKind =
  | 'InsurancePolicy'
  | 'Tenencia'
  | 'CirculationCard'
  | 'TechnicalInspection'
  | 'Invoice'
  | 'Other';

export interface VehicleDocument {
  id: string;
  title: string;
  kind: DocumentKind;
  fileName: string;
  url: string;
  contentType: string;
  notes?: string | null;
  issuedOn?: string | null;
  expiresOn?: string | null;
  createdAt: string;
}

export interface Vehicle {
  id: string;
  brand: string;
  model: string;
  year: number;
  licensePlate: string;
  vin?: string | null;
  color: string;
  currentMileage: number;
  notes?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  managers: VehicleManager[];
  photos: VehiclePhoto[];
  documents: VehicleDocument[];
}

export interface MaintenanceRecord {
  id: string;
  vehicleId: string;
  title: string;
  description?: string | null;
  serviceDate: string;
  mileage: number;
  cost: number;
  createdByUserId: string;
  createdByName: string;
  createdAt: string;
}

export type ProcedureFrequency = 'Once' | 'Monthly' | 'Semiannual' | 'Annual';
export type ProcedureStatus = 'Pending' | 'Upcoming' | 'Overdue' | 'Completed';

export interface Procedure {
  id: string;
  vehicleId: string;
  vehicleName: string;
  licensePlate: string;
  title: string;
  description?: string | null;
  frequency: ProcedureFrequency;
  dueDate: string;
  lastCost?: number | null;
  isCompleted: boolean;
  status: ProcedureStatus;
  completionCount: number;
  createdAt: string;
}

export interface ProcedureCompletion {
  id: string;
  procedureId: string;
  completedAt: string;
  dueDateCovered: string;
  cost: number;
  notes?: string | null;
  createdByName: string;
  createdAt: string;
}

export interface DashboardStats {
  totalVehicles: number;
  totalUsers: number;
  totalMaintenanceRecords: number;
  totalMaintenanceCost: number;
  pendingProcedures: number;
  overdueProcedures: number;
}

export interface ApiError {
  message?: string;
}

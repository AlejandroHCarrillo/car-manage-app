import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { Procedure, ProcedureCompletion, ProcedureFrequency, ProcedureStatus } from './models';

export interface ProcedurePayload {
  title: string;
  description?: string | null;
  frequency: ProcedureFrequency;
  dueDate: string;
}

export interface CompleteProcedurePayload {
  completedAt: string;
  cost: number;
  notes?: string | null;
}

@Injectable({ providedIn: 'root' })
export class ProcedureService {
  private readonly http = inject(HttpClient);

  getAll() {
    return this.http.get<Procedure[]>(`${environment.apiUrl}/procedures`);
  }

  getByVehicle(vehicleId: string) {
    return this.http.get<Procedure[]>(`${environment.apiUrl}/vehicles/${vehicleId}/procedures`);
  }

  getById(id: string) {
    return this.http.get<Procedure>(`${environment.apiUrl}/procedures/${id}`);
  }

  getHistory(id: string) {
    return this.http.get<ProcedureCompletion[]>(`${environment.apiUrl}/procedures/${id}/history`);
  }

  create(vehicleId: string, payload: ProcedurePayload) {
    return this.http.post<Procedure>(`${environment.apiUrl}/vehicles/${vehicleId}/procedures`, payload);
  }

  update(id: string, payload: ProcedurePayload) {
    return this.http.put<Procedure>(`${environment.apiUrl}/procedures/${id}`, payload);
  }

  complete(id: string, payload: CompleteProcedurePayload) {
    return this.http.post<Procedure>(`${environment.apiUrl}/procedures/${id}/complete`, payload);
  }

  delete(id: string) {
    return this.http.delete(`${environment.apiUrl}/procedures/${id}`);
  }

  frequencyLabel(frequency: ProcedureFrequency): string {
    switch (frequency) {
      case 'Once':
        return 'Una sola vez';
      case 'Monthly':
        return 'Mensual';
      case 'Semiannual':
        return 'Semestral';
      case 'Annual':
        return 'Anual';
    }
  }

  statusLabel(status: ProcedureStatus): string {
    switch (status) {
      case 'Completed':
        return 'Completado';
      case 'Overdue':
        return 'Vencido';
      case 'Upcoming':
        return 'Próximo';
      default:
        return 'Pendiente';
    }
  }

  statusBadge(status: ProcedureStatus): string {
    switch (status) {
      case 'Completed':
        return 'badge-success';
      case 'Overdue':
        return 'badge-error';
      case 'Upcoming':
        return 'badge-warning';
      default:
        return 'badge-ghost';
    }
  }
}

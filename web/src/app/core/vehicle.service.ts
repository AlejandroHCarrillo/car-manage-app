import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { DashboardStats, DocumentKind, MaintenanceRecord, Vehicle, VehicleDocument, VehiclePhoto } from './models';

export interface VehiclePayload {
  brand: string;
  model: string;
  year: number;
  licensePlate: string;
  vin?: string | null;
  color: string;
  currentMileage: number;
  notes?: string | null;
}

export interface MaintenancePayload {
  title: string;
  description?: string | null;
  serviceDate: string;
  mileage: number;
  cost: number;
}

@Injectable({ providedIn: 'root' })
export class VehicleService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/vehicles`;

  getAll() {
    return this.http.get<Vehicle[]>(this.base);
  }

  getById(id: string) {
    return this.http.get<Vehicle>(`${this.base}/${id}`);
  }

  create(payload: VehiclePayload) {
    return this.http.post<Vehicle>(this.base, payload);
  }

  update(id: string, payload: VehiclePayload) {
    return this.http.put<Vehicle>(`${this.base}/${id}`, payload);
  }

  delete(id: string) {
    return this.http.delete(`${this.base}/${id}`);
  }

  dashboard() {
    return this.http.get<DashboardStats>(`${this.base}/dashboard`);
  }

  assignManager(vehicleId: string, userId: string) {
    return this.http.post(`${this.base}/${vehicleId}/managers`, { userId });
  }

  unassignManager(vehicleId: string, userId: string) {
    return this.http.delete(`${this.base}/${vehicleId}/managers/${userId}`);
  }

  getMaintenance(vehicleId: string) {
    return this.http.get<MaintenanceRecord[]>(`${this.base}/${vehicleId}/maintenance`);
  }

  addMaintenance(vehicleId: string, payload: MaintenancePayload) {
    return this.http.post<MaintenanceRecord>(`${this.base}/${vehicleId}/maintenance`, payload);
  }

  deleteMaintenance(id: string) {
    return this.http.delete(`${environment.apiUrl}/maintenance/${id}`);
  }

  uploadPhoto(vehicleId: string, file: File) {
    const data = new FormData();
    data.append('file', file);
    return this.http.post<VehiclePhoto>(`${this.base}/${vehicleId}/photos`, data);
  }

  deletePhoto(vehicleId: string, photoId: string) {
    return this.http.delete(`${this.base}/${vehicleId}/photos/${photoId}`);
  }

  photoSrc(url: string): string {
    if (url.startsWith('http') || url.startsWith('blob:')) {
      return url;
    }
    return `${environment.filesUrl}${url}`;
  }

  uploadDocument(
    vehicleId: string,
    payload: {
      file: File;
      title: string;
      kind: DocumentKind;
      notes?: string | null;
      issuedOn?: string | null;
      expiresOn?: string | null;
    },
  ) {
    const data = new FormData();
    data.append('file', payload.file);
    data.append('title', payload.title);
    data.append('kind', payload.kind);
    if (payload.notes) {
      data.append('notes', payload.notes);
    }
    if (payload.issuedOn) {
      data.append('issuedOn', payload.issuedOn);
    }
    if (payload.expiresOn) {
      data.append('expiresOn', payload.expiresOn);
    }
    return this.http.post<VehicleDocument>(`${this.base}/${vehicleId}/documents`, data);
  }

  deleteDocument(vehicleId: string, documentId: string) {
    return this.http.delete(`${this.base}/${vehicleId}/documents/${documentId}`);
  }

  documentKindLabel(kind: DocumentKind): string {
    switch (kind) {
      case 'InsurancePolicy':
        return 'Póliza de seguro';
      case 'Tenencia':
        return 'Tenencia / refrendo';
      case 'CirculationCard':
        return 'Tarjeta de circulación';
      case 'TechnicalInspection':
        return 'Verificación / revisión técnica';
      case 'Invoice':
        return 'Factura';
      default:
        return 'Otro';
    }
  }

  documentIcon(contentType: string): string {
    if (contentType.includes('pdf')) {
      return 'fa-file-pdf';
    }
    if (contentType.startsWith('image/')) {
      return 'fa-file-image';
    }
    return 'fa-file';
  }

  isImage(contentType: string): boolean {
    return contentType.startsWith('image/');
  }
}

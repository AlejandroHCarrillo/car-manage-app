import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';
import { readApiError } from '../../core/http-error';
import { MaintenanceRecord, Procedure, User, Vehicle } from '../../core/models';
import { ProcedureService } from '../../core/procedure.service';
import { UserService } from '../../core/user.service';
import { VehicleService } from '../../core/vehicle.service';
import { DocumentEditor } from '../../shared/document-editor';
import { MaintenanceEditor } from '../../shared/maintenance-editor';
import { Modal } from '../../shared/modal';
import { ProcedureEditor } from '../../shared/procedure-editor';

@Component({
  selector: 'app-vehicle-detail',
  imports: [RouterLink, CurrencyPipe, DatePipe, DecimalPipe, Modal, ProcedureEditor, MaintenanceEditor, DocumentEditor],
  templateUrl: './vehicle-detail.html',
})
export class VehicleDetail implements OnInit {
  protected readonly vehiclesApi = inject(VehicleService);
  private readonly usersApi = inject(UserService);
  protected readonly proceduresApi = inject(ProcedureService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  protected readonly auth = inject(AuthService);

  protected readonly vehicle = signal<Vehicle | null>(null);
  protected readonly records = signal<MaintenanceRecord[]>([]);
  protected readonly procedures = signal<Procedure[]>([]);
  protected readonly managers = signal<User[]>([]);
  protected readonly error = signal('');
  protected readonly selectedManagerId = signal('');
  protected readonly procedureModal = signal(false);
  protected readonly maintenanceModal = signal(false);
  protected readonly documentModal = signal(false);
  protected readonly availableManagers = computed(() => {
    const assigned = new Set(this.vehicle()?.managers.map((manager) => manager.id) ?? []);
    return this.managers().filter((manager) => !assigned.has(manager.id));
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      return;
    }
    this.load(id);
  }

  protected assign(): void {
    const vehicle = this.vehicle();
    const userId = this.selectedManagerId();
    if (!vehicle || !userId) {
      return;
    }

    this.vehiclesApi.assignManager(vehicle.id, userId).subscribe({
      next: () => this.load(vehicle.id),
      error: (err) => this.error.set(readApiError(err)),
    });
  }

  protected unassign(userId: string): void {
    const vehicle = this.vehicle();
    if (!vehicle) {
      return;
    }

    this.vehiclesApi.unassignManager(vehicle.id, userId).subscribe({
      next: () => this.load(vehicle.id),
      error: (err) => this.error.set(readApiError(err)),
    });
  }

  protected onProcedureSaved(): void {
    const vehicle = this.vehicle();
    this.procedureModal.set(false);
    if (vehicle) {
      this.load(vehicle.id);
    }
  }

  protected onMaintenanceSaved(): void {
    const vehicle = this.vehicle();
    this.maintenanceModal.set(false);
    if (vehicle) {
      this.load(vehicle.id);
    }
  }

  protected onDocumentSaved(): void {
    const vehicle = this.vehicle();
    this.documentModal.set(false);
    if (vehicle) {
      this.load(vehicle.id);
    }
  }

  protected removeDocument(documentId: string): void {
    const vehicle = this.vehicle();
    if (!vehicle || !confirm('¿Eliminar este documento?')) {
      return;
    }

    this.vehiclesApi.deleteDocument(vehicle.id, documentId).subscribe({
      next: () => this.load(vehicle.id),
      error: (err) => this.error.set(readApiError(err)),
    });
  }

  protected isExpired(date?: string | null): boolean {
    return !!date && new Date(date) < new Date(new Date().toDateString());
  }

  protected removeMaintenance(id: string): void {
    const vehicle = this.vehicle();
    if (!vehicle) {
      return;
    }

    this.vehiclesApi.deleteMaintenance(id).subscribe({
      next: () => this.load(vehicle.id),
      error: (err) => this.error.set(readApiError(err)),
    });
  }

  protected removeVehicle(): void {
    const vehicle = this.vehicle();
    if (!vehicle || !confirm('¿Eliminar este vehículo?')) {
      return;
    }

    this.vehiclesApi.delete(vehicle.id).subscribe({
      next: () => void this.router.navigateByUrl('/vehicles'),
      error: (err) => this.error.set(readApiError(err)),
    });
  }

  private load(id: string): void {
    this.vehiclesApi.getById(id).subscribe({
      next: (vehicle) => this.vehicle.set(vehicle),
      error: (err) => this.error.set(readApiError(err)),
    });

    this.vehiclesApi.getMaintenance(id).subscribe({
      next: (records) => this.records.set(records),
      error: (err) => this.error.set(readApiError(err)),
    });

    this.proceduresApi.getByVehicle(id).subscribe({
      next: (items) => this.procedures.set(items),
      error: (err) => this.error.set(readApiError(err)),
    });

    if (this.auth.isAdmin()) {
      this.usersApi.getAll().subscribe((users) => {
        this.managers.set(users.filter((user) => user.role === 'VehicleManager' && user.isActive));
      });
    }
  }
}

import { Component, inject, input, OnInit, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { readApiError } from '../core/http-error';
import { Procedure, ProcedureFrequency, Vehicle } from '../core/models';
import { ProcedureService } from '../core/procedure.service';
import { VehicleService } from '../core/vehicle.service';

@Component({
  selector: 'app-procedure-editor',
  imports: [ReactiveFormsModule],
  templateUrl: './procedure-editor.html',
})
export class ProcedureEditor implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ProcedureService);
  private readonly vehiclesApi = inject(VehicleService);

  readonly procedureId = input<string | null>(null);
  readonly vehicleId = input<string | null>(null);
  readonly lockVehicle = input(false);
  readonly variant = input<'page' | 'modal'>('page');

  readonly saved = output<Procedure>();
  readonly cancelled = output<void>();

  protected readonly vehicles = signal<Vehicle[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal('');
  protected readonly isEdit = () => !!this.procedureId();

  protected readonly form = this.fb.nonNullable.group({
    vehicleId: ['', Validators.required],
    title: ['', Validators.required],
    description: [''],
    frequency: ['Annual' as ProcedureFrequency, Validators.required],
    dueDate: [new Date().toISOString().slice(0, 10), Validators.required],
  });

  ngOnInit(): void {
    const vehicleId = this.vehicleId();
    const procedureId = this.procedureId();

    if (!this.lockVehicle()) {
      this.vehiclesApi.getAll().subscribe({
        next: (vehicles) => this.vehicles.set(vehicles),
        error: (err) => this.error.set(readApiError(err)),
      });
    }

    if (vehicleId) {
      this.form.patchValue({ vehicleId });
    }

    if (!procedureId) {
      return;
    }

    this.api.getById(procedureId).subscribe({
      next: (item) => {
        this.form.patchValue({
          vehicleId: item.vehicleId,
          title: item.title,
          description: item.description ?? '',
          frequency: item.frequency,
          dueDate: item.dueDate.slice(0, 10),
        });
        this.form.controls.vehicleId.disable();
      },
      error: (err) => this.error.set(readApiError(err)),
    });
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set('');
    const value = this.form.getRawValue();
    const payload = {
      title: value.title,
      description: value.description || null,
      frequency: value.frequency,
      dueDate: value.dueDate,
    };

    const request = this.procedureId()
      ? this.api.update(this.procedureId()!, payload)
      : this.api.create(value.vehicleId, payload);

    request.subscribe({
      next: (item) => this.saved.emit(item),
      error: (err) => {
        this.loading.set(false);
        this.error.set(readApiError(err));
      },
    });
  }
}

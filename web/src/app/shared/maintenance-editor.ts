import { Component, inject, input, OnInit, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { readApiError } from '../core/http-error';
import { MaintenanceRecord } from '../core/models';
import { VehicleService } from '../core/vehicle.service';

@Component({
  selector: 'app-maintenance-editor',
  imports: [ReactiveFormsModule],
  templateUrl: './maintenance-editor.html',
})
export class MaintenanceEditor implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly vehiclesApi = inject(VehicleService);

  readonly vehicleId = input.required<string>();
  readonly defaultMileage = input(0);
  readonly variant = input<'page' | 'modal'>('modal');

  readonly saved = output<MaintenanceRecord>();
  readonly cancelled = output<void>();

  protected readonly loading = signal(false);
  protected readonly error = signal('');

  protected readonly form = this.fb.nonNullable.group({
    title: ['', Validators.required],
    description: [''],
    serviceDate: [new Date().toISOString().slice(0, 10), Validators.required],
    mileage: [0, [Validators.required, Validators.min(0)]],
    cost: [0, [Validators.required, Validators.min(0)]],
  });

  ngOnInit(): void {
    this.form.patchValue({ mileage: this.defaultMileage() });
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set('');
    const value = this.form.getRawValue();

    this.vehiclesApi
      .addMaintenance(this.vehicleId(), {
        ...value,
        description: value.description || null,
      })
      .subscribe({
        next: (record) => this.saved.emit(record),
        error: (err) => {
          this.loading.set(false);
          this.error.set(readApiError(err));
        },
      });
  }
}

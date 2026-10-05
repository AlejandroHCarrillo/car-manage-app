import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import { readApiError } from '../../core/http-error';
import { VehiclePhoto } from '../../core/models';
import { VehicleService } from '../../core/vehicle.service';

interface PendingPhoto {
  id: string;
  file: File;
  preview: string;
}

@Component({
  selector: 'app-vehicle-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './vehicle-form.html',
})
export class VehicleForm implements OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  protected readonly vehiclesApi = inject(VehicleService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly id = signal<string | null>(null);
  protected readonly loading = signal(false);
  protected readonly error = signal('');
  protected readonly savedPhotos = signal<VehiclePhoto[]>([]);
  protected readonly pendingPhotos = signal<PendingPhoto[]>([]);
  protected readonly isEdit = () => !!this.id();

  protected readonly form = this.fb.nonNullable.group({
    brand: ['', Validators.required],
    model: ['', Validators.required],
    year: [new Date().getFullYear(), [Validators.required, Validators.min(1950)]],
    licensePlate: ['', Validators.required],
    vin: [''],
    color: ['', Validators.required],
    currentMileage: [0, [Validators.required, Validators.min(0)]],
    notes: [''],
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      return;
    }

    this.id.set(id);
    this.vehiclesApi.getById(id).subscribe({
      next: (vehicle) => {
        this.form.patchValue({
          brand: vehicle.brand,
          model: vehicle.model,
          year: vehicle.year,
          licensePlate: vehicle.licensePlate,
          vin: vehicle.vin ?? '',
          color: vehicle.color,
          currentMileage: vehicle.currentMileage,
          notes: vehicle.notes ?? '',
        });
        this.savedPhotos.set(vehicle.photos ?? []);
      },
      error: (err) => this.error.set(readApiError(err)),
    });
  }

  ngOnDestroy(): void {
    for (const photo of this.pendingPhotos()) {
      URL.revokeObjectURL(photo.preview);
    }
  }

  protected onFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files ?? []);
    input.value = '';

    const remaining = 8 - this.savedPhotos().length - this.pendingPhotos().length;
    if (remaining <= 0) {
      this.error.set('Puedes agregar un máximo de 8 fotos por vehículo.');
      return;
    }

    const accepted: PendingPhoto[] = [];
    for (const file of files.slice(0, remaining)) {
      if (!file.type.startsWith('image/')) {
        this.error.set('Solo se permiten imágenes JPG, PNG o WEBP.');
        continue;
      }
      if (file.size > 5 * 1024 * 1024) {
        this.error.set('Cada foto debe pesar máximo 5 MB.');
        continue;
      }
      accepted.push({
        id: crypto.randomUUID(),
        file,
        preview: URL.createObjectURL(file),
      });
    }

    if (accepted.length > 0) {
      this.pendingPhotos.update((current) => [...current, ...accepted]);
    }
  }

  protected removePending(id: string): void {
    const photo = this.pendingPhotos().find((item) => item.id === id);
    if (photo) {
      URL.revokeObjectURL(photo.preview);
    }
    this.pendingPhotos.update((current) => current.filter((item) => item.id !== id));
  }

  protected removeSaved(photoId: string): void {
    const vehicleId = this.id();
    if (!vehicleId) {
      return;
    }

    this.vehiclesApi.deletePhoto(vehicleId, photoId).subscribe({
      next: () => this.savedPhotos.update((current) => current.filter((item) => item.id !== photoId)),
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
      ...value,
      vin: value.vin || null,
      notes: value.notes || null,
    };

    const request = this.id()
      ? this.vehiclesApi.update(this.id()!, payload)
      : this.vehiclesApi.create(payload);

    request
      .pipe(
        switchMap((vehicle) => {
          this.id.set(vehicle.id);
          const uploads = this.pendingPhotos().map((photo) => this.vehiclesApi.uploadPhoto(vehicle.id, photo.file));
          if (uploads.length === 0) {
            return of({ vehicle, photoError: null as unknown });
          }

          return forkJoin(uploads).pipe(
            map(() => ({ vehicle, photoError: null as unknown })),
            catchError((err: unknown) => of({ vehicle, photoError: err })),
          );
        }),
      )
      .subscribe({
        next: ({ vehicle, photoError }) => {
          if (photoError) {
            this.loading.set(false);
            this.error.set('El vehículo se guardó, pero no se pudieron subir las fotos. Intenta de nuevo.');
            this.vehiclesApi.getById(vehicle.id).subscribe({
              next: (fresh) => this.savedPhotos.set(fresh.photos ?? []),
            });
            return;
          }

          void this.router.navigate(['/vehicles', vehicle.id]);
        },
        error: (err) => {
          this.loading.set(false);
          this.error.set(readApiError(err));
        },
      });
  }
}

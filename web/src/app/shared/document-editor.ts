import { Component, inject, input, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { readApiError } from '../core/http-error';
import { DocumentKind, VehicleDocument } from '../core/models';
import { VehicleService } from '../core/vehicle.service';

@Component({
  selector: 'app-document-editor',
  imports: [ReactiveFormsModule],
  templateUrl: './document-editor.html',
})
export class DocumentEditor {
  private readonly fb = inject(FormBuilder);
  protected readonly vehiclesApi = inject(VehicleService);

  readonly vehicleId = input.required<string>();
  readonly saved = output<VehicleDocument>();
  readonly cancelled = output<void>();

  protected readonly loading = signal(false);
  protected readonly error = signal('');
  protected readonly fileName = signal('');
  private selectedFile: File | null = null;

  protected readonly kinds: DocumentKind[] = [
    'InsurancePolicy',
    'Tenencia',
    'CirculationCard',
    'TechnicalInspection',
    'Invoice',
    'Other',
  ];

  protected readonly form = this.fb.nonNullable.group({
    title: ['', Validators.required],
    kind: ['InsurancePolicy' as DocumentKind, Validators.required],
    issuedOn: [''],
    expiresOn: [''],
    notes: [''],
  });

  protected onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    this.selectedFile = file;
    this.fileName.set(file?.name ?? '');
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    if (!this.selectedFile) {
      this.error.set('Selecciona un archivo PDF o una imagen.');
      return;
    }

    if (this.selectedFile.size > 10 * 1024 * 1024) {
      this.error.set('Cada documento debe pesar máximo 10 MB.');
      return;
    }

    this.loading.set(true);
    this.error.set('');
    const value = this.form.getRawValue();

    this.vehiclesApi
      .uploadDocument(this.vehicleId(), {
        file: this.selectedFile,
        title: value.title,
        kind: value.kind,
        notes: value.notes || null,
        issuedOn: value.issuedOn || null,
        expiresOn: value.expiresOn || null,
      })
      .subscribe({
        next: (document) => this.saved.emit(document),
        error: (err) => {
          this.loading.set(false);
          this.error.set(readApiError(err));
        },
      });
  }
}

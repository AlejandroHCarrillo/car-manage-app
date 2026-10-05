import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { readApiError } from '../../core/http-error';
import { Procedure, ProcedureCompletion } from '../../core/models';
import { ProcedureService } from '../../core/procedure.service';

@Component({
  selector: 'app-procedure-detail',
  imports: [RouterLink, ReactiveFormsModule, CurrencyPipe, DatePipe],
  templateUrl: './procedure-detail.html',
})
export class ProcedureDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  protected readonly api = inject(ProcedureService);

  protected readonly item = signal<Procedure | null>(null);
  protected readonly history = signal<ProcedureCompletion[]>([]);
  protected readonly error = signal('');
  protected readonly completing = signal(false);

  protected readonly completeForm = this.fb.nonNullable.group({
    completedAt: [new Date().toISOString().slice(0, 10), Validators.required],
    cost: [0, [Validators.required, Validators.min(0)]],
    notes: [''],
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.load(id);
    }
  }

  protected complete(): void {
    const current = this.item();
    if (!current || this.completeForm.invalid) {
      this.completeForm.markAllAsTouched();
      return;
    }

    this.completing.set(true);
    const value = this.completeForm.getRawValue();
    this.api
      .complete(current.id, {
        completedAt: value.completedAt,
        cost: value.cost,
        notes: value.notes || null,
      })
      .subscribe({
        next: () => {
          this.completing.set(false);
          this.completeForm.reset({
            completedAt: new Date().toISOString().slice(0, 10),
            cost: 0,
            notes: '',
          });
          this.load(current.id);
        },
        error: (err) => {
          this.completing.set(false);
          this.error.set(readApiError(err));
        },
      });
  }

  protected remove(): void {
    const current = this.item();
    if (!current || !confirm('¿Eliminar este trámite?')) {
      return;
    }

    this.api.delete(current.id).subscribe({
      next: () => void this.router.navigateByUrl('/procedures'),
      error: (err) => this.error.set(readApiError(err)),
    });
  }

  private load(id: string): void {
    this.api.getById(id).subscribe({
      next: (item) => this.item.set(item),
      error: (err) => this.error.set(readApiError(err)),
    });
    this.api.getHistory(id).subscribe({
      next: (history) => this.history.set(history),
      error: (err) => this.error.set(readApiError(err)),
    });
  }
}

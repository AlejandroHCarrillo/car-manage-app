import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Procedure } from '../../core/models';
import { ProcedureEditor } from '../../shared/procedure-editor';

@Component({
  selector: 'app-procedure-form',
  imports: [ProcedureEditor],
  templateUrl: './procedure-form.html',
})
export class ProcedureForm implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly procedureId = signal<string | null>(null);
  protected readonly vehicleId = signal<string | null>(null);

  ngOnInit(): void {
    this.procedureId.set(this.route.snapshot.paramMap.get('id'));
    this.vehicleId.set(this.route.snapshot.queryParamMap.get('vehicleId'));
  }

  protected onSaved(item: Procedure): void {
    void this.router.navigate(['/procedures', item.id]);
  }

  protected onCancelled(): void {
    void this.router.navigateByUrl('/procedures');
  }
}

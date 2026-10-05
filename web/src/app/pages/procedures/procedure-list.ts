import { DatePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Procedure } from '../../core/models';
import { ProcedureService } from '../../core/procedure.service';

@Component({
  selector: 'app-procedure-list',
  imports: [RouterLink, DatePipe],
  templateUrl: './procedure-list.html',
})
export class ProcedureList implements OnInit {
  protected readonly api = inject(ProcedureService);
  protected readonly procedures = signal<Procedure[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal('');
  protected readonly filter = signal<'all' | 'open' | 'overdue' | 'completed'>('open');

  protected readonly visible = computed(() => {
    const items = this.procedures();
    switch (this.filter()) {
      case 'overdue':
        return items.filter((item) => item.status === 'Overdue');
      case 'completed':
        return items.filter((item) => item.isCompleted);
      case 'open':
        return items.filter((item) => !item.isCompleted);
      default:
        return items;
    }
  });

  ngOnInit(): void {
    this.api.getAll().subscribe({
      next: (items) => {
        this.procedures.set(items);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudieron cargar los trámites.');
        this.loading.set(false);
      },
    });
  }
}

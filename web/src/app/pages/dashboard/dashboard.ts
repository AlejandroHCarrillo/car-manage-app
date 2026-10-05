import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';
import { DashboardStats, Vehicle } from '../../core/models';
import { VehicleService } from '../../core/vehicle.service';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink, CurrencyPipe, DecimalPipe],
  templateUrl: './dashboard.html',
})
export class Dashboard implements OnInit {
  private readonly vehiclesApi = inject(VehicleService);
  protected readonly auth = inject(AuthService);

  protected readonly stats = signal<DashboardStats | null>(null);
  protected readonly vehicles = signal<Vehicle[]>([]);
  protected readonly loading = signal(true);

  ngOnInit(): void {
    this.vehiclesApi.dashboard().subscribe((stats) => this.stats.set(stats));
    this.vehiclesApi.getAll().subscribe({
      next: (vehicles) => {
        this.vehicles.set(vehicles.slice(0, 5));
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}

import { DecimalPipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';
import { Vehicle } from '../../core/models';
import { VehicleService } from '../../core/vehicle.service';

@Component({
  selector: 'app-vehicle-list',
  imports: [RouterLink, DecimalPipe],
  templateUrl: './vehicle-list.html',
})
export class VehicleList implements OnInit {
  protected readonly vehiclesApi = inject(VehicleService);
  protected readonly auth = inject(AuthService);
  protected readonly vehicles = signal<Vehicle[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal('');

  ngOnInit(): void {
    this.vehiclesApi.getAll().subscribe({
      next: (vehicles) => {
        this.vehicles.set(vehicles);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudieron cargar los vehículos.');
        this.loading.set(false);
      },
    });
  }
}

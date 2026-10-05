import { Routes } from '@angular/router';
import { adminGuard, authGuard, guestGuard } from './core/auth.guard';
import { Shell } from './layout/shell';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login').then((m) => m.Login),
    canActivate: [guestGuard],
  },
  {
    path: '',
    component: Shell,
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () => import('./pages/dashboard/dashboard').then((m) => m.Dashboard),
      },
      {
        path: 'vehicles',
        loadComponent: () => import('./pages/vehicles/vehicle-list').then((m) => m.VehicleList),
      },
      {
        path: 'vehicles/new',
        loadComponent: () => import('./pages/vehicles/vehicle-form').then((m) => m.VehicleForm),
      },
      {
        path: 'vehicles/:id',
        loadComponent: () => import('./pages/vehicles/vehicle-detail').then((m) => m.VehicleDetail),
      },
      {
        path: 'vehicles/:id/edit',
        loadComponent: () => import('./pages/vehicles/vehicle-form').then((m) => m.VehicleForm),
      },
      {
        path: 'procedures',
        loadComponent: () => import('./pages/procedures/procedure-list').then((m) => m.ProcedureList),
      },
      {
        path: 'procedures/new',
        loadComponent: () => import('./pages/procedures/procedure-form').then((m) => m.ProcedureForm),
      },
      {
        path: 'procedures/:id',
        loadComponent: () => import('./pages/procedures/procedure-detail').then((m) => m.ProcedureDetail),
      },
      {
        path: 'procedures/:id/edit',
        loadComponent: () => import('./pages/procedures/procedure-form').then((m) => m.ProcedureForm),
      },
      {
        path: 'users',
        canActivate: [adminGuard],
        loadComponent: () => import('./pages/users/user-list').then((m) => m.UserList),
      },
      {
        path: 'users/new',
        canActivate: [adminGuard],
        loadComponent: () => import('./pages/users/user-form').then((m) => m.UserForm),
      },
      {
        path: 'users/:id/edit',
        canActivate: [adminGuard],
        loadComponent: () => import('./pages/users/user-form').then((m) => m.UserForm),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];

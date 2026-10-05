import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthResponse, CurrentUser, UserRole } from './models';

const TOKEN_KEY = 'pcg.token';
const USER_KEY = 'pcg.user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly userSignal = signal<CurrentUser | null>(this.readUser());
  readonly user = this.userSignal.asReadonly();
  readonly isAuthenticated = computed(() => !!this.userSignal());
  readonly isAdmin = computed(() => this.userSignal()?.role === 'Admin');

  token(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  roleLabel(role?: UserRole | null): string {
    return role === 'Admin' ? 'Administrador' : 'Gestor de vehículos';
  }

  login(email: string, password: string) {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/login`, { email, password }).pipe(
      tap((response) => {
        localStorage.setItem(TOKEN_KEY, response.token);
        const current: CurrentUser = {
          id: response.userId,
          email: response.email,
          firstName: response.firstName,
          lastName: response.lastName,
          role: response.role,
          isActive: true,
        };
        localStorage.setItem(USER_KEY, JSON.stringify(current));
        this.userSignal.set(current);
      }),
    );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.userSignal.set(null);
    void this.router.navigateByUrl('/login');
  }

  private readUser(): CurrentUser | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as CurrentUser;
    } catch {
      return null;
    }
  }
}

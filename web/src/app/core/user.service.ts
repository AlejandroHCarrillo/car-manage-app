import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { User, UserRole } from './models';

export interface CreateUserPayload {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  role: UserRole;
}

export interface UpdateUserPayload {
  firstName: string;
  lastName: string;
  role: UserRole;
  isActive: boolean;
  password?: string | null;
}

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/users`;

  getAll() {
    return this.http.get<User[]>(this.base);
  }

  getById(id: string) {
    return this.http.get<User>(`${this.base}/${id}`);
  }

  create(payload: CreateUserPayload) {
    return this.http.post<User>(this.base, payload);
  }

  update(id: string, payload: UpdateUserPayload) {
    return this.http.put<User>(`${this.base}/${id}`, payload);
  }

  delete(id: string) {
    return this.http.delete(`${this.base}/${id}`);
  }
}

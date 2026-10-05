import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';
import { readApiError } from '../../core/http-error';
import { User } from '../../core/models';
import { UserService } from '../../core/user.service';

@Component({
  selector: 'app-user-list',
  imports: [RouterLink, DatePipe],
  templateUrl: './user-list.html',
})
export class UserList implements OnInit {
  private readonly usersApi = inject(UserService);
  protected readonly auth = inject(AuthService);
  protected readonly users = signal<User[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal('');

  ngOnInit(): void {
    this.load();
  }

  protected remove(user: User): void {
    if (!confirm(`¿Eliminar a ${user.firstName} ${user.lastName}?`)) {
      return;
    }

    this.usersApi.delete(user.id).subscribe({
      next: () => this.load(),
      error: (err) => this.error.set(readApiError(err)),
    });
  }

  private load(): void {
    this.usersApi.getAll().subscribe({
      next: (users) => {
        this.users.set(users);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(readApiError(err));
        this.loading.set(false);
      },
    });
  }
}

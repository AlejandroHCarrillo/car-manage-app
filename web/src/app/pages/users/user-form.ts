import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { readApiError } from '../../core/http-error';
import { UserRole } from '../../core/models';
import { UserService } from '../../core/user.service';

@Component({
  selector: 'app-user-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './user-form.html',
})
export class UserForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly usersApi = inject(UserService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly id = signal<string | null>(null);
  protected readonly loading = signal(false);
  protected readonly error = signal('');
  protected readonly isEdit = () => !!this.id();

  protected readonly form = this.fb.nonNullable.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: [''],
    role: ['VehicleManager' as UserRole, Validators.required],
    isActive: [true],
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.form.controls.password.addValidators([Validators.required, Validators.minLength(8)]);
      return;
    }

    this.id.set(id);
    this.usersApi.getById(id).subscribe({
      next: (user) => {
        this.form.patchValue({
          firstName: user.firstName,
          lastName: user.lastName,
          email: user.email,
          role: user.role,
          isActive: user.isActive,
        });
        this.form.controls.email.disable();
      },
      error: (err) => this.error.set(readApiError(err)),
    });
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set('');
    const value = this.form.getRawValue();

    const request = this.id()
      ? this.usersApi.update(this.id()!, {
          firstName: value.firstName,
          lastName: value.lastName,
          role: value.role,
          isActive: value.isActive,
          password: value.password || null,
        })
      : this.usersApi.create({
          firstName: value.firstName,
          lastName: value.lastName,
          email: value.email,
          password: value.password,
          role: value.role,
        });

    request.subscribe({
      next: () => void this.router.navigateByUrl('/users'),
      error: (err) => {
        this.loading.set(false);
        this.error.set(readApiError(err));
      },
    });
  }
}

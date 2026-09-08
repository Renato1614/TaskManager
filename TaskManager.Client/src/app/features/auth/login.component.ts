import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  standalone: true,
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <section class="auth-panel">
      <div>
        <p class="eyebrow">Welcome back</p>
        <h1>Sign in</h1>
      </div>
      <form [formGroup]="form" (ngSubmit)="submit()" class="form">
        <label>Email<input type="email" formControlName="email" autocomplete="email"></label>
        @if (form.controls.email.touched && form.controls.email.invalid) {
          <small>Email is required and must be valid.</small>
        }
        <label>Password<input type="password" formControlName="password" autocomplete="current-password"></label>
        @if (form.controls.password.touched && form.controls.password.invalid) {
          <small>Password is required.</small>
        }
        @if (error()) {
          <p class="error">{{ error() }}</p>
        }
        <button type="submit" [disabled]="form.invalid || loading()">{{ loading() ? 'Signing in...' : 'Login' }}</button>
        <a routerLink="/register">Create an account</a>
      </form>
    </section>
  `
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly loading = signal(false);
  protected readonly error = signal('');
  protected readonly form = this.fb.nonNullable.group({
    email: ['demo@taskmanager.com', [Validators.required, Validators.email]],
    password: ['Demo@123', Validators.required]
  });

  submit(): void {
    if (this.form.invalid) {
      return;
    }

    this.loading.set(true);
    this.error.set('');
    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => void this.router.navigateByUrl('/tasks'),
      error: () => {
        this.error.set('Unable to sign in with those credentials.');
        this.loading.set(false);
      }
    });
  }
}

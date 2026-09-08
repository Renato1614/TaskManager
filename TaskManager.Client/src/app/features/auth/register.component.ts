import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  standalone: true,
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <section class="auth-panel">
      <div>
        <p class="eyebrow">Start clean</p>
        <h1>Create account</h1>
      </div>
      <form [formGroup]="form" (ngSubmit)="submit()" class="form">
        <label>Name<input formControlName="name" autocomplete="name"></label>
        <label>Email<input type="email" formControlName="email" autocomplete="email"></label>
        <label>Password<input type="password" formControlName="password" autocomplete="new-password"></label>
        @if (form.touched && form.invalid) {
          <small>Use a name, valid email, and password with at least 8 characters.</small>
        }
        @if (error()) {
          <p class="error">{{ error() }}</p>
        }
        <button type="submit" [disabled]="form.invalid || loading()">{{ loading() ? 'Creating...' : 'Register' }}</button>
        <a routerLink="/login">Already have an account?</a>
      </form>
    </section>
  `
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly loading = signal(false);
  protected readonly error = signal('');
  protected readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]]
  });

  submit(): void {
    if (this.form.invalid) {
      return;
    }

    this.loading.set(true);
    this.error.set('');
    this.auth.register(this.form.getRawValue()).subscribe({
      next: () => void this.router.navigateByUrl('/tasks'),
      error: () => {
        this.error.set('Unable to register this account.');
        this.loading.set(false);
      }
    });
  }
}

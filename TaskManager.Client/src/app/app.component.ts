import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink],
  template: `
    <div class="shell">
      <header class="topbar">
        <a routerLink="/tasks" class="brand">
          <span class="brand-mark">✓</span>
          <span>TaskManager</span>
        </a>
        @if (auth.isAuthenticated()) {
          <button type="button" class="ghost" (click)="logout()">Logout</button>
        }
      </header>
      <main>
        <router-outlet />
      </main>
    </div>
  `
})
export class AppComponent {
  protected readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  logout(): void {
    this.auth.logout();
    void this.router.navigateByUrl('/login');
  }
}

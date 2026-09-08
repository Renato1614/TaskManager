import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { tap } from 'rxjs';
import { AuthResponse, LoginRequest, RegisterRequest, UserResponse } from '../../shared/models/auth.models';
import { environment } from '../../../environments/environment';

const tokenKey = 'taskmanager.token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly baseUrl = `${environment.apiUrl}/api/auth`;
  private readonly tokenState = signal<string | null>(localStorage.getItem(tokenKey));

  constructor(private readonly http: HttpClient) {}

  token(): string | null {
    return this.tokenState();
  }

  isAuthenticated(): boolean {
    return !!this.tokenState();
  }

  login(request: LoginRequest) {
    return this.http.post<AuthResponse>(`${this.baseUrl}/login`, request).pipe(tap((response) => this.setToken(response.token)));
  }

  register(request: RegisterRequest) {
    return this.http.post<AuthResponse>(`${this.baseUrl}/register`, request).pipe(tap((response) => this.setToken(response.token)));
  }

  me() {
    return this.http.get<UserResponse>(`${this.baseUrl}/me`);
  }

  logout(): void {
    localStorage.removeItem(tokenKey);
    this.tokenState.set(null);
  }

  private setToken(token: string): void {
    localStorage.setItem(tokenKey, token);
    this.tokenState.set(token);
  }
}

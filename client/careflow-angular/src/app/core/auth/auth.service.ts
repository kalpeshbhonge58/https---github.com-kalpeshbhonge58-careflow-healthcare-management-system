import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, map, tap } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { ApiResponse, AuthUser, LoginRequest, LoginResponse } from './auth.models';

const TOKEN_KEY = 'careflow_token';
const USER_KEY = 'careflow_user';
const EXPIRATION_KEY = 'careflow_token_expiration';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly currentUserSubject = new BehaviorSubject<AuthUser | null>(this.readUser());
  readonly currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private readonly http: HttpClient,
    private readonly router: Router
  ) {}

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<ApiResponse<LoginResponse>>(`${environment.apiUrl}/auth/login`, credentials)
      .pipe(
        map(response => {
          if (!response.success || !response.data?.token || !response.data.user) {
            throw new Error(response.message || 'Login failed.');
          }
          return response.data;
        }),
        tap(loginResponse => this.storeAuthentication(loginResponse))
      );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    localStorage.removeItem(EXPIRATION_KEY);
    this.currentUserSubject.next(null);
    void this.router.navigate(['/login']);
  }

  getToken(): string | null {
    const token = localStorage.getItem(TOKEN_KEY);
    return token && !this.isExpired(token) ? token : null;
  }

  isAuthenticated(): boolean {
    return this.getToken() !== null && this.currentUserSubject.value !== null;
  }

  getCurrentUser(): AuthUser | null {
    return this.currentUserSubject.value;
  }

  hasAnyRole(roles: string[]): boolean {
    const userRoles = this.getCurrentUser()?.roles ?? [];
    return roles.some(role => userRoles.some(userRole => userRole.toLowerCase() === role.toLowerCase()));
  }

  getDashboardRoute(): string {
    const roles = this.getCurrentUser()?.roles ?? [];
    const routeByRole: Record<string, string> = {
      admin: '/admin/dashboard',
      doctor: '/doctor/dashboard',
      receptionist: '/receptionist/dashboard',
      patient: '/patient/dashboard'
    };

    for (const role of roles) {
      const route = routeByRole[role.toLowerCase()];
      if (route) {
        return route;
      }
    }
    return '/dashboard';
  }

  clearAuthentication(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    localStorage.removeItem(EXPIRATION_KEY);
    this.currentUserSubject.next(null);
  }

  private storeAuthentication(loginResponse: LoginResponse): void {
    localStorage.setItem(TOKEN_KEY, loginResponse.token);
    localStorage.setItem(USER_KEY, JSON.stringify(loginResponse.user));
    localStorage.setItem(EXPIRATION_KEY, loginResponse.expiresAt);
    this.currentUserSubject.next(loginResponse.user);
  }

  private readUser(): AuthUser | null {
    const serializedUser = localStorage.getItem(USER_KEY);
    if (!serializedUser || !this.getStoredToken()) {
      return null;
    }

    try {
      return JSON.parse(serializedUser) as AuthUser;
    } catch {
      this.clearStoredValues();
      return null;
    }
  }

  private getStoredToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  private isExpired(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1])) as { exp?: number };
      return typeof payload.exp === 'number' && payload.exp * 1000 <= Date.now();
    } catch {
      return true;
    }
  }

  private clearStoredValues(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    localStorage.removeItem(EXPIRATION_KEY);
  }
}

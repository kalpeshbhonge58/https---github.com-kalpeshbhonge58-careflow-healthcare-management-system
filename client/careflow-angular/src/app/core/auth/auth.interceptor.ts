import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const isLoginRequest = request.url.replace(/\/$/, '').endsWith('/auth/login');
  const token = authService.getToken();
  const requestWithAuth = !isLoginRequest && token
    ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : request;

  return next(requestWithAuth).pipe(
    catchError(error => {
      if (error.status === 401 && !isLoginRequest) {
        authService.clearAuthentication();
        void router.navigate(['/login'], { queryParams: { reason: 'session-expired' } });
      } else if (error.status === 403 && !request.url.includes('/unauthorized')) {
        void router.navigate(['/unauthorized'], {
          state: { message: 'You do not have permission to access this resource.' }
        });
      }
      return throwError(() => error);
    })
  );
};

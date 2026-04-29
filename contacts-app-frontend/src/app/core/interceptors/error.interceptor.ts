import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import { AuthService } from '../services/auth.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const authService = inject(AuthService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      switch (error.status) {
        case 401:
          authService.logout();
          router.navigate(['/auth/login']);
          break;
        case 403:
          router.navigate(['/forbidden']);
          break;
        case 404:
          break;
        case 500:
          if (!environment.production) {
            console.error('Server error occurred. Please try again later.');
          }
          break;
        default:
          if (!environment.production) {
            console.error('An unexpected error occurred.');
          }
          break;
      }

      return throwError(() => error);
    })
  );
};

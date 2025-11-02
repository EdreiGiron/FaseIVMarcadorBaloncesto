import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../servicios/auth.service';
import { catchError, switchMap } from 'rxjs/operators';
import { throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const access = auth.getAccessToken();
  if (access) {
    req = req.clone({ setHeaders: { Authorization: `Bearer ${access}` } });
  }

  return next(req).pipe(
    catchError(err => {
      if (err.status === 401 && auth.getRefreshToken()) {
        // Intentar refresh
        return auth.refreshToken().pipe(
          switchMap(newAccess => {
            if (!newAccess) {
              auth.clearTokens();
              router.navigate(['/login']);
              return throwError(() => err);
            }
            const retry = req.clone({
              setHeaders: { Authorization: `Bearer ${newAccess}` }
            });
            return next(retry);
          }),
          catchError(refreshErr => {
            auth.clearTokens();
            router.navigate(['/login']);
            return throwError(() => refreshErr);
          })
        );
      }
      return throwError(() => err);
    })
  );
};

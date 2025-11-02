import { CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../servicios/auth.service';
import { Router } from '@angular/router';
import { map, catchError } from 'rxjs/operators';



export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.isAuthenticatedAsync().pipe(
    map(valid => {
      if (!valid) {
        router.navigate(['/login']);
        return false;
      }
      return true;
    })
  );
};

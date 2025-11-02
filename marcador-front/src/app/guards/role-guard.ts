// guards/role-guard.ts
import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, Router } from '@angular/router';
import { AuthService } from '../servicios/auth.service';

@Injectable({ providedIn: 'root' })
export class RoleGuard implements CanActivate {
  constructor(private auth: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    const allowed = ((route.data?.['roles'] as string[] | undefined) ?? []).map(x => x.toLowerCase());
    if (!allowed.length) return true; // sin roles => pasa
    const role = (this.auth.getRole() || '').trim().toLowerCase(); // debe devolver 'Admin' o 'User'
    if (role && allowed.includes(role)) return true;
    this.router.navigate(['/forbidden']);
    return false;
  }
}

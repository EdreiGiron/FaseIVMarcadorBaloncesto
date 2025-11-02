import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Login as LoginDto } from '../modelos/dto/login';
import { LoginResponse } from '../modelos/dto/login-response';
import { HttpClient } from '@angular/common/http';
import { Global } from './global';
import { Observable } from 'rxjs';
import { RegisterResponseDto } from '../modelos/dto/register-response-dto';
import { map, catchError, tap } from 'rxjs/operators';
import { of } from 'rxjs';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly ACCESS_KEY = 'access_token';
  private readonly REFRESH_KEY = 'refresh_token';
  private readonly USER_KEY = 'auth_user';
  private readonly api = Global.url; //

  private isLoggedInSubject = new BehaviorSubject<boolean>(this.hasAccessToken());
  isLoggedIn$ = this.isLoggedInSubject.asObservable();

  constructor(private router: Router, private http: HttpClient) { }

  // ---------- Helpers de entorno / storage ----------
  private isBrowser(): boolean {
    return typeof window !== 'undefined' && typeof localStorage !== 'undefined';
  }

  private hasAccessToken(): boolean {
    return this.isBrowser() && !!localStorage.getItem(this.ACCESS_KEY);
  }

  getAccessToken(): string | null {
    return this.isBrowser() ? localStorage.getItem(this.ACCESS_KEY) : null;
  }

  getRefreshToken(): string | null {
    return this.isBrowser() ? localStorage.getItem(this.REFRESH_KEY) : null;
  }

  setTokens(access: string, refresh: string) {
    localStorage.setItem(this.ACCESS_KEY, access);
    localStorage.setItem(this.REFRESH_KEY, refresh);
    this.isLoggedInSubject.next(true);
  }

  clearTokens() {
    localStorage.removeItem(this.ACCESS_KEY);
    localStorage.removeItem(this.REFRESH_KEY);
    this.isLoggedInSubject.next(false);
  }

  private setUser(username: string, role?: string) {
    localStorage.setItem(this.USER_KEY, JSON.stringify({ username, role }));
  }

  private getUser(): { username: string; role?: string } | null {
    const raw = this.isBrowser() ? localStorage.getItem(this.USER_KEY) : null;
    return raw ? JSON.parse(raw) : null;
  }

  // ----------------- Auth público -------------------
  login(dto: LoginDto): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.api}/auth/login`, dto)
      .pipe(
        tap(res => {
          // guarda tokens + usuario
          this.setTokens(res.token, res.refreshToken);
          this.setUser(res.username, res.role?.name);
        })
      );
  }

  saveLoginData(res: LoginResponse): void {
    // guarda tokens + usuario usando las llaves actuales
    this.setTokens(res.token, res.refreshToken);
    this.setUser(res.username, res.role?.name);
  }

  logout(): void {
    // Notificar al backend (opcional)
    this.http.post(`${this.api}/auth/logout`, {}).pipe(
      catchError(() => of(null))
    ).subscribe();

    this.clearTokens();
    localStorage.removeItem(this.USER_KEY);
    this.router.navigate(['/login']);
  }

  isAuthenticated(): boolean {
    return this.hasAccessToken();
  }

  isAuthenticatedAsync(): Observable<boolean> {
    return this.validateToken().pipe(
      map(r => !!r?.valid),
      catchError(() => of(false))
    );
  }

  validateToken(): Observable<{ valid: boolean }> {
    return this.http.get<{ valid: boolean }>(`${this.api}/auth/validate`);
  }

  register(data: { username: string; password: string; roleId: number }) {
    return this.http.post<RegisterResponseDto>(`${this.api}/auth/register`, data);
  }

  getUsername(): string | null {
    return this.getUser()?.username ?? null;
  }
  getRole(): string | null {
    const raw = localStorage.getItem(this.USER_KEY);
    if (!raw) return null;

    try {
      const u = JSON.parse(raw) as { username: string; role?: string };
      const r = (u.role ?? '').toString().trim().toLowerCase();

      if (['admin', 'administrador'].includes(r)) return 'Admin';
      if (['user', 'usuario'].includes(r)) return 'User';

      return u.role ?? null; // fallback
    } catch {
      return null;
    }
  }
  getRoles(): Observable<{ id: number; name: string }[]> {
    return this.http.get<{ id: number; name: string }[]>(`${Global.url}/roles`);
  }

  // ------------- Refresh token (clave) --------------
  /**
   * Llama al endpoint y guarda nuevos tokens.
   * Devuelve SOLO el nuevo access token para que el interceptor pueda reintentar.
   */
  refreshToken(): Observable<string> {
    const refresh = this.getRefreshToken();
    if (!refresh) return of('');

    return this.http.post<{ token: string; refreshToken: string }>(
      `${this.api}/auth/refresh`,
      { refreshToken: refresh }
    ).pipe(
      tap(res => this.setTokens(res.token, res.refreshToken)),
      map(res => res.token)
    );
  }

  // Compatibilidad con código existente
  getToken(): string | null {
    return this.getAccessToken();
  }

  clearToken(): void {
    this.clearTokens();
  }
}

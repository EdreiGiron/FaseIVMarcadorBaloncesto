import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Login as LoginDto } from '../modelos/dto/login';
import { LoginResponse } from '../modelos/dto/login-response';
import { HttpClient } from '@angular/common/http';
import { Global } from './global';
import { Observable, of, BehaviorSubject } from 'rxjs';
import { RegisterResponseDto } from '../modelos/dto/register-response-dto';
import { map, catchError, tap, switchMap } from 'rxjs/operators';

interface MeDto { name: string; role: string; }

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly ACCESS_KEY = 'access_token';
  private readonly REFRESH_KEY = 'refresh_token';
  private readonly USER_KEY = 'auth_user';
  private readonly api = Global.url;

  private isLoggedInSubject = new BehaviorSubject<boolean>(this.hasAccessToken());
  isLoggedIn$ = this.isLoggedInSubject.asObservable();

  constructor(private router: Router, private http: HttpClient) { }

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

  setTokens(access: string, refresh: string | null) {
    if (access) localStorage.setItem(this.ACCESS_KEY, access);
    if (refresh) localStorage.setItem(this.REFRESH_KEY, refresh);
    this.isLoggedInSubject.next(true);
  }

  clearTokens() {
    localStorage.removeItem(this.ACCESS_KEY);
    localStorage.removeItem(this.REFRESH_KEY);
    this.isLoggedInSubject.next(false);
  }

  private setUser(username: string | null | undefined, role?: string | null) {
    if (!username && !role) return;
    localStorage.setItem(this.USER_KEY, JSON.stringify({ username: username ?? '', role: role ?? undefined }));
  }

  private getUser(): { username: string; role?: string } | null {
    const raw = this.isBrowser() ? localStorage.getItem(this.USER_KEY) : null;
    return raw ? JSON.parse(raw) : null;
  }

  login(dto: LoginDto): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.api}/auth/login`, dto).pipe(
      tap(res => {
        this.setTokens(res.token, res.refreshToken);
        this.setUser(res.username, res.role?.name);
      })
    );
  }

  saveLoginData(res: LoginResponse): void {
    this.setTokens(res.token, res.refreshToken);
    this.setUser(res.username, res.role?.name);
  }

  startOAuth(provider: 'google' | 'github') {
    window.location.href = `${this.api}/oauth/${provider}`;
  }


  handleOAuthCallback(url: string): void {
    const u = new URL(url);
    const params = new URLSearchParams(u.search || (u.hash.startsWith('#') ? u.hash.substring(1) : ''));

    const token = params.get('access_token') || params.get('token');
    const refresh = params.get('refresh_token') || params.get('refreshToken');
    const username = params.get('username');
    const role = params.get('role');

    if (token) this.setTokens(token, refresh);
    if (username || role) this.setUser(username, role);
  }


  logout(): void {
    const username = this.getUsername() ?? '';
    this.http.post(`${this.api}/auth/logout`, { username }, { responseType: 'text' })
      .pipe(catchError(() => of(null)))
      .subscribe(() => {
        this.clearTokens();
        localStorage.removeItem(this.USER_KEY);
        this.router.navigate(['/login']);
      });
  }

  isAuthenticated(): boolean {
    return this.hasAccessToken();
  }


  isAuthenticatedAsync(): Observable<boolean> {
    if (!this.hasAccessToken()) return of(false);
    return this.me().pipe(
      map(() => true),
      catchError(() =>
        this.refreshToken().pipe(
          switchMap(tk => tk ? this.me().pipe(map(() => true), catchError(() => of(false))) : of(false))
        )
      )
    );
  }

  private me(): Observable<MeDto> {
    return this.http.get<MeDto>(`${this.api}/me`).pipe(
      tap(m => this.setUser(m?.name ?? this.getUsername(), m?.role ?? this.getRole()))
    );
  }

  validateToken(): Observable<{ valid: boolean }> {
    return this.me().pipe(
      map(() => ({ valid: true })),
      catchError(() => of({ valid: false }))
    );
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
      return u.role ?? null;
    } catch { return null; }
  }

  getRoles(): Observable<{ id: number; name: string }[]> {
    return this.http.get<{ id: number; name: string }[]>(`${Global.url}/roles`);
  }

  refreshToken(): Observable<string> {
    const refresh = this.getRefreshToken();
    if (!refresh) return of('');
    return this.http.post<{ token: string; refreshToken: string }>(
      `${this.api}/auth/refresh`, { refreshToken: refresh }
    ).pipe(
      tap(res => this.setTokens(res.token, res.refreshToken)),
      map(res => res.token),
      catchError(() => of(''))
    );
  }

  getToken(): string | null { return this.getAccessToken(); }
  clearToken(): void { this.clearTokens(); }

  saveOAuthTokens(p: { token: string; refreshToken: string; username: string; role?: { name: string } }) {
    this.setTokens(p.token, p.refreshToken);
    this.setUser(p.username, p.role?.name);
  }

}

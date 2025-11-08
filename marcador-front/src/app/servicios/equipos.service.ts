/**
 * Servicio de equipos: listado, paginado, detalle y CRUD.
 * Soporta JSON simple o FormData (para subir logo).
 * Todas las rutas pasan por el proxy: /api/equipos
 */
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PagedResult } from '../modelos/paged';
import { EquipoAdminDto } from '../modelos/equipo-admin';

// Permitir tanto FormData como JSON simple
export type EquipoUpsert = FormData | { nombre: string; ciudad: string };

@Injectable({ providedIn: 'root' })
export class EquiposService {
  private http = inject(HttpClient);
  private base = '/api/equipos';

  // Helpers para logo
  logoUrl(file?: string | null): string | null {
    if (!file) return null;
    if (/^https?:\/\//i.test(file)) return file;
    return `${this.base}/logo/${encodeURIComponent(file)}`;
  }
  fallbackLogo(kind: 'local' | 'visita'): string {
    return `/assets/logos/${kind}.png`;
  }
  logoOrFallback(file?: string | null, kind: 'local' | 'visita' = 'local'): string {
    return this.logoUrl(file) ?? this.fallbackLogo(kind);
  }

  // ===== Listados / Detalle =====
  list(search?: string, ciudad?: string): Observable<EquipoAdminDto[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (ciudad) params = params.set('ciudad', ciudad);
    return this.http.get<EquipoAdminDto[]>(this.base, { params });
  }

  getById(id: number): Observable<EquipoAdminDto> {
    return this.http.get<EquipoAdminDto>(`${this.base}/${id}`);
  }

  listPaged(opts: {
    page: number; pageSize: number;
    search?: string; ciudad?: string;
    sortBy?: 'nombre' | 'ciudad' | 'puntos' | 'faltas';
    sortDir?: 'asc' | 'desc';
  }) {
    let params = new HttpParams()
      .set('page', opts.page)
      .set('pageSize', opts.pageSize);
    if (opts.search) params = params.set('search', opts.search);
    if (opts.ciudad) params = params.set('ciudad', opts.ciudad);
    if (opts.sortBy) params = params.set('sortBy', opts.sortBy);
    if (opts.sortDir) params = params.set('sortDir', opts.sortDir);

    return this.http.get<PagedResult<EquipoAdminDto>>(`${this.base}/paged`, { params });
  }

  // ===== CRUD (JSON o FormData) =====
  create(body: EquipoUpsert): Observable<EquipoAdminDto> {
    // Si es FormData NO setear content-type manualmente.
    return this.http.post<EquipoAdminDto>(this.base, body);
  }

  update(id: number, body: EquipoUpsert): Observable<EquipoAdminDto> {
    // Laravel maneja bien POST + _method=PUT con multipart
    // (si tu backend acepta PUT multipart directo, cambia por .put)
    return this.http.post<EquipoAdminDto>(`${this.base}/${id}?_method=PUT`, body);
    // return this.http.put<EquipoAdminDto>(`${this.base}/${id}`, body);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  // Convenience para armar FormData desde JSON + File
  toFormData(dto: { nombre: string; ciudad: string }, logoFile?: File): FormData {
    const fd = new FormData();
    fd.append('nombre', dto.nombre.trim());
    fd.append('ciudad', dto.ciudad.trim());
    if (logoFile) fd.append('logo', logoFile);
    return fd;
  }
}

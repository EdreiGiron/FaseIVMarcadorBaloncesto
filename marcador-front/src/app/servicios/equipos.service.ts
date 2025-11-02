/**
 * Servicio de equipos: listado, paginado, detalle y CRUD.
 * Maneja subida/borrado de logos mediante formularios (FormData).
 * Base URL: `Global.url`.
 */
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PagedResult } from '../modelos/paged';
import {
  EquipoAdminDto,
  EquipoCreateForm,
  EquipoUpdateForm,
} from '../modelos/equipo-admin';

@Injectable({ providedIn: 'root' })
export class EquiposService {
  private http = inject(HttpClient);
  private base = '/api/equipos'; 

  // GET /api/equipos?search=&ciudad=
  list(search?: string, ciudad?: string): Observable<EquipoAdminDto[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (ciudad) params = params.set('ciudad', ciudad);
    return this.http.get<EquipoAdminDto[]>(this.base, { params });
  }
   logoUrl(file?: string | null): string | null {
    if (!file) return null;
    if (/^https?:\/\//i.test(file)) return file; 
    return `/api/equipos/logo/${encodeURIComponent(file)}`;
  }
  fallbackLogo(kind: 'local'|'visita'): string {
  return `/assets/logos/${kind}.png`;
}

//  
logoOrFallback(file?: string | null, kind: 'local'|'visita'='local'): string {
  return this.logoUrl(file) ?? this.fallbackLogo(kind);
}

  // GET /api/equipos/:id
  getById(id: number): Observable<EquipoAdminDto> {
    return this.http.get<EquipoAdminDto>(`${this.base}/${id}`);
  }

  // POST /api/equipos  (multipart/form-data)
  create(data: EquipoCreateForm): Observable<EquipoAdminDto> {
    const form = new FormData();
    form.append('Nombre', data.nombre);
    form.append('Ciudad', data.ciudad);
    if (data.logo) form.append('Logo', data.logo);
    return this.http.post<EquipoAdminDto>(this.base, form);
  }

  // PUT /api/equipos/:id (multipart/form-data)
  update(id: number, data: EquipoUpdateForm): Observable<EquipoAdminDto> {
    const form = new FormData();
    form.append('Nombre', data.nombre);
    form.append('Ciudad', data.ciudad);
    if (data.logo) form.append('Logo', data.logo);
    return this.http.put<EquipoAdminDto>(`${this.base}/${id}`, form);
  }

  // DELETE /api/equipos/:id
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  listPaged(opts: {
  page: number; pageSize: number;
  search?: string; ciudad?: string;
  sortBy?: 'nombre'|'ciudad'|'puntos'|'faltas';
  sortDir?: 'asc'|'desc';
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
}

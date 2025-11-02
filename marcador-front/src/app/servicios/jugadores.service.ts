/**
 * Servicio de jugadores: listado, paginado, detalle y CRUD.
 * Permite filtrar por nombre, equipo (id/nombre) y posición.
 * Base URL: `Global.url`.
 */
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PagedResult } from '../modelos/paged';
import { JugadorAdminDto, JugadorCreateDto, JugadorUpdateDto } from '../modelos/jugador-admin';

@Injectable({ providedIn: 'root' })
export class JugadoresService {
  private http = inject(HttpClient);
  private base = '/api/jugadores';

  list(search?: string, equipoNombre?: string, equipoId?: number, posicion?: string): Observable<JugadorAdminDto[]> {
    let params = new HttpParams();
    if (search?.trim()) params = params.set('search', search.trim());
    if (equipoNombre?.trim()) params = params.set('equipoNombre', equipoNombre.trim());
    if (equipoId != null) params = params.set('equipoId', equipoId);
    if (posicion?.trim()) params = params.set('posicion', posicion.trim());
    return this.http.get<JugadorAdminDto[]>(this.base, { params });
  }

  listPaged(opts: {
    page: number; pageSize: number;
    search?: string; equipoNombre?: string; equipoId?: number; posicion?: string;
    sortBy?: 'nombre'|'equipo'|'posicion'|'puntos'|'faltas';
    sortDir?: 'asc'|'desc';
  }): Observable<PagedResult<JugadorAdminDto>> {
    let params = new HttpParams().set('page', opts.page).set('pageSize', opts.pageSize);
    if (opts.search) params = params.set('search', opts.search);
    if (opts.equipoNombre) params = params.set('equipoNombre', opts.equipoNombre);
    if (opts.equipoId != null) params = params.set('equipoId', opts.equipoId);
    if (opts.posicion) params = params.set('posicion', opts.posicion);
    if (opts.sortBy) params = params.set('sortBy', opts.sortBy);
    if (opts.sortDir) params = params.set('sortDir', opts.sortDir);
    return this.http.get<PagedResult<JugadorAdminDto>>(`${this.base}/paged`, { params });
  }

  getById(id: number) { return this.http.get<JugadorAdminDto>(`${this.base}/${id}`); }
  create(dto: JugadorCreateDto) { return this.http.post<JugadorAdminDto>(this.base, dto); }
  update(id: number, dto: JugadorUpdateDto) { return this.http.put<JugadorAdminDto>(`${this.base}/${id}`, dto); }
  delete(id: number) { return this.http.delete<void>(`${this.base}/${id}`); }
}

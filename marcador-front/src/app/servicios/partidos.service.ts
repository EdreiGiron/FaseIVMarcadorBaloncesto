import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';

export type PartidoEstado = 'Programado' | 'EnJuego' | 'Finalizado' | 'Pospuesto' | 'Cancelado';

export interface PartidoDto {
  id: number;
  torneoId: number;
  seriePlayoffId: number;
  gameNumber: number;
  fechaHora: string;
  estado: PartidoEstado;
  equipoLocalId: number;
  equipoVisitanteId: number;
  marcadorLocal?: number;
  marcadorVisitante?: number;
  ronda: 'Final' | 'Semifinal' | 'Cuartos' | 'Octavos';
  seedA: number;
  seedB: number;
}
export interface Paged<T> { items: T[]; total: number; page: number; pageSize: number; }

@Injectable({
  providedIn: 'root'
})
export class PartidosService {
  private http = inject(HttpClient);
  private base = '/api/Partidos/historial';

  historial(opts: {
    torneoId?: number; estado?: string; ronda?: string; equipoId?: number;
    fechaDesde?: string; fechaHasta?: string; page?: number; pageSize?: number;
  }) {
    let params = new HttpParams();
    if (opts.torneoId != null) params = params.set('torneoId', opts.torneoId);
    if (opts.estado) params = params.set('estado', opts.estado);
    if (opts.ronda) params = params.set('ronda', opts.ronda);
    if (opts.equipoId != null) params = params.set('equipoId', opts.equipoId);
    if (opts.fechaDesde) params = params.set('fechaDesde', opts.fechaDesde);
    if (opts.fechaHasta) params = params.set('fechaHasta', opts.fechaHasta);
    if (opts.page != null) params = params.set('page', opts.page);
    if (opts.pageSize != null) params = params.set('pageSize', opts.pageSize);
    return this.http.get<Paged<PartidoDto>>(`${this.base}/historial`, { params });
  }

  cerrarPartido(id: number, marcadorLocal: number, marcadorVisitante: number) {
    return this.http.put(`${this.base}/${id}/marcador`, { marcadorLocal, marcadorVisitante });
  }

  asignarRoster(id: number, equipoId: number, jugadores: { jugadorId: number; titular: boolean }[]) {
    return this.http.post(`${this.base}/${id}/roster`, { equipoId, jugadores });
  }
  
  getById(id: number) {
    return this.http.get<PartidoDto>(`${this.base}/${id}`);
  }

  
  cambiarEstado(
    id: number,
    estado: 'Programado' | 'EnJuego' | 'Finalizado' | 'Pospuesto' | 'Cancelado'
  ) {
    return this.http.put(`${this.base}/${id}/estado`, { estado });
  }

  crear(dto: { fechaHora: string; equipoLocalId: number; equipoVisitanteId: number }) {
    return this.http.post<{ id: number }>('/api/Partidos/historial', dto);
  }
}

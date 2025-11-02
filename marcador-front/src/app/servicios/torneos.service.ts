import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface CrearTorneoDto {
  nombre: string;
  temporada: number;
  bestOf: number;           // 3, 5 o 7
  equipoIdsSeed: number[];  // equipos en orden 
}
export interface TorneoDto {
  id: number;
  nombre: string;
  temporada: number;
  bestOf: number;
  estado: string;
}

@Injectable({
  providedIn: 'root'
})
export class TorneosService {
  private http = inject(HttpClient);
  private base = '/api/torneos';

  crear(dto: CrearTorneoDto) {
    return this.http.post<TorneoDto>(this.base, dto);
  }

  getById(id: number) {
    return this.http.get<TorneoDto>(`${this.base}/${id}`);
  }

  generarSiguienteRonda(id: number) {
    return this.http.post<{ message: string }>(`${this.base}/${id}/generar-siguiente-ronda`, {});
  }

  seedDemo() {
    return this.http.post<{ message: string; torneoId: number }>(`${this.base}/seed-demo`, {});
  }
}

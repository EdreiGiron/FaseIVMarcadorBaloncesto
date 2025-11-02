/**
 * Servicio del marcador/partido en tiempo real.
 *
 * Capacidades:
 * - Leer estado global y estado del tiempo (cronómetro).
 * - Sumar/restar puntos, registrar faltas.
 * - Avanzar cuartos y gestionar prórrogas.
 * - Iniciar/pausar/reanudar/reiniciar reloj.
 * - Renombrar equipos (en ficha activa o creando nueva).
 * - Finalizar/terminar partido (manual/automático) y crear nuevo.
 *
 * Base URL: `Global.url`.
 */

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { MarcadorGlobal } from '../modelos/marcador-global';

@Injectable({ providedIn: 'root' })
export class MarcadorService {
  private http = inject(HttpClient);
  private base = '/api/marcador';

  // ---- Lecturas ----
  obtenerMarcador(): Observable<MarcadorGlobal> {
    return this.http.get<MarcadorGlobal>(`${this.base}`);
  }

  obtenerEstadoTiempo(): Observable<{
    estado: 'Running' | 'Stopped' | 'Paused';
    cuartoActual: number;
    segundosRestantes: number;
    duracionCuarto: number;
  }> {
    return this.http.get<any>(`${this.base}/tiempo`).pipe(
      map(r => ({
        estado: r.estado,
        cuartoActual: r.cuartoActual,
        segundosRestantes: r.segundosRestantes,
        duracionCuarto: r.duracionCuarto
      }))
    );
  }

  // ---- Puntos ----
  sumarPuntos(quien: 'local'|'visitante', puntos: number): Observable<MarcadorGlobal> {
    return this.http.post<MarcadorGlobal>(
      `${this.base}/puntos/sumar?equipo=${quien}&puntos=${puntos}`, {}
    );
  }

  restarPuntos(quien: 'local'|'visitante', puntos: number): Observable<MarcadorGlobal> {
    return this.http.post<MarcadorGlobal>(
      `${this.base}/puntos/restar?equipo=${quien}&puntos=${puntos}`, {}
    );
  }

  // ---- Faltas ----
  registrarFalta(quien: 'local'|'visitante'): Observable<MarcadorGlobal> {
    return this.http.post<MarcadorGlobal>(`${this.base}/falta?equipo=${quien}`, {});
  }

  // ---- Cuartos ----
  avanzarCuarto(): Observable<MarcadorGlobal> {
    return this.http.post<MarcadorGlobal>(`${this.base}/cuarto/siguiente`, {});
  }

  // ---- Tiempo (mergeado: reloj/* y tiempo/*) ----
  iniciarReloj(): Observable<MarcadorGlobal> {
    return this.http.post<MarcadorGlobal>(`${this.base}/reloj/iniciar`, {});
  }
  
  pausarReloj(): Observable<MarcadorGlobal> {
    return this.http.post<MarcadorGlobal>(`${this.base}/reloj/pausar`, {});
  }

  reanudarTiempo(): Observable<MarcadorGlobal> {
    return this.http.post<MarcadorGlobal>(`${this.base}/tiempo/reanudar`, {});
  }

  reiniciarTiempo(segundos: number = 600): Observable<MarcadorGlobal> {
    return this.http.post<MarcadorGlobal>(`${this.base}/tiempo/reiniciar?seg=${segundos}`, {});
  }

  establecerTiempo(segundos: number): Observable<MarcadorGlobal> {
    return this.http.post<MarcadorGlobal>(`${this.base}/tiempo/establecer?seg=${segundos}`, {});
  }

  // ---- Equipos ----
  renombrarEquipos(local?: string, visitante?: string): Observable<MarcadorGlobal> {
    return this.http.post<MarcadorGlobal>(`${this.base}/equipos/renombrar`, {
      local,
      visitante
    });
  }

  renombrarEquiposNuevo(local?: string, visitante?: string) {
    const params = new URLSearchParams();
    if (local && local.trim()) params.set('local', local.trim());
    if (visitante && visitante.trim()) params.set('visitante', visitante.trim());
    return this.http.post<MarcadorGlobal>(`/api/marcador/equipos/renombrar-nuevo?${params.toString()}`, {});
  }

  // Nuevo partido despues de que guardo
  nuevoPartido(): Observable<MarcadorGlobal> {
    return this.http.post<MarcadorGlobal>(`/api/marcador/nuevo`, {});
  }

  //Termina antes del tiempo estipulado
  terminarPartido(motivo?: string) {
    return this.http.post<MarcadorGlobal>(`${this.base}/partido/terminar`, { motivo });
  }

  //Se guarda cuando ya termino el tiempo estipulado
  finalizarAuto() {
    return this.http.post<MarcadorGlobal>(`${this.base}/partido/finalizar-auto`, {});
  }

  // Inicializar marcador con partido específico
  inicializarConPartido(partidoId: number): Observable<MarcadorGlobal> {
    return this.http.post<MarcadorGlobal>(`${this.base}/inicializar/${partidoId}`, {});
  }

}

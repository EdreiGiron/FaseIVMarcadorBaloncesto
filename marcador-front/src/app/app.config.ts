// src/app/app.config.ts
import { ApplicationConfig, Injectable } from '@angular/core';
import { provideHttpClient, HttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { provideAnimations } from '@angular/platform-browser/animations';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(), 
    provideRouter(routes),
    provideAnimations(),
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule
  ]
};

// Servicio simple para llamar a la API (.NET)
@Injectable({ providedIn: 'root' })
export class MarcadorApi {
  constructor(private http: HttpClient) {}

  // Estado general del marcador
  getMarcador() {
    return this.http.get('/api/marcador');
  }

  // Tiempo
  iniciarTiempo() {
    return this.http.post('/api/marcador/tiempo/iniciar', {});
  }
  pausarTiempo() {
    return this.http.post('/api/marcador/tiempo/pausar', {});
  }
  reanudarTiempo() {
    return this.http.post('/api/marcador/tiempo/reanudar', {});
  }
  reiniciarTiempo(segundos?: number) {
    const url =
      segundos != null
        ? `/api/marcador/tiempo/reiniciar?seg=${segundos}`
        : '/api/marcador/tiempo/reiniciar';
    return this.http.post(url, {});
  }
  establecerTiempo(segundos: number) {
    return this.http.post(`/api/marcador/tiempo/establecer?seg=${segundos}`, {});
  }
  getTiempo() {
    return this.http.get('/api/marcador/tiempo');
  }

  // Puntos / faltas / cuartos
  sumarPuntos(equipo: 'Local' | 'Visitante', puntos: number) {
    return this.http.post(
      `/api/marcador/puntos/sumar?equipo=${equipo}&puntos=${puntos}`,
      {}
    );
  }
  restarPuntos(equipo: 'Local' | 'Visitante', puntos: number) {
    return this.http.post(
      `/api/marcador/puntos/restar?equipo=${equipo}&puntos=${puntos}`,
      {}
    );
  }
  falta(equipo: 'Local' | 'Visitante') {
    return this.http.post(`/api/marcador/falta?equipo=${equipo}`, {});
  }
  siguienteCuarto() {
    return this.http.post('/api/marcador/cuarto/siguiente', {});
  }
}

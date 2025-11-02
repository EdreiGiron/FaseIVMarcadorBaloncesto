import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { Subscription, interval } from 'rxjs';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatBadgeModule } from '@angular/material/badge';
import { MarcadorService } from '../servicios/marcador.service';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Router } from '@angular/router';


// Interfaces 
interface Equipo {
  nombre: string;
  puntos: number;
  faltas: number;
}

interface MarcadorGlobal {
  id?: number;
  equipoLocal: Equipo;
  equipoVisitante: Equipo;
  cuartoActual: number;
  tiempoRestante: number;   // en segundos
  enProrroga: boolean;
  numeroProrroga: number;
  relojCorriendo?: boolean; 
}

@Component({
  selector: 'app-tablero',
  standalone: true,
  imports: [
  CommonModule,
  MatCardModule,
  MatButtonModule,
  MatBadgeModule,
  MatIconModule,        
  MatFormFieldModule,   
  MatInputModule        
  ],
  templateUrl: './tablero.component.html',
  styleUrls: ['./tablero.component.css']
})
export class TableroComponent implements OnInit, OnDestroy {
  private svc = inject(MarcadorService);
  private router = inject(Router);

  // estado UI
  private _datos = signal<MarcadorGlobal | null>(null);
  private _error = signal<string>('');
  private _cargando = signal<boolean>(true);
  private _esperando = signal<boolean>(false);
  private _autoFinalizado = false;

  // control de tiempo local para suavizar la UI
  private _segundosLocal = signal<number>(600);
  private _ticker$?: Subscription;
  private _resync$?: Subscription;

  // bandera para auto‑avance
  autoAvanzar = true;
  // evita múltiples disparos en el mismo final
  private _yaAvanceEnEsteCero = false;

  duracionMin: number = 10;

  ngOnInit(): void {
    this.cargar();
    // tick local: 1s para que la UI baje el contador cuando el reloj está corriendo
    this._ticker$ = interval(1000).subscribe(() => {
      const d = this._datos();
      if (!d) return;
      if (d.relojCorriendo) {
        const s = this._segundosLocal();
        if (s > 0) this._segundosLocal.set(s - 1);

        const next = s - 1;

        // --- NUEVO: final automático al llegar a 0 en 4º cuarto (sin prórroga)
        if (next === 0 && !d.enProrroga && d.cuartoActual === 4) {
          if (!this._autoFinalizado) this.finalizarAuto();
          // no avanzar cuarto si se finaliza
          this._yaAvanceEnEsteCero = true;
        } else if (next === 0) {
          // cuando llega a 0, podemos auto‑avanzar (solo si no es el fin del partido)
          if (this.autoAvanzar && !this._yaAvanceEnEsteCero) {
            this._yaAvanceEnEsteCero = true;
            // dispara avanzar cuarto
            this.svc.avanzarCuarto().subscribe({
              next: (d) => {
                this._datos.set(d);
                this._segundosLocal.set(d.tiempoRestante);
                // rearmamos el flag para el siguiente final
                setTimeout(()=> this._yaAvanceEnEsteCero = false, 500);
                // rearmar final automático para el nuevo periodo
                this._autoFinalizado = false;
              },
              error: () => { /* si falla, deja el flag en true para no disparar en loop */ }
            });
          }
        }

        // si llega a 0, detenemos local y refrescamos
        if (s - 1 <= 0) {
          this.cargar(false);
        }
      } else {
        this._segundosLocal.set(d.tiempoRestante);
      }
    });
    // re-sincronización con backend cada 5s
    this._resync$ = interval(5000).subscribe(() => this.cargar(false));
  }

  ngOnDestroy(): void {
    this._ticker$?.unsubscribe();
    this._resync$?.unsubscribe();
  }


  datos() { return this._datos(); }
  error() { return this._error(); }
  cargando() { return this._cargando(); }
  esperando() { return this._esperando(); }
  corriendo() { return this._datos()?.relojCorriendo ?? false; }

  tiempoMostrado(): number {
    const d = this._datos();
    return d ? this._segundosLocal() : 0;
  }

  mmss(seg: number): string {
    const s = Math.max(0, Math.floor(seg));
    const m = Math.floor(s / 60);
    const r = s % 60;
    const mm = m.toString().padStart(2, '0');
    const ss = r.toString().padStart(2, '0');
    return `${mm}:${ss}`;
  }

  // --------- acciones ---------
  cargar(marcarCargando = true) {
    if (marcarCargando) this._cargando.set(true);
    this.svc.obtenerMarcador().subscribe({
      next: (d: MarcadorGlobal) => {
        this._datos.set(d);
        this._segundosLocal.set(d.tiempoRestante);
        if (d.tiempoRestante > 0) this._yaAvanceEnEsteCero = false;
        this._partidoTerminado = !d.relojCorriendo && d.cuartoActual >= 4 && !d.enProrroga 
                               && d.tiempoRestante === 0;
        // si ya viene en 0 y es 4º cuarto sin prórroga, dispara final auto
        if (d.tiempoRestante === 0 && !d.enProrroga && d.cuartoActual === 4 && !this._autoFinalizado) {
          this.finalizarAuto();
        }

        this._error.set('');
        this._cargando.set(false);
      },
      error: () => {
        this._error.set('No se pudo cargar el tablero');
        this._cargando.set(false);
      }
    });
  }

  iniciar() {
    this._esperando.set(true);
    this.svc.iniciarReloj().subscribe({
      next: (d: MarcadorGlobal) => {
        this._datos.set(d);
        this._segundosLocal.set(d.tiempoRestante);
        this._esperando.set(false);
      },
      error: () => { this._esperando.set(false); }
    });
  }

  pausar() {
    this._esperando.set(true);
    this.svc.pausarReloj().subscribe({
      next: (d: MarcadorGlobal) => {
        this._datos.set(d);
        this._segundosLocal.set(d.tiempoRestante);
        this._esperando.set(false);
      },
      error: () => { this._esperando.set(false); }
    });
  }

  reiniciar() {
    const seg = this.duracionMin * 60;     // usa la duración actual
    this._esperando.set(true);
    this._segundosLocal.set(seg);          // feedback inmediato en la UI
    this.svc.reiniciarTiempo(seg).subscribe({
      next: (d: MarcadorGlobal) => {
        this._datos.set(d);
        this._segundosLocal.set(d.tiempoRestante);
        this._yaAvanceEnEsteCero = false;
        this._autoFinalizado = false;      // <-- rearmar final auto
        this._esperando.set(false);
      },
      error: () => { this._esperando.set(false); }
    });
  }

  ajustar(delta: number) {
    const actual = this.tiempoMostrado();
    let nuevo = actual + delta;
    if (nuevo < 0) nuevo = 0;
    this._esperando.set(true);
    this.svc.establecerTiempo(nuevo).subscribe({
      next: (d: MarcadorGlobal) => {
        this._datos.set(d);
        this._segundosLocal.set(d.tiempoRestante);
        this._esperando.set(false);
      },
      error: () => { this._esperando.set(false); }
    });
  }

  sumar(quien: 'local'|'visitante', puntos: number) {
    this._esperando.set(true);
    this.svc.sumarPuntos(quien, puntos).subscribe({
      next: (d: MarcadorGlobal) => { this._datos.set(d); this._esperando.set(false); },
      error: () => { this._esperando.set(false); }
    });
  }

  restar(quien: 'local'|'visitante', puntos: number) {
    this._esperando.set(true);
    this.svc.restarPuntos(quien, puntos).subscribe({
      next: (d: MarcadorGlobal) => { this._datos.set(d); this._esperando.set(false); },
      error: () => { this._esperando.set(false); }
    });
  }

  falta(quien: 'local'|'visitante') {
    this._esperando.set(true);
    this.svc.registrarFalta(quien).subscribe({
      next: (d: MarcadorGlobal) => { this._datos.set(d); this._esperando.set(false); },
      error: () => { this._esperando.set(false); }
    });
  }

  siguienteCuarto() {
    this._esperando.set(true);
    this.svc.avanzarCuarto().subscribe({
      next: (d: MarcadorGlobal) => {
        this._datos.set(d);
        this._segundosLocal.set(d.tiempoRestante);
        this._autoFinalizado = false;  // <-- rearmar final auto al pasar de cuarto
        this._esperando.set(false);
      },
      error: () => { this._esperando.set(false); }
    });
  }

  abrirRenombrar() {
    const d = this._datos();
    const nuevoLocal = prompt('Nuevo nombre para el equipo local', d?.equipoLocal.nombre ?? '');
    const nuevoVisitante = prompt('Nuevo nombre para el equipo visitante', d?.equipoVisitante.nombre ?? '');
    if (nuevoLocal === null && nuevoVisitante === null) return;

    this._esperando.set(true);
    this.svc.renombrarEquiposNuevo(nuevoLocal ?? undefined, nuevoVisitante ?? undefined).subscribe({
      next: (res: MarcadorGlobal) => {
        this._datos.set(res);
        this._esperando.set(false);
      },
      error: () => { this._esperando.set(false); }
    });
  }

  renombrarCreando() {
    const d = this._datos();
    const nuevoLocal = prompt('Nuevo nombre (crea equipo NUEVO) local', d?.equipoLocal.nombre ?? '');
    const nuevoVis   = prompt('Nuevo nombre (crea equipo NUEVO) visitante', d?.equipoVisitante.nombre ?? '');
    if (nuevoLocal === null && nuevoVis === null) return;

    this._esperando.set(true);
    this.svc.renombrarEquiposNuevo(nuevoLocal ?? undefined, nuevoVis ?? undefined).subscribe({
      next: (res) => { this._datos.set(res); this._esperando.set(false); },
      error: () => this._esperando.set(false)
    });
  }

  nuevoPartido() {
    this._esperando.set(true);
    this.svc.nuevoPartido().subscribe({
      next: (d) => { 
        this._datos.set(d); 
        this._segundosLocal.set(d.tiempoRestante); 
        this._esperando.set(false); 
        this._yaAvanceEnEsteCero = false;
        this._autoFinalizado = false; 
        this._partidoTerminado = false;  // habilita el inicio
      },
      error: () => { this._esperando.set(false); }
    });
  }

  onDuracionInput(ev: Event) {
  const v = Number((ev.target as HTMLInputElement).value);
  this.duracionMin = Number.isFinite(v) ? Math.max(1, Math.min(20, v)) : 10;
  }

  aplicarDuracion() {
    const seg = this.duracionMin * 60;
    this._esperando.set(true);
    this.svc.reiniciarTiempo(seg).subscribe({
      next: (d) => {
        this._datos.set(d);
        this._segundosLocal.set(d.tiempoRestante);
        this._autoFinalizado = false; // <-- rearmar final auto
        this._esperando.set(false);
      },
      error: () => { this._esperando.set(false); }
    });
  } 
  
  onAutoChange(ev: Event) {
    this.autoAvanzar = (ev.target as HTMLInputElement).checked;
  }

  finalizarAuto() {
    if (this._autoFinalizado) return; // evitar duplicados
    this._autoFinalizado = true;

    this._esperando.set(true);
    this.svc.finalizarAuto().subscribe({
      next: () => {
        this._partidoTerminado = true;
        this.cargar(); // refresca datos del marcador
        this._esperando.set(false);
      },
      error: () => {
        this._esperando.set(false);
        this._autoFinalizado = false; // permitir reintento si falló
      }
    });
  }
  
  private _partidoTerminado = false;

  partidoTerminado() { 
    return this._partidoTerminado; 
  } 
  onTerminar() {
    const motivo = prompt('Motivo (opcional):', '');
    this._esperando.set(true);
    this.svc.terminarPartido(motivo || undefined).subscribe({
      next: (d: MarcadorGlobal) => {
        this._datos.set(d);
        this._segundosLocal.set(d.tiempoRestante); // ya viene pausado
        this._autoFinalizado = true; // ya quedó finalizado 
        this._esperando.set(false);
        this._partidoTerminado = true;   // bloquea botones
      },
      error: () => this._esperando.set(false)
    });
  }

  goBack() {
    this.router.navigate(['/dashboard']);
  }
}

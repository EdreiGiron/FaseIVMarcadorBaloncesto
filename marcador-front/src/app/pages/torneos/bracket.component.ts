import { Component, Input, Inject, Optional, signal, computed, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

import { TorneosService, TorneoDto } from '../../servicios/torneos.service';
import { PartidosService, PartidoDto } from '../../servicios/partidos.service';
import { EquiposService } from '../../servicios/equipos.service';

type RondaTipo = 'Final' | 'Semifinal' | 'Cuartos' | 'Octavos';
function esRonda(x: any): x is RondaTipo {
  return x === 'Final' || x === 'Semifinal' || x === 'Cuartos' || x === 'Octavos';
}

type SerieUi = {
  seriePlayoffId: number;
  ronda: RondaTipo;
  seedA: number;
  seedB: number;
  eqAId: number;
  eqBId: number;
  eqANombre: string;
  eqBNombre: string;
  winsA: number;
  winsB: number;
  juegos: PartidoDto[];
};

@Component({
  standalone: true,
  selector: 'app-bracket',
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatProgressSpinnerModule
  ],
  template: `
    <mat-card>
      <div class="hdr">
        <div>
          <h2>Llaves del Torneo</h2>
          <div class="sub" *ngIf="torneo() as t">#{{t.id}} — {{t.nombre}} (Best-of {{t.bestOf}})</div>
        </div>
        <div class="actions">
          <button mat-stroked-button (click)="refrescar()"><mat-icon>refresh</mat-icon> Actualizar</button>
          <button mat-raised-button color="primary" (click)="siguienteRonda()" [disabled]="loading()">Generar siguiente ronda</button>
        </div>
      </div>

      <!-- Selector de torneos (solo aparece si hay más de 1) -->
      <div class="torneos" *ngIf="torneosAbiertos().length > 1">
        <button
          mat-stroked-button
          class="chip"
          *ngFor="let t of torneosAbiertos()"
          [class.active]="t.id === torneoId()"
          (click)="cambiarTorneo(t.id)">
          #{{t.id}} · {{t.nombre}}
        </button>
      </div>

      <div class="grid" *ngIf="!loading(); else loadingTpl">
        <ng-container *ngFor="let col of columnas()">
          <div class="col">
            <h3 class="ronda">{{ col.ronda }}</h3>

            <div class="serie" *ngFor="let s of col.series">
              <div class="header">
                <div class="eq">
                  <span class="seed">#{{s.seedA}}</span>
                  <span class="name">{{ s.eqANombre }}</span>
                  <span class="wins">{{ s.winsA }}</span>
                </div>
                <div class="eq">
                  <span class="seed">#{{s.seedB}}</span>
                  <span class="name">{{ s.eqBNombre }}</span>
                  <span class="wins">{{ s.winsB }}</span>
                </div>
              </div>

              <div class="games">
                <div class="g" *ngFor="let g of s.juegos">
                  <span class="num">#{{g.gameNumber}}</span>
                  <span class="when">{{ g.fechaHora | date:'short' }}</span>
                  <span class="score" *ngIf="g.marcadorLocal != null && g.marcadorVisitante != null; else dash">
                    {{ nombreEquipo(g.equipoLocalId) }} {{ g.marcadorLocal }} - {{ g.marcadorVisitante }} {{ nombreEquipo(g.equipoVisitanteId) }}
                  </span>
                  <ng-template #dash><span class="score">—</span></ng-template>
                  <span class="estado">{{ g.estado }}</span>
                </div>
              </div>
            </div>
          </div>
        </ng-container>
      </div>

      <ng-template #loadingTpl>
        <div class="loading">
          <mat-progress-spinner mode="indeterminate"></mat-progress-spinner>
        </div>
      </ng-template>
    </mat-card>
  `,
  styles: [`
    mat-card { padding: 16px; }
    .hdr { display:flex; align-items:center; justify-content:space-between; gap:16px; }
    .sub { opacity:.8; margin-top:-6px; }
    .actions { display:flex; gap:8px; }
    .torneos { display:flex; flex-wrap:wrap; gap:8px; margin:10px 0 0; }
    .chip.active { background: rgba(103,58,183,.12); border-color: rgba(103,58,183,.42); }
    .grid { display:grid; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap:12px; margin-top:12px; }
    .col { display:flex; flex-direction:column; gap:12px; }
    .ronda { margin:0 0 4px; opacity:.85; }
    .serie { border:1px solid rgba(0,0,0,.08); border-radius:12px; padding:8px; background:rgba(0,0,0,.02); }
    .header .eq { display:flex; align-items:center; gap:8px; }
    .seed { font-weight:600; opacity:.7; }
    .name { flex:1; }
    .wins { font-weight:700; }
    .games { margin-top:6px; display:flex; flex-direction:column; gap:4px; }
    .g { display:grid; grid-template-columns: auto 1fr 2fr auto; gap:8px; align-items:center; font-size:.92rem; }
    .num { font-weight:600; opacity:.7; }
    .when { opacity:.8; }
    .score { font-variant-numeric: tabular-nums; }
    .estado { opacity:.7; }
    .loading { display:flex; justify-content:center; padding:24px; }
  `]
})
export class BracketComponent implements OnChanges {
  /** Permite usar este componente dentro de un modal (además de por ruta). */
  @Input() torneoIdInput: number | null = null;

  torneoId = signal<number | null>(null);
  torneo   = signal<TorneoDto | null>(null);
  loading  = signal(false);

  private equiposMap = signal<Record<number, string>>({});
  private partidos   = signal<PartidoDto[]>([]);
  private _torneosAbiertos = signal<TorneoDto[]>([]);
  torneosAbiertos = () => this._torneosAbiertos();

  columnas = computed(() => {
    const bySerie = new Map<number, SerieUi>();
    const map = this.equiposMap();

    const partidos = this.partidos()
      .slice()
      .sort((a, b) => a.gameNumber - b.gameNumber)
      .filter(p => !!p.seriePlayoffId && esRonda(p.ronda));

    for (const p of partidos) {
      const key = p.seriePlayoffId;

      let s = bySerie.get(key);
      if (!s) {
        s = {
          seriePlayoffId: key,
          ronda: p.ronda as RondaTipo,
          seedA: p.seedA,
          seedB: p.seedB,
          eqAId: p.equipoLocalId,
          eqBId: p.equipoVisitanteId,
          eqANombre: map[p.equipoLocalId] ?? `Equipo ${p.equipoLocalId}`,
          eqBNombre: map[p.equipoVisitanteId] ?? `Equipo ${p.equipoVisitanteId}`,
          winsA: 0,
          winsB: 0,
          juegos: []
        };
        bySerie.set(key, s);
      }

      s.juegos.push(p);

      if (p.estado === 'Finalizado' && p.marcadorLocal != null && p.marcadorVisitante != null) {
        const ganadorId = p.marcadorLocal > p.marcadorVisitante ? p.equipoLocalId : p.equipoVisitanteId;
        if (ganadorId === s.eqAId) s.winsA++; else if (ganadorId === s.eqBId) s.winsB++;
      }
    }

    const porRonda: Record<string, SerieUi[]> = {};
    for (const s of bySerie.values()) {
      (porRonda[s.ronda] ||= []).push(s);
    }

    const orden: RondaTipo[] = ['Octavos', 'Cuartos', 'Semifinal', 'Final'];
    return orden
      .filter(r => porRonda[r]?.length)
      .map(r => ({ ronda: r, series: porRonda[r].sort((a, b) => a.seedA - b.seedA) }));
  });

  constructor(
    private route: ActivatedRoute,
    private torneos: TorneosService,
    private partidosSvc: PartidosService,
    private equiposSvc: EquiposService,
    @Optional() @Inject(MAT_DIALOG_DATA) private data?: { torneoId?: number }
  ) {
    // mapa de equipos
    this.equiposSvc.list().subscribe({
      next: (arr: any[]) => {
        const m: Record<number, string> = {};
        for (const e of arr) m[e.id] = e.nombre ?? `Equipo ${e.id}`;
        this.equiposMap.set(m);
      }
    });

    // Cargar torneos "abiertos" (y fallback con historial si llega <=1)
    this.cargarTorneosAbiertos();

    // Resolución del id: data (modal) > @Input > :torneoId|:id en ruta > ?torneoId= en query
    const dataId = this.data?.torneoId ?? null;
    if (dataId != null) {
      this.torneoId.set(dataId);
      this.refrescar();
    } else if (this.torneoIdInput != null) {
      this.torneoId.set(this.torneoIdInput);
      this.refrescar();
    } else {
      this.route.paramMap.subscribe(pm => {
        const idByName = pm.get('torneoId') ?? pm.get('id');
        let id = Number(idByName);
        if (!Number.isFinite(id)) {
          const q = this.route.snapshot.queryParamMap.get('torneoId');
          id = Number(q);
        }
        if (!Number.isFinite(id)) return;
        this.torneoId.set(id);
        this.refrescar();
      });
    }
  }

  /** Reacciona si el @Input llega después de construir (caso modal). */
  ngOnChanges(changes: SimpleChanges): void {
    if ('torneoIdInput' in changes && this.torneoIdInput != null) {
      this.torneoId.set(this.torneoIdInput);
      this.refrescar();
    }
  }

  private cargarTorneosAbiertos() {
    const svc: any = this.torneos as any;
    const obs =
      (typeof svc.abiertos === 'function' && svc.abiertos()) ||
      (typeof svc.list === 'function' && svc.list()) ||
      (typeof svc.getAll === 'function' && svc.getAll()) ||
      null;

    if (obs && typeof obs.subscribe === 'function') {
      obs.subscribe({
        next: (arr: TorneoDto[]) => {
          const lista = arr ?? [];
          this._torneosAbiertos.set(lista);
          // Si solo llega 0–1, intento descubrir más torneos a partir de partidos
          if (lista.length <= 1) this.descubrirTorneosPorPartidos();
          // Si no hay seleccionado aún, usa el primero disponible
          if (!this.torneoId() && this._torneosAbiertos().length) {
            this.torneoId.set(this._torneosAbiertos()[0].id);
            this.refrescar();
          }
        },
        error: () => {
          // Si falla, intento por partidos
          this.descubrirTorneosPorPartidos();
        }
      });
    } else {
      // Sin método de torneos → fallback directo
      this.descubrirTorneosPorPartidos();
    }
  }

  private descubrirTorneosPorPartidos() {
    // Trae partidos de todos los torneos y arma lista única de torneos con actividad
    this.partidosSvc.historial({ page: 1, pageSize: 1000 }).subscribe({
      next: res => {
        const ids = Array.from(new Set((res.items ?? [])
          .map(p => p.torneoId)
          .filter((x): x is number => typeof x === 'number' && x > 0)));

        if (!ids.length) return;
        const encontrados: TorneoDto[] = [];
        let pendientes = ids.length;

        ids.forEach(id => {
          this.torneos.getById(id).subscribe({
            next: t => { if (t) encontrados.push(t); },
            complete: () => {
              pendientes--;
              if (pendientes === 0) {
                // Unir con la lista existente evitando duplicados
                const actuales = this._torneosAbiertos();
                const mapa = new Map<number, TorneoDto>(actuales.map(t => [t.id, t]));
                for (const t of encontrados) mapa.set(t.id, t);
                const combinados = Array.from(mapa.values()).sort((a,b) => a.id - b.id);
                this._torneosAbiertos.set(combinados);

                if (!this.torneoId() && combinados.length) {
                  this.torneoId.set(combinados[0].id);
                  this.refrescar();
                }
              }
            }
          });
        });
      }
    });
  }

  cambiarTorneo(id: number) {
    if (this.torneoId() === id) return;
    this.torneoId.set(id);
    this.refrescar();
  }

  nombreEquipo(id: number) {
    return this.equiposMap()[id] ?? `Equipo ${id}`;
  }

  refrescar() {
    const id = this.torneoId();
    if (!id) return;

    this.loading.set(true);
    this.torneos.getById(id).subscribe({ next: t => this.torneo.set(t) });

    this.partidosSvc.historial({ torneoId: id, page: 1, pageSize: 500 })
      .subscribe({
        next: res => this.partidos.set(res.items),
        complete: () => this.loading.set(false)
      });
  }

  siguienteRonda() {
    const id = this.torneoId();
    if (!id) return;
    this.loading.set(true);
    this.torneos.generarSiguienteRonda(id).subscribe({
      next: () => this.refrescar(),
      error: (e) => { alert(e?.error ?? 'No se pudo generar la siguiente ronda'); this.loading.set(false); }
    });
  }
}

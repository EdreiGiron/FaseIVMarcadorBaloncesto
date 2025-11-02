import { Component, effect, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PartidosService, PartidoDto } from '../../servicios/partidos.service';
import { EquiposService } from '../../servicios/equipos.service';
import { TorneosService } from '../../servicios/torneos.service';
import { MarcadorService } from '../../servicios/marcador.service';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { PartidoCerrarDialog } from './partido-cerrar.dialog';
import { Router } from '@angular/router';
import { RouterModule } from '@angular/router';
import { BracketComponent } from '../torneos/bracket.component';

@Component({
  standalone: true,
  selector: 'app-partidos-historial',
  imports: [
    CommonModule, FormsModule,
    MatCardModule, MatTabsModule, MatTableModule,
    MatFormFieldModule, MatInputModule, MatAutocompleteModule, MatSelectModule,
    MatButtonModule, MatIconModule, MatDialogModule, RouterModule
  ],
  templateUrl: './partidos-historial.component.html',
  styleUrls: ['./partidos-historial.component.scss']
})
export class PartidosHistorialComponent {
  // ===== Filtros efectivos (en cliente) =====
  torneoId = signal<number | undefined>(undefined);
  equipoId  = signal<number | undefined>(undefined);
  ronda     = signal<string | undefined>(undefined);

  // Texto visible en inputs
  torneoNombre = signal<string>('');
  equipoNombre = signal<string>('');

  // catálogos (si están disponibles)
  private torneosList = signal<Array<{id:number; nombre:string}>>([]);
  private equiposList = signal<Array<{id:number; nombre:string}>>([]);
  private torneosIndex = signal<Record<string, number>>({});
  private equiposIndex = signal<Record<string, number>>({});

  // mapas id->nombre para pintar
  private equiposMap  = signal<Record<number, string>>({});
  private torneosMap  = signal<Record<number, string>>({});

  torneosFiltrados = computed(() => {
    const q = this.torneoNombre().trim().toLowerCase();
    const all = this.torneosList();
    if (!q) return all;
    return all.filter(t => (t.nombre ?? '').toLowerCase().includes(q));
  });

  equiposFiltrados = computed(() => {
    const q = this.equipoNombre().trim().toLowerCase();
    const all = this.equiposList();
    if (!q) return all;
    return all.filter(e => (e.nombre ?? '').toLowerCase().includes(q));
  });

  // tablas
  hoy         = signal<PartidoDto[]>([]);
  proximos    = signal<PartidoDto[]>([]);
  finalizados = signal<PartidoDto[]>([]);
  displayedColumns = ['fecha','ronda','game','tipo','torneo','equipos','marcador','estado','acciones'];

  loading = signal(false);
  error   = signal<string | undefined>(undefined);

  constructor(
    private api: PartidosService,
    private equiposSvc: EquiposService,
    private torneosSvc: TorneosService,
    private marcadorSvc: MarcadorService,
    private dialog: MatDialog,
    private router: Router
  ) {
    // cat. equipos
    this.equiposSvc.list().subscribe({
      next: (arr: any[]) => {
        const m: Record<number,string> = {};
        const list: Array<{id:number; nombre:string}> = [];
        const idx: Record<string,number> = {};
        for (const e of arr ?? []) {
          if (typeof e?.id !== 'number') continue;
          const nombre = e?.nombre ?? `Equipo ${e.id}`;
          m[e.id] = nombre;
          list.push({ id: e.id, nombre });
          idx[nombre.toLowerCase()] = e.id;
        }
        this.equiposMap.set(m);
        this.equiposList.set(list.sort((a,b) => a.nombre.localeCompare(b.nombre)));
        this.equiposIndex.set(idx);
      }
    });

    // cat. torneos (si existe método)
    this._cargarCatalogoTorneos();

    // carga inicial
    effect(() => this.cargar());
  }

  // ===== normalizador de ronda =====
  private _normalizeRonda(v?: string | null): 'Final' | 'Semifinal' | 'Cuartos' | 'Octavos' | undefined {
    if (!v) return undefined;
    const s = v.trim().toLowerCase();
    if (!s) return undefined;
    if (['final', 'finales', 'finals'].includes(s)) return 'Final';
    if (['semifinal', 'semifinales', 'semi', 'semifinals'].includes(s)) return 'Semifinal';
    if (['cuartos', 'cuartos de final', 'quarters', 'quarterfinals', 'qf'].includes(s)) return 'Cuartos';
    if (['octavos', 'octavos de final', 'round of 16', 'of'].includes(s)) return 'Octavos';
    return (s.charAt(0).toUpperCase() + s.slice(1)) as any;
  }

  // ===== autocomplete Torneo =====
  private _resolverTorneoIdDesdeTexto(texto: string): number | undefined {
    const raw = (texto ?? '').trim().toLowerCase();
    if (!raw) return undefined;
    const exact = this.torneosIndex()[raw];
    if (typeof exact === 'number') return exact;
    const candidatos = this.torneosFiltrados();
    if (candidatos.length === 1) return candidatos[0].id;
    return undefined;
  }
  onTorneoChange(texto: string) {
    this.torneoNombre.set(texto);
    this.torneoId.set(this._resolverTorneoIdDesdeTexto(texto));
    this.cargar(); // fuerza refresco inmediato
  }
  onTorneoSelect(id: number, nombre: string) {
    this.torneoNombre.set(nombre);
    this.torneoId.set(id);
    this.cargar();
  }
  confirmarTorneo() {
    const id = this._resolverTorneoIdDesdeTexto(this.torneoNombre());
    this.torneoId.set(id);
    this.cargar();
  }

  // ===== autocomplete Equipo =====
  onEquipoChange(texto: string) {
    this.equipoNombre.set(texto);
    const id = this.equiposIndex()[texto.trim().toLowerCase()];
    this.equipoId.set(id);
    this.cargar();
  }
  onEquipoSelect(id: number, nombre: string) {
    this.equipoNombre.set(nombre);
    this.equipoId.set(id);
    this.cargar();
  }

  // ===== Ronda =====
  onRondaChange(texto: string) {
    this.ronda.set(this._normalizeRonda(texto));
    this.cargar();
  }
  aplicarRonda() {
    this.onRondaChange(this.ronda() ?? '');
  }

  // ===== cat. torneos (si hay método disponible) =====
  private _cargarCatalogoTorneos() {
    const svc: any = this.torneosSvc;
    const fn =
      svc?.list ?? svc?.listar ?? svc?.getAll ?? svc?.getLista ?? svc?.obtenerTodos ?? svc?.all;
    if (typeof fn !== 'function') {
      this.torneosList.set([]);
      this.torneosIndex.set({});
      this.torneosMap.set({});
      return;
    }
    fn.call(svc).subscribe({
      next: (arr: any[]) => {
        const list: Array<{ id: number; nombre: string }> = [];
        const idx: Record<string, number> = {};
        const map: Record<number, string> = {};
        for (const t of arr ?? []) {
          if (typeof t?.id !== 'number') continue;
          const nombre = t?.nombre ?? `Torneo ${t.id}`;
          list.push({ id: t.id, nombre });
          idx[nombre.toLowerCase()] = t.id;
          map[t.id] = nombre;
        }
        this.torneosList.set(list.sort((a,b) => a.nombre.localeCompare(b.nombre)));
        this.torneosIndex.set(idx);
        this.torneosMap.set(map);
      },
      error: () => {
        this.torneosList.set([]);
        this.torneosIndex.set({});
        this.torneosMap.set({});
      }
    });
  }

  // ===== utilidades visuales =====
  nombreEquipo(id?: number | null) {
    if (id == null) return '—';
    return this.equiposMap()[id] ?? `Equipo ${id}`;
  }

  // Obtiene el nombre del torneo desde múltiples posibles campos y lo memoriza
  nombreTorneo(p: any): string {
    const id = (typeof p?.torneoId === 'number') ? p.torneoId
              : (typeof p?.torneo?.id === 'number') ? p.torneo.id
              : null;

    // lista de alias posibles donde podría venir el nombre
    const posibles = [
      p?.torneoNombre,
      p?.torneo?.nombre,
      p?.nombreTorneo,
      p?.torneo_name,
      p?.competition?.name,
      p?.league?.name
    ].filter(Boolean) as string[];

    let nombre = posibles[0];

    // si no vino en el objeto, probar el catálogo (si existe)
    if (!nombre && id) nombre = this.torneosMap()[id];

    // fallback: si no sabemos el nombre, usar lo que el usuario escribió (si matchea),
    // para no mostrar "Torneo X" cuando en pantalla está buscando por ese nombre.
    if (!nombre) {
      const q = this.torneoNombre().trim().toLowerCase();
      if (q) {
        const txt = (p?.torneoNombre ?? p?.torneo?.nombre ?? '').toLowerCase();
        if (txt.includes(q)) nombre = p?.torneoNombre ?? p?.torneo?.nombre;
      }
    }

    if (!nombre) nombre = id ? `Torneo ${id}` : ' - ';

    // memorizar para futuras filas
    if (id && nombre && nombre !== `Torneo ${id}`) {
      const map = { ...this.torneosMap() };
      if (!map[id]) { map[id] = nombre; this.torneosMap.set(map); }
    }
    return nombre;
  }

  getTipoJuego(p: PartidoDto): string {
    const anyP = p as any;
    if (typeof anyP?.esAmistoso === 'boolean') {
      return anyP.esAmistoso ? 'Amistoso' : 'Torneo';
    }
    return anyP?.torneoId ? 'Torneo' : 'Amistoso';
  }

  // ===== filtro local seguro =====
  private _postFilter(arr: PartidoDto[]): PartidoDto[] {
    const tid       = this.torneoId();
    const rondaNorm = this._normalizeRonda(this.ronda());
    const eqId      = this.equipoId();
    const tNameQ    = this.torneoNombre().trim().toLowerCase();

    return arr.filter(p => {
      // torneo por id
      if (tid != null) {
        const pTid = (p as any)?.torneoId ?? (p as any)?.torneo?.id ?? null;
        if (pTid !== tid) return false;
      }
      // torneo por NOMBRE (subcadena), si no hay id pero hay texto
      if (tid == null && tNameQ) {
        const nombre = this.nombreTorneo(p).toLowerCase();
        if (!nombre.includes(tNameQ)) return false;
      }
      // ronda
      if (rondaNorm) {
        const pr = (p as any)?.ronda ?? '';
        const prNorm = this._normalizeRonda(pr);
        if (prNorm !== rondaNorm) return false;
      }
      // equipo
      if (eqId != null) {
        if (p.equipoLocalId !== eqId && p.equipoVisitanteId !== eqId) return false;
      }
      return true;
    });
  }

  private _aprenderTorneosDesde(items: any[]) {
    if (!items?.length) return;
    const map = { ...this.torneosMap() };
    for (const p of items) {
      const id = (typeof p?.torneoId === 'number') ? p.torneoId
               : (typeof p?.torneo?.id === 'number') ? p.torneo.id
               : null;
      const nombre = p?.torneoNombre ?? p?.torneo?.nombre ?? p?.nombreTorneo ?? p?.torneo_name;
      if (id && nombre && !map[id]) map[id] = nombre;
    }
    this.torneosMap.set(map);
  }

  private cargar() {
    this.loading.set(true); this.error.set(undefined);

    // NO mandamos filtros al backend; aquí solo página/tamaño/fechas/estado.
    const baseParams: any = { page: 1, pageSize: 200 };

    const hoyIso = new Date(); hoyIso.setHours(0,0,0,0);
    const mañanaIso = new Date(hoyIso); mañanaIso.setDate(mañanaIso.getDate()+1);

    // Hoy
    this.api.historial({
      ...baseParams,
      fechaDesde: hoyIso.toISOString(),
      fechaHasta: mañanaIso.toISOString()
    }).subscribe({
      next: r => {
        this._aprenderTorneosDesde(r.items);
        const filtrados = this._postFilter(r.items)
          .filter(p => p.estado === 'Programado' || p.estado === 'EnJuego');
        this.hoy.set(filtrados);
      },
      error: err => this.error.set(err?.error ?? 'Error al cargar partidos de hoy'),
      complete: () => this.loading.set(false)
    });

    // Próximos
    this.api.historial({
      ...baseParams,
      fechaDesde: mañanaIso.toISOString()
    }).subscribe({
      next: r => {
        this._aprenderTorneosDesde(r.items);
        const filtrados = this._postFilter(r.items).filter(p => p.estado === 'Programado');
        this.proximos.set(filtrados);
      },
      error: err => this.error.set(err?.error ?? 'Error al cargar próximos')
    });

    // Finalizados
    this.api.historial({
      ...baseParams,
      estado: 'Finalizado'
    }).subscribe({
      next: r => {
        this._aprenderTorneosDesde(r.items);
        const filtrados = this._postFilter(r.items);
        this.finalizados.set(filtrados);
      },
      error: err => this.error.set(err?.error ?? 'Error al cargar finalizados')
    });
  }

  cerrar(p: PartidoDto) {
    const ref = this.dialog.open(PartidoCerrarDialog, {
      data: {
        localNombre: this.nombreEquipo(p.equipoLocalId),
        visitanteNombre: this.nombreEquipo(p.equipoVisitanteId),
        marcadorLocal: p.marcadorLocal ?? 0,
        marcadorVisitante: p.marcadorVisitante ?? 0
      }
    });

    ref.afterClosed().subscribe(res => {
      if (!res) return;
      this.api.cerrarPartido(p.id, res.ml, res.mv).subscribe({
        next: () => this.cargar(),
        error: () => alert('No se pudo cerrar el partido')
      });
    });
  }

  abrirRoster(p: PartidoDto) {
    this.router.navigate(
      ['/admin/partidos', p.id, 'roster'],
      { state: { partido: p } }
    );
  }

  irTablero(p: PartidoDto) {
    // Primero inicializar el marcador con este partido
    this.marcadorSvc.inicializarConPartido(p.id).subscribe({
      next: () => {
        // Luego navegar al tablero
        this.router.navigate(['/control']);
      },
      error: () => {
        alert('No se pudo inicializar el partido');
      }
    });
  }

  // ===== Bracket en modal =====
  private _extraerTorneoId(p: any): number | null {
    if (!p) return null;
    return (
      (typeof p.torneoId === 'number' && p.torneoId) ||
      (p.torneo && typeof p.torneo.id === 'number' && p.torneo.id) ||
      null
    );
  }

  verBracket() {
    let id = this.torneoId?.() ?? null;
    if (id == null) {
      id =
        this._extraerTorneoId(this.hoy()?.[0]) ??
        this._extraerTorneoId(this.proximos()?.[0]) ??
        this._extraerTorneoId(this.finalizados()?.[0]) ??
        null;
    }
    if (id == null) {
      alert('No encuentro el Torneo ID.');
      return;
    }

    const ref = this.dialog.open(BracketComponent, {
      width: '1000px',
      maxWidth: '95vw',
      data: { torneoId: id },
      panelClass: 'dialog-bracket'
    });

    ref.afterOpened().subscribe(() => {
      if (!ref.componentInstance) return;
      ref.componentInstance.torneoIdInput = id;
      (ref.componentInstance as any).torneoId?.set(id);
      (ref.componentInstance as any).refrescar?.();
    });
  }
}

import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { interval, Subscription } from 'rxjs';
import { CommonModule } from '@angular/common';
import { MarcadorService } from '../servicios/marcador.service';
import { EquiposService } from '../servicios/equipos.service';

// 👇 añadidos mínimos
import { ActivatedRoute, Router } from '@angular/router';
import { PartidosService } from '../servicios/partidos.service';

interface Equipo { nombre:string; puntos:number; faltas:number; }
interface MarcadorGlobal {
  equipoLocal: Equipo; equipoVisitante: Equipo;
  cuartoActual: number; enProrroga:boolean; numeroProrroga:number;
  tiempoRestante:number; relojCorriendo?:boolean;
}

@Component({
  standalone: true,
  selector: 'app-tablero-publico',
  imports: [CommonModule],
  templateUrl: './tablero-publico.component.html',
  styleUrls: ['./tablero-publico.component.css']
})
export class TableroPublicoComponent implements OnInit, OnDestroy {
  private marcador = inject(MarcadorService);
  private equipos   = inject(EquiposService);

  // 👇 añadidos mínimos
  private route     = inject(ActivatedRoute);
  private partidos  = inject(PartidosService);
  private router    = inject(Router);

  // Fallbacks (rutas relativas para evitar 404 por baseHref)
  defaultLogoLocal  = 'assets/logos/local.png';
  defaultLogoVisita = 'assets/logos/visita.png';

  titulo = signal<string>('Amistoso');

  d = signal<MarcadorGlobal | null>(null);

  // catálogo en memoria para resolver logos por nombre
  private _equiposCache: Array<{id:number; nombre:string; logoFileName?:string|null}> = [];

  // URLs calculadas
  logoLocalUrl?:  string | null = null;
  logoVisitaUrl?: string | null = null;

  private _tick?: Subscription;

  // ===== utilidades =====
  private normalize(s?: string): string {
    return (s ?? '')
      .trim()
      .toLowerCase()
      .normalize('NFD')
      .replace(/\p{Diacritic}/gu, '');
  }

  private logoUrl(file?: string | null): string | null {
    if (!file) return null;
    if (/^https?:\/\//i.test(file)) return file;
    return `/api/equipos/logo/${encodeURIComponent(file)}`;
  }

  // match seguro: exacto; si no, empieza-con, sólo si es único
  private findEquipoByNombre(nombre?: string) {
    const n = this.normalize(nombre);
    if (!n) return undefined;

    // 1) exacto
    const exact = this._equiposCache.filter(x => this.normalize(x.nombre) === n);
    if (exact.length === 1) return exact[0];

    // 2) empieza-con (único)
    const starts = this._equiposCache.filter(x => this.normalize(x.nombre).startsWith(n) || n.startsWith(this.normalize(x.nombre)));
    if (starts.length === 1) return starts[0];

    // 3) nada seguro => undefined (usará fallback)
    return undefined;
  }

  private setLogosByNames(localNombre?: string, visitaNombre?: string) {
    if (!this._equiposCache.length) return;

    const eL = this.findEquipoByNombre(localNombre);
    const eV = this.findEquipoByNombre(visitaNombre);

    this.logoLocalUrl  = this.logoUrl(eL?.logoFileName)  ?? this.defaultLogoLocal;
    this.logoVisitaUrl = this.logoUrl(eV?.logoFileName) ?? this.defaultLogoVisita;
  }

  // fallback visible para errores de <img>
  useFallback(side: 'local'|'visita') {
    if (side === 'local')  this.logoLocalUrl  = this.defaultLogoLocal;
    if (side === 'visita') this.logoVisitaUrl = this.defaultLogoVisita;
  }

  t(): number {
    const m = this.d();
    return m ? Math.max(0, Math.floor(m.tiempoRestante)) : 0;
  }

  // ===== ciclo de vida =====
  ngOnInit(): void {
    const savedTitle = localStorage.getItem('marcador.titulo');
    if (savedTitle) this.titulo.set(savedTitle);

    // 1) cargar equipos (una vez)
    this.equipos.list().subscribe({
      next: (arr: any[]) => {
        this._equiposCache = (arr ?? []).map(e => ({
          id: e.id,
          nombre: e.nombre ?? '',
          logoFileName: e.logoFileName ?? e.logo ?? null
        }));

        // 👇 si venimos de /publico/:id, inicializa nombres desde el partido
        const param = this.route.snapshot.paramMap.get('id');
        const partidoId = param ? Number(param) : NaN;
        if (Number.isFinite(partidoId)) {
          this.initFromPartido(partidoId);
        }

        const cur = this.d();
        if (cur) this.setLogosByNames(cur.equipoLocal?.nombre, cur.equipoVisitante?.nombre);
      }
    });

    // 2) cargar marcador + refrescar
    this.refrescar();
    this._tick = interval(3000).subscribe(() => this.refrescar(false));
  }

  ngOnDestroy(): void {
    this._tick?.unsubscribe();
  }

  refrescar(_mark = true) {
    this.marcador.obtenerMarcador().subscribe({
      next: (res) => {
        this.d.set(res);
        this.setLogosByNames(res?.equipoLocal?.nombre, res?.equipoVisitante?.nombre);
      }
    });
  }

  mmss(seg:number) {
    const s = Math.max(0, Math.floor(seg));
    const m = Math.floor(s/60);
    const r = s % 60;
    return `${m.toString().padStart(2,'0')}:${r.toString().padStart(2,'0')}`;
  }

  // ======= NUEVO (mínimo necesario): tomar nombres desde el partido =======
  private initFromPartido(id: number) {
    this.partidos.getById(id).subscribe({
      next: (p: any) => {
        // resuelve nombres por ID con el catálogo ya cargado
        const nombreLocal  = this._equiposCache.find(e => e.id === p.equipoLocalId)?.nombre  ?? 'Local';
        const nombreVisita = this._equiposCache.find(e => e.id === p.equipoVisitanteId)?.nombre ?? 'Visitante';

        // renombra en el marcador (no crea fichas nuevas)
        this.marcador.renombrarEquipos(nombreLocal, nombreVisita).subscribe({
          next: (res) => {
            this.d.set(res);
            this.setLogosByNames(nombreLocal, nombreVisita);
          },
          error: () => {
            // si por alguna razón falla, al menos actualiza la vista local
            const cur = this.d();
            if (cur) {
              cur.equipoLocal.nombre = nombreLocal;
              cur.equipoVisitante.nombre = nombreVisita;
              this.d.set({ ...cur });
              this.setLogosByNames(nombreLocal, nombreVisita);
            }
          }
        });

        
        // if (p.torneoId && p.torneoId > 0) this.titulo.set('Playoffs'); else this.titulo.set('Amistoso');
      }
    });
  }

  goBack() {
    this.router.navigate(['/dashboard']);
  }
}

import { Component, signal, computed, effect } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';

import { PartidosService, PartidoDto } from '../../servicios/partidos.service';
import { JugadoresService } from '../../servicios/jugadores.service';
import { EquiposService } from '../../servicios/equipos.service';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatListModule } from '@angular/material/list';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';

type JugadorLite = { id:number; nombre:string };

@Component({
  standalone: true,
  selector: 'app-partido-roster',
  imports: [
    CommonModule, FormsModule,
    MatCardModule, MatButtonModule, MatSelectModule, MatListModule,
    MatCheckboxModule, MatFormFieldModule, MatIconModule, MatInputModule
  ],
  templateUrl: './partido-roster.component.html',
  styleUrls: ['./partido-roster.component.scss']
})
export class PartidoRosterComponent {
  partido = signal<PartidoDto | null>(null);

  // nombres de equipos (map id->nombre)
  equiposMap = signal<Record<number,string>>({});

  // equipo seleccionado (local/visitante id)
  equipoId = signal<number | null>(null);

  // lista de jugadores del equipo seleccionado
  jugadores = signal<JugadorLite[]>([]);
  seleccion = signal<Set<number>>(new Set());   // ids seleccionados
  titulares = signal<Set<number>>(new Set());   // ids marcados como titulares

  msg = signal<string|undefined>(undefined);
  guardando = signal(false);

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private loc: Location,
    private partidos: PartidosService,
    private jugadoresSvc: JugadoresService,
    private equiposSvc: EquiposService
  ) {
    // obtenemos el partido desde state (viene del historial)
    const st = this.router.getCurrentNavigation()?.extras?.state as { partido?: PartidoDto } | undefined;
    if (st?.partido) this.partido.set(st.partido);

    // cargar nombres de equipos
    this.equiposSvc.list().subscribe({
      next: (arr: any[]) => {
        const m: Record<number,string> = {};
        for (const e of arr) m[e.id] = e.nombre ?? `Equipo ${e.id}`;
        this.equiposMap.set(m);
        // si hay partido, selecciona por defecto el local
        if (this.partido()) this.equipoId.set(this.partido()!.equipoLocalId);
      }
    });

    // cuando cambie equipoId, carga jugadores de ese equipo
    effect(() => {
      const id = this.equipoId();
      if (id == null) return;
      this.jugadoresSvc.list(undefined, undefined, id).subscribe({
        next: (arr: any[]) => {
          const js = (arr || []).map((j:any) => ({ id: j.id, nombre: j.nombre ?? `Jugador ${j.id}` }));
          this.jugadores.set(js);
          // limpia selección al cambiar equipo
          this.seleccion.set(new Set());
          this.titulares.set(new Set());
        }
      });
    });
  }

  nombreEquipo(id?: number | null) {
    if (id == null) return '—';
    return this.equiposMap()[id] ?? `Equipo ${id}`;
  }

  toggle(jid:number) {
    const sel = new Set(this.seleccion());
    sel.has(jid) ? sel.delete(jid) : sel.add(jid);
    // limitar a 12
    if (sel.size > 12) { this.msg.set('Máximo 12 jugadores.'); sel.delete(Array.from(sel).pop()!); }
    this.seleccion.set(sel);
  }

  toggleTitular(jid:number) {
    const tit = new Set(this.titulares());
    tit.has(jid) ? tit.delete(jid) : tit.add(jid);
    this.titulares.set(tit);
  }

  guardar() {
    const p = this.partido(); const eq = this.equipoId();
    if (!p || !eq) { this.msg.set('Falta información de partido/equipo'); return; }

    const jugadores = Array.from(this.seleccion()).slice(0,12).map(jid => ({
      jugadorId: jid,
      titular: this.titulares().has(jid)
    }));

    this.guardando.set(true);
    this.partidos.asignarRoster(p.id, eq, jugadores).subscribe({
      next: () => this.msg.set('✅ Roster guardado'),
      error: () => this.msg.set('No se pudo guardar el roster'),
      complete: () => this.guardando.set(false)
    });
  }

  volver() { this.loc.back(); }
}

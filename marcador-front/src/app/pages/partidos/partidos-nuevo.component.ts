import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

import { PartidosService } from '../../servicios/partidos.service';
import { EquiposService } from '../../servicios/equipos.service';

type EquipoLite = { id: number; nombre: string };

@Component({
  standalone: true,
  selector: 'app-partidos-nuevo',
  imports: [
    CommonModule, FormsModule,
    MatCardModule, MatFormFieldModule, MatSelectModule, MatInputModule, MatButtonModule
  ],
  templateUrl: './partidos-nuevo.component.html',
  styleUrls: ['./partidos-nuevo.component.scss']
})
export class PartidosNuevoComponent {
  private partidos = inject(PartidosService);
  private equiposSvc = inject(EquiposService);

  // señales del formulario (amistoso)
  fechaLocal = signal<string>('');           // <input type="datetime-local">
  localId    = signal<number | null>(null);
  visitaId   = signal<number | null>(null);

  equipos = signal<EquipoLite[]>([]);
  msg     = signal<string | undefined>(undefined);

  // control de guardado
  private saving = signal(false);
  guardando() { return this.saving(); }

  constructor() {
    // cargar equipos para los selects
    this.equiposSvc.list().subscribe({
      next: (arr: any[]) => {
        const data = arr.map(e => ({ id: e.id, nombre: e.nombre ?? `Equipo ${e.id}` }));
        this.equipos.set(data);
      }
    });
  }

  // Intercambiar local/visitante
  swap() {
    const a = this.localId();
    this.localId.set(this.visitaId());
    this.visitaId.set(a);
  }

  // Crear partido amistoso
  crear() {
    this.msg.set(undefined);

    const local  = this.localId();
    const visita = this.visitaId();
    if (local == null || visita == null) {
      this.msg.set('Selecciona equipo local y visitante.');
      return;
    }
    if (local === visita) {
      this.msg.set('Los equipos no pueden ser iguales.');
      return;
    }

    const f = this.fechaLocal();
    let iso: string;
    try {
      iso = f ? (f.length === 16 ? `${f}:00` : f) // 'YYYY-MM-DDTHH:mm[:ss]' SIN 'Z'
         : (() => {
             const now = new Date();
             const pad = (n: number) => String(n).padStart(2, '0');
             return `${now.getFullYear()}-${pad(now.getMonth()+1)}-${pad(now.getDate())}` +
                    `T${pad(now.getHours())}:${pad(now.getMinutes())}:00`;
           })();
    } catch {
      this.msg.set('Fecha/hora inválida.');
      return;
    }

    this.saving.set(true);
    this.partidos
      .crear({ fechaHora: iso, equipoLocalId: local, equipoVisitanteId: visita })
      .subscribe({
        next: (p: any) => {
          this.msg.set(`✅ Partido amistoso creado (#${p?.id ?? '—'})`);
          this.saving.set(false);
        },
        error: (err: any) => {
          this.msg.set(err?.error ?? 'No se pudo crear el partido');
          this.saving.set(false);
        }
      });
  }
}

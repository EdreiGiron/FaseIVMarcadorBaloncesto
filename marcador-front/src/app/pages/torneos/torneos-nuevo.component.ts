import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TorneosService } from '../../servicios/torneos.service';
import { EquiposService } from '../../servicios/equipos.service';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

@Component({
  standalone: true,
  selector: 'app-torneos-nuevo',
  imports: [CommonModule, FormsModule,
    MatCardModule, MatFormFieldModule, MatSelectModule, MatInputModule, MatButtonModule, RouterModule],
  templateUrl: './torneos-nuevo.component.html',
  styleUrls: ['./torneos-nuevo.component.scss']
})
export class TorneosNuevoComponent {
  nombre = signal('Playoffs');
  temporada = signal<number>(new Date().getFullYear());
  bestOf = signal(5);
  equipos = signal<{id:number; nombre:string}[]>([]);
  seeds = signal<number[]>([]);

  msg = signal<string|undefined>(undefined);
  creando = signal(false);

  constructor(private api: TorneosService, private equiposSvc: EquiposService) {
    this.equiposSvc.list().subscribe({
      next: (arr: any[]) => {
        const data = arr.map(x => ({ id: x.id, nombre: x.nombre ?? `Equipo ${x.id}` }));
        this.equipos.set(data);
      }
    });
  }

  toggleSeed(id: number) {
    const s = this.seeds().slice();
    const idx = s.indexOf(id);
    if (idx >= 0) s.splice(idx,1); else s.push(id);
    this.seeds.set(s);
  }

  crear() {
    const payload = {
      nombre: this.nombre(),
      temporada: this.temporada(),
      bestOf: this.bestOf(),
      equipoIdsSeed: this.seeds()
    };
    if (payload.equipoIdsSeed.length < 2) { this.msg.set('Selecciona al menos 2 equipos.'); return; }
    this.creando.set(true);
    this.api.crear(payload).subscribe({
      next: (t) => this.msg.set(`✅ Torneo creado: ${t.nombre} (#${t.id})`),
      error: (err) => this.msg.set(err?.error ?? 'No se pudo crear'),
      complete: () => this.creando.set(false)
    });
  }

  seedDemo() {
    this.creando.set(true);
    this.api.seedDemo().subscribe({
      next: r => this.msg.set(`✅ ${r.message} (torneoId: ${r.torneoId})`),
      error: () => this.msg.set('No se pudo crear demo'),
      complete: () => this.creando.set(false)
    });
  }
}

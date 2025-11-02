import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output, signal, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { JugadoresService } from '../../servicios/jugadores.service';
import { JugadorAdminDto } from '../../modelos/jugador-admin';
import { EquiposService } from '../../servicios/equipos.service';
import { EquipoAdminDto } from '../../modelos/equipo-admin';

type Modo = 'crear'|'editar';

@Component({
  selector: 'app-jugador-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './jugador-form.component.html',
  styleUrls: ['./jugador-form.component.scss']
})
export class JugadorFormComponent implements OnInit {
  private svc = inject(JugadoresService);
  private equiposSvc = inject(EquiposService);

  @Input() modo: Modo = 'crear';
  @Input() inicial?: JugadorAdminDto | null = null;
  @Output() saved = new EventEmitter<JugadorAdminDto>();
  @Output() cancelled = new EventEmitter<void>();

  nombre = ''; posicion = '';
  equipoId: number | null = null;

  equipos = signal<EquipoAdminDto[]>([]);
  guardando = signal(false);
  errorMsg = signal<string | null>(null);

  ngOnInit(): void {
    if (this.inicial) {
      this.nombre = this.inicial.nombre;
      this.posicion = this.inicial.posicion ?? '';
      this.equipoId = this.inicial.equipoId;
    }
    // cargar equipos para el select
    this.equiposSvc.list().subscribe(e => this.equipos.set(e));
  }

  submit() {
    this.errorMsg.set(null);
    if (!this.nombre.trim()) return this.errorMsg.set('El nombre es obligatorio.');
    if (this.equipoId == null) return this.errorMsg.set('Debe seleccionar un equipo.');

    this.guardando.set(true);
    if (this.modo === 'crear') {
      this.svc.create({ nombre: this.nombre.trim(), equipoId: this.equipoId, posicion: this.posicion?.trim() || undefined })
        .subscribe({
          next: dto => { this.guardando.set(false); this.saved.emit(dto); },
          error: err => { this.guardando.set(false); this.errorMsg.set(err?.error?.message || 'No se pudo crear el jugador.'); }
        });
    } else if (this.inicial) {
      this.svc.update(this.inicial.id, { nombre: this.nombre.trim(), equipoId: this.equipoId, posicion: this.posicion?.trim() || undefined })
        .subscribe({
          next: dto => { this.guardando.set(false); this.saved.emit(dto); },
          error: err => { this.guardando.set(false); this.errorMsg.set(err?.error?.message || 'No se pudo actualizar el jugador.'); }
        });
    }
  }

  cancelar() { this.cancelled.emit(); }
}

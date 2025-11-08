import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EquiposService } from '../../servicios/equipos.service';
import { EquipoAdminDto } from '../../modelos/equipo-admin';

@Component({
  selector: 'app-equipo-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './equipo-form.component.html',
  styleUrls: ['./equipo-form.component.scss']
})
export class EquipoFormComponent {
  @Input() modo: 'crear' | 'editar' = 'crear';
  @Input() inicial: EquipoAdminDto | null = null;
  @Output() saved = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  nombre = '';
  ciudad = '';
  logoFile: File | null = null;
  logoPreview: string | null = null;

  guardando = signal(false);
  errorMsg = signal<string | null>(null);

  constructor(private svc: EquiposService) {}

  ngOnInit() {
    if (this.inicial) {
      this.nombre = this.inicial.nombre ?? '';
      this.ciudad = this.inicial.ciudad ?? '';
      // si tienes url del logo en el DTO
      // this.logoPreview = this.inicial.logoUrl ?? null;
    }
  }

  onFileChange(ev: Event) {
    const input = ev.target as HTMLInputElement;
    const f = input.files?.[0] ?? null;
    this.logoFile = f;
    this.logoPreview = f ? URL.createObjectURL(f) : null;
  }

  submit() {
    this.errorMsg.set(null);

    const nombre = this.nombre.trim();
    const ciudad = this.ciudad.trim();
    if (!nombre || !ciudad) {
      this.errorMsg.set('Nombre y ciudad son obligatorios.');
      return;
    }

    const fd = new FormData();
    // Aceptamos mayúsculas o minúsculas en el backend, usamos estas claves:
    fd.append('Nombre', nombre);
    fd.append('Ciudad', ciudad);
    if (this.logoFile) fd.append('Logo', this.logoFile);

    this.guardando.set(true);
    const req$ = this.modo === 'crear'
      ? this.svc.create(fd)
      : this.svc.update(this.inicial!.id, fd);

    req$.subscribe({
      next: () => { this.guardando.set(false); this.saved.emit(); },
      error: (e) => {
        this.guardando.set(false);
        this.errorMsg.set(e?.error?.message || 'No se pudo guardar el equipo.');
      }
    });
  }

  cancelar() { this.cancelled.emit(); }
}

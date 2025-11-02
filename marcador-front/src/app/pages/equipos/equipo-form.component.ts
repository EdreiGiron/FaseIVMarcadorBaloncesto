import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { EquiposService } from '../../servicios/equipos.service';
import { EquipoAdminDto } from '../../modelos/equipo-admin';

type Modo = 'crear' | 'editar';

@Component({
  selector: 'app-equipo-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './equipo-form.component.html',
  styleUrls: ['./equipo-form.component.scss'],
})
export class EquipoFormComponent implements OnInit {
  @Input() modo: Modo = 'crear';
  @Input() inicial?: EquipoAdminDto | null = null;

  @Output() saved = new EventEmitter<EquipoAdminDto>();
  @Output() cancelled = new EventEmitter<void>();

  nombre = '';
  ciudad = '';
  logoFile: File | null = null;
  logoPreview: string | null = null;

  guardando = signal(false);
  errorMsg = signal<string | null>(null);

  constructor(private svc: EquiposService) {}

  ngOnInit(): void {
    if (this.inicial) {
      this.nombre = this.inicial.nombre ?? '';
      this.ciudad = this.inicial.ciudad ?? '';
      this.logoPreview = this.inicial.logoUrl ?? null;
    }
  }

  onFileChange(ev: Event) {
    const input = ev.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    this.logoFile = file || null;

    // preview
    if (file) {
      const reader = new FileReader();
      reader.onload = () => (this.logoPreview = reader.result as string);
      reader.readAsDataURL(file);
    } else {
      this.logoPreview = this.inicial?.logoUrl ?? null;
    }
  }

  submit() {
    this.errorMsg.set(null);

    // Validaciones mínimas
    if (!this.nombre || this.nombre.trim().length < 2) {
      this.errorMsg.set('El nombre es obligatorio (mínimo 2 caracteres).');
      return;
    }
    if (!this.ciudad || this.ciudad.trim().length < 2) {
      this.errorMsg.set('La ciudad es obligatoria (mínimo 2 caracteres).');
      return;
    }

    this.guardando.set(true);

    if (this.modo === 'crear') {
      this.svc
        .create({ nombre: this.nombre.trim(), ciudad: this.ciudad.trim(), logo: this.logoFile })
        .subscribe({
          next: (dto) => {
            this.guardando.set(false);
            this.saved.emit(dto);
          },
          error: (err) => {
            const msg = err?.error?.message || 'No se pudo crear el equipo.';
            this.errorMsg.set(msg);
            this.guardando.set(false);
          },
        });
    } else if (this.modo === 'editar' && this.inicial) {
      this.svc
        .update(this.inicial.id, { nombre: this.nombre.trim(), ciudad: this.ciudad.trim(), logo: this.logoFile })
        .subscribe({
          next: (dto) => {
            this.guardando.set(false);
            this.saved.emit(dto);
          },
          error: (err) => {
            const msg = err?.error?.message || 'No se pudo actualizar el equipo.';
            this.errorMsg.set(msg);
            this.guardando.set(false);
          },
        });
    }
  }

  cancelar() {
    this.cancelled.emit();
  }
}

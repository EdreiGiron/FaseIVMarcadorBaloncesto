import { Component, Input, Output, EventEmitter, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RolesService, Role } from '../../servicios/roles.service';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-roles.lista.component',
  standalone: true,
  imports: [CommonModule,RouterModule],
  templateUrl: './roles.lista.component.html',
  styleUrl: './roles.lista.component.css'
})
export class RolesListaComponent {
  roles = signal<Role[]>([]);
  cargando = signal(false);
  error = signal('');

  constructor(private service: RolesService) {
    this.cargar();
  }

  cargar() {
    this.cargando.set(true);
    this.service.listar().subscribe({
      next: res => {
        this.roles.set(res);
        this.cargando.set(false);
      },
      error: () => {
        this.error.set('❌ Error cargando roles');
        this.cargando.set(false);
      }
    });
  }

  eliminar(role: Role) {
    if (!confirm(`¿Eliminar el rol ${role.name}?`)) return;
    this.service.eliminar(role.id).subscribe({
      next: () => this.cargar(),
      error: () => this.error.set('❌ Error eliminando rol')
    });
  }
}

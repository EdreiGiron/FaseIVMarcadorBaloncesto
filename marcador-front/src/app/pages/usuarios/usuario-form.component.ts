import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UsuariosService, Usuario } from '../../servicios/usuarios.service';
import { RolesService, Role } from '../../servicios/roles.service';

@Component({
  selector: 'app-usuario-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './usuario-form.component.html',
  styleUrls: ['./usuario-form.component.css']
})
export class UsuarioFormComponent {
  @Input() usuario: Partial<Usuario & { password?: string; roleId?: number }> = {};
  @Output() saved = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();
  mostrarForm = false;
  usuarioSeleccionado: any = {};

  roles: Role[] = [];

  constructor(
    private usuariosService: UsuariosService,
    private rolesService: RolesService
  ) { }

  ngOnInit() {
    this.rolesService.listar().subscribe({
      next: roles => (this.roles = roles),
      error: () => console.error('❌ Error cargando roles')
    });
  }

  guardar() {
    if (this.usuario.id) {
      // Actualizar
      this.usuariosService.actualizar(this.usuario.id, this.usuario).subscribe({
        next: () => this.saved.emit(),
        error: () => alert('❌ Error actualizando usuario')
      });
    } else {
      // Crear
      const payload = {
        username: this.usuario.username!,
        password: this.usuario['password'] || '123456',
        roleId: this.usuario['roleId'] || 1
      };
      this.usuariosService.crear(payload).subscribe({
        next: () => this.saved.emit(),
        error: () => alert('❌ Error creando usuario')
      });
    }
  }
}

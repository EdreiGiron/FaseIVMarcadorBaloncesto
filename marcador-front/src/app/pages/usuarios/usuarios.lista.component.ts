import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UsuariosService, Usuario } from '../../servicios/usuarios.service';
import { RouterModule } from '@angular/router';
import { UsuarioFormComponent } from './usuario-form.component';

@Component({
  selector: 'app-usuario-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, UsuarioFormComponent],
  templateUrl: './usuarios.lista.component.html',
  styleUrls: ['./usuarios.lista.component.css']
})
export class UsuariosListaComponent {
  usuarios = signal<Usuario[]>([]);
  cargando = signal(false);
  mostrarForm = false;
  error = signal('');
  search = '';
  usuarioSeleccionado: Partial<Usuario> = {};


  constructor(private service: UsuariosService) {
    this.cargar();
  }

  onSaved() {
    this.cargar();
    this.mostrarForm = false;
  }
  cargar() {
    this.cargando.set(true);
    this.service.listPaged({ page: 1, pageSize: 10, search: this.search }).subscribe({
      next: res => {
        this.usuarios.set(res.items);
        this.cargando.set(false);
      },
      error: () => {
        this.error.set('❌ Error cargando usuarios');
        this.cargando.set(false);
      }
    });
  }

  nuevoUsuario() {
    this.usuarioSeleccionado = {};   // usuario vacío
    this.mostrarForm = true;
  }

  editar(usuario: Usuario) {
    this.usuarioSeleccionado = { ...usuario };  // clona datos para editar
    this.mostrarForm = true;
  }

  eliminar(usuario: Usuario) {
    if (!confirm(`¿Eliminar al usuario ${usuario.username}?`)) return;
    this.service.eliminar(usuario.id).subscribe({
      next: () => this.cargar(),
      error: () => this.error.set('❌ Error eliminando usuario')
    });
  }
}

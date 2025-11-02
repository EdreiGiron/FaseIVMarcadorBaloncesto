import { Component, Input, Output, EventEmitter, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RolesService, Role } from '../../servicios/roles.service';

@Component({
  selector: 'app-role-form.component',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './role-form.component.html',
  styleUrl: './role-form.component.css'
})
export class RoleFormComponent {
  @Input() role: Partial<Role> = {};
  @Output() saved = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  constructor(private rolesService: RolesService) { }

  guardar() {
    if (this.role.id) {
      alert('⚠️ La edición de roles no está implementada aún.');
      this.saved.emit();
    } else {
      this.rolesService.crear({ name: this.role.name! }).subscribe({
        next: () => this.saved.emit(),
        error: () => alert('❌ Error creando rol')
      });
    }
  }
}

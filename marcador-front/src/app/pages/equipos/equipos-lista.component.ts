import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EquiposService } from '../../servicios/equipos.service';
import { EquipoAdminDto } from '../../modelos/equipo-admin';
import { EquipoFormComponent } from './equipo-form.component';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-equipos-lista',
  standalone: true,
  imports: [CommonModule, FormsModule, EquipoFormComponent, RouterModule],
  templateUrl: './equipos-lista.component.html',
  styleUrls: ['./equipos-lista.component.scss'],
})
export class EquiposListaComponent implements OnInit {
  private svc = inject(EquiposService);

  // filtros
  search = '';
  ciudad = '';

  // estado UI
  cargando = signal(false);
  errorMsg = signal<string | null>(null);
  equipos = signal<EquipoAdminDto[]>([]);

  // modal
  modalVisible = signal(false);
  modo: 'crear' | 'editar' = 'crear';
  seleccionado: EquipoAdminDto | null = null;

  ngOnInit(): void {
    this.cargarLista();
  }


page = 1;
pageSize = 5;
totalItems = 0;
sortBy: 'nombre'|'ciudad'|'puntos'|'faltas' = 'nombre';
sortDir: 'asc'|'desc' = 'asc';

private cargarLista(): void {
  this.cargando.set(true);
  this.errorMsg.set(null);

  this.svc.listPaged({
    page: this.page,
    pageSize: this.pageSize,
    search: this.search?.trim() || undefined,
    ciudad: this.ciudad?.trim() || undefined,
    sortBy: this.sortBy,
    sortDir: this.sortDir
  }).subscribe({
    next: (res: any) => {
      // Log para confirmar qué llega del backend
      console.log('GET /api/equipos/paged =>', res);

      
      const items = Array.isArray(res) ? res : (res?.items ?? []);
      const total = Array.isArray(res) ? res.length : (res?.totalItems ?? items.length);

      this.equipos.set(items);
      this.totalItems = total;
      this.cargando.set(false);
    },
    error: (err) => {
      
      console.warn('paged falló, probando GET simple...', err);
      this.svc.list(this.search?.trim() || undefined, this.ciudad?.trim() || undefined)
        .subscribe({
          next: (arr) => {
            console.log('GET /api/equipos =>', arr);
            this.equipos.set(arr);
            this.totalItems = arr.length;
            this.cargando.set(false);
          },
          error: (e2) => {
            const status = e2?.status;
            const msg = e2?.error?.message || `Error al listar equipos (HTTP ${status ?? '??'})`;
            this.errorMsg.set(msg);
            this.cargando.set(false);
          }
        });
    }
  });
}

get rangeStart(): number {
  if (this.totalItems === 0) return 0;
  return (this.page - 1) * this.pageSize + 1;
}

get rangeEnd(): number {
  return Math.min(this.page * this.pageSize, this.totalItems);
}


// UI helpers
get totalPages() { return Math.max(1, Math.ceil(this.totalItems / this.pageSize)); }
goPage(p: number) { this.page = Math.min(Math.max(1, p), this.totalPages); this.cargarLista(); }
changePageSize(n: number) { this.pageSize = n; this.page = 1; this.cargarLista(); }
toggleSort(field: 'nombre'|'ciudad'|'puntos'|'faltas') {
  if (this.sortBy === field) this.sortDir = this.sortDir === 'asc' ? 'desc' : 'asc';
  else { this.sortBy = field; this.sortDir = 'asc'; }
  this.cargarLista();
}


  onBuscarChange(): void {
    
    clearTimeout((this as any)._deb);
    (this as any)._deb = setTimeout(() => this.cargarLista(), 300);
  }

  limpiarFiltros(): void {
    this.search = '';
    this.ciudad = '';
    this.cargarLista();
  }

  // Acciones
  crear(): void {
    this.modo = 'crear';
    this.seleccionado = null;
    this.modalVisible.set(true);
  }

  editar(item: EquipoAdminDto): void {
    this.modo = 'editar';
    this.seleccionado = item;
    this.modalVisible.set(true);
  }

eliminar(item: EquipoAdminDto) {
  const seguro = confirm(`¿Eliminar el equipo "${item.nombre}"? Esta acción es irreversible.`);
  if (!seguro) return;

  this.cargando.set(true);
  this.svc.delete(item.id).subscribe({
    next: () => this.cargarLista(),
    error: (err) => {
      const status = err?.status;
      const msg = status === 409
        ? (err?.error?.message || 'El equipo está en uso y no puede eliminarse.')
        : (err?.error?.message || 'No se pudo eliminar el equipo.');
      this.errorMsg.set(msg);          
      this.cargando.set(false);
    }
  });
}

  onFormSaved(_: EquipoAdminDto) {
    this.modalVisible.set(false);
    this.cargarLista();
  }

  onFormCancelled() {
    this.modalVisible.set(false);
  }

  trackById(_i: number, e: EquipoAdminDto) { return e.id; }
  
}


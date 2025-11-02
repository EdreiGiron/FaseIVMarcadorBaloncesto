import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { JugadoresService } from '../../servicios/jugadores.service';
import { EquiposService } from '../../servicios/equipos.service';
import { JugadorAdminDto } from '../../modelos/jugador-admin';
import { EquipoAdminDto } from '../../modelos/equipo-admin';
import { JugadorFormComponent } from './jugador-form.component';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-jugadores-lista',
  standalone: true,
  imports: [CommonModule, FormsModule, JugadorFormComponent, RouterModule],
  templateUrl: './jugadores-lista.component.html',
  styleUrls: ['./jugadores-lista.component.scss']
})
export class JugadoresListaComponent implements OnInit {
  private svc = inject(JugadoresService);
  private equiposSvc = inject(EquiposService);

  // filtros
  search = ''; equipoNombre = ''; posicion = ''; equipoId: number | null = null;

  // paginado/orden
  page = 1; pageSize = 10; totalItems = 0;
  sortBy: 'nombre'|'equipo'|'posicion'|'puntos'|'faltas' = 'nombre';
  sortDir: 'asc'|'desc' = 'asc';

  // estado UI
  cargando = signal(false);
  errorMsg = signal<string | null>(null);
  jugadores = signal<JugadorAdminDto[]>([]);
  equipos = signal<EquipoAdminDto[]>([]);

  // modal
  modalVisible = signal(false);
  modo: 'crear'|'editar' = 'crear';
  seleccionado: JugadorAdminDto | null = null;

  ngOnInit(): void {
    this.equiposSvc.list().subscribe(e => this.equipos.set(e));
    this.cargarLista();
  }

cargarLista(): void {
  this.cargando.set(true);
  this.errorMsg.set(null);
  this.svc.listPaged({
    page: this.page, pageSize: this.pageSize,
    search: this.search?.trim() || undefined,
    equipoNombre: this.equipoNombre?.trim() || undefined,
    equipoId: this.equipoId ?? undefined,
    posicion: this.posicion?.trim() || undefined,
    sortBy: this.sortBy, sortDir: this.sortDir
  }).subscribe({
    next: res => { this.jugadores.set(res.items); this.totalItems = res.totalItems; this.cargando.set(false); },
    error: err => { this.errorMsg.set(err?.error?.message || 'Error al listar jugadores.'); this.cargando.set(false); }
  });
}
  // pager helpers
  get hasRows(): boolean { return this.jugadores().length > 0; }
  get totalPages() { return Math.max(1, Math.ceil(this.totalItems / this.pageSize)); }
  get rangeStart() { return this.totalItems ? (this.page-1)*this.pageSize + 1 : 0; }
  get rangeEnd()   { return Math.min(this.page*this.pageSize, this.totalItems); }
  goPage(p: number) { this.page = Math.min(Math.max(1,p), this.totalPages); this.cargarLista(); }
  changePageSize(n: number | string) { this.pageSize = Number(n); this.page = 1; this.cargarLista(); }
  toggleSort(f: 'nombre'|'equipo'|'posicion'|'puntos'|'faltas'){ if(this.sortBy===f)this.sortDir=this.sortDir==='asc'?'desc':'asc'; else{this.sortBy=f;this.sortDir='asc';} this.cargarLista(); }

  onBuscarChange(){ clearTimeout((this as any)._deb); (this as any)._deb=setTimeout(()=>this.cargarLista(),300); }
  limpiarFiltros(){ this.search=''; this.equipoNombre=''; this.posicion=''; this.equipoId=null; this.page=1; this.cargarLista(); }

  crear(){ this.modo='crear'; this.seleccionado=null; this.modalVisible.set(true); }
  editar(j: JugadorAdminDto){ this.modo='editar'; this.seleccionado=j; this.modalVisible.set(true); }
  
eliminar(j: JugadorAdminDto) {
  const seguro = confirm(`¿Eliminar al jugador "${j.nombre}"? Esta acción es irreversible.`);
  if (!seguro) return;

  this.cargando.set(true);
  this.svc.delete(j.id).subscribe({
    next: () => this.cargarLista(),  
    error: (err) => {
      const status = err?.status;

      // registrarerrores que puedan generarse, mesajes claros
      const msg = status === 409
        ? (err?.error?.message || 'El jugador no puede eliminarse porque está en uso.')
        : (err?.error?.message || 'No se pudo eliminar el jugador.');

      this.errorMsg.set(msg);   
      this.cargando.set(false);
    }
  });
}

  onFormSaved(){ this.modalVisible.set(false); this.cargarLista(); }
  onFormCancelled(){ this.modalVisible.set(false); }

  trackById(_i:number, j:JugadorAdminDto){ return j.id; }
}

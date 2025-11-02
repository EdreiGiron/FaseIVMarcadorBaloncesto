import { Routes } from '@angular/router';

// Guards
import { authGuard } from './guards/auth-guard';
import { RoleGuard } from './guards/role-guard';

// Componentes directos
import { TableroComponent } from './pages/tablero.component';
import { Login } from './pages/login/login';
import { RegistroUsuarios } from './pages/registro-usuarios/registro-usuarios';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },

  // Públicas
  { path: 'login', component: Login, title: 'Iniciar sesión' },
  { path: 'register', component: RegistroUsuarios, title: 'Crear cuenta' },

  // Dashboard (admin)
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./pages/dashboard/dashboard.component')
        .then(m => m.DashboardComponent),
    title: 'Panel de Administración',
    canActivate: [authGuard]
  },

  // Operador (marcador)
  { path: 'control', component: TableroComponent, canActivate: [authGuard], title: 'Tablero de control' },
  { path: 'control/:id', component: TableroComponent, canActivate: [authGuard], title: 'Tablero de control' },

  // Tablero público
  {
    path: 'publico',
    loadComponent: () =>
      import('./publico/tablero-publico.component')
        .then(m => m.TableroPublicoComponent),
    title: 'Tablero público'
  },
  {
    path: 'publico/:id',
    loadComponent: () =>
      import('./publico/tablero-publico.component')
        .then(m => m.TableroPublicoComponent),
    title: 'Tablero público'
  },

  // ADMIN (todas requieren Admin)
  {
    path: 'admin/equipos',
    loadComponent: () => import('./pages/equipos/equipos-lista.component')
      .then(m => m.EquiposListaComponent),
    title: 'Gestión de Equipos',
    canActivate: [authGuard, RoleGuard],
    data: { roles: ['Admin'] }
  },
  {
    path: 'admin/jugadores',
    loadComponent: () => import('./pages/jugadores/jugadores-lista.component')
      .then(m => m.JugadoresListaComponent),
    title: 'Gestión de Jugadores',
    canActivate: [authGuard, RoleGuard],
    data: { roles: ['Admin'] }
  },
  {
    path: 'admin/usuarios',
    loadComponent: () => import('./pages/usuarios/usuarios.lista.component')
      .then(m => m.UsuariosListaComponent),
    title: 'Gestión de Usuarios',
    canActivate: [authGuard, RoleGuard],
    data: { roles: ['Admin'] }
  },
  {
    path: 'admin/roles',
    loadComponent: () => import('./pages/roles/roles.lista.component')
      .then(m => m.RolesListaComponent),
    title: 'Gestión de Roles',
    canActivate: [authGuard, RoleGuard],
    data: { roles: ['Admin'] }
  },

  // Partidos
  {
    path: 'admin/partidos/historial',
    loadComponent: () => import('./pages/partidos/partidos-historial.component')
      .then(m => m.PartidosHistorialComponent),
    title: 'Historial de Partidos',
    canActivate: [authGuard, RoleGuard],
    data: { roles: ['Admin'] }
  },
  {
    path: 'admin/partidos/nuevo',
    loadComponent: () => import('./pages/partidos/partidos-nuevo.component')
      .then(m => m.PartidosNuevoComponent),
    title: 'Nuevo Partido',
    canActivate: [authGuard, RoleGuard],
    data: { roles: ['Admin'] }
  },
  {
    path: 'admin/partidos/:id/roster',
    loadComponent: () => import('./pages/partidos/partido-roster.component')
      .then(m => m.PartidoRosterComponent),
    title: 'Asignar Roster',
    canActivate: [authGuard, RoleGuard],
    data: { roles: ['Admin'] }
  },

  // Torneos
  {
    path: 'admin/torneos/nuevo',
    loadComponent: () => import('./pages/torneos/torneos-nuevo.component')
      .then(m => m.TorneosNuevoComponent),
    title: 'Nuevo Torneo',
    canActivate: [authGuard, RoleGuard],
    data: { roles: ['Admin'] }
  },
  {
    path: 'admin/torneos/:id/bracket',
    loadComponent: () => import('./pages/torneos/bracket.component')
      .then(m => m.BracketComponent),
    title: 'Llaves del Torneo',
    canActivate: [authGuard, RoleGuard],
    data: { roles: ['Admin'] }
  },

  // Reportes
  {
    path: 'admin/reportes',
    loadComponent: () =>
      import('./pages/admin/reportes.component/reportes.component')
        .then(m => m.ReportesComponent),
    title: 'Reportes',
    canActivate: [authGuard, RoleGuard],
    data: { roles: ['Admin'] }
  },

  { path: '**', redirectTo: 'login' }
];

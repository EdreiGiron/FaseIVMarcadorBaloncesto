import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Location } from '@angular/common';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Global } from '../../../servicios/global';

type Team    = { id: number; nombre: string };
type Player  = { id: number; nombre: string; equipoId: number };
type Season  = { id: number; nombre: string };

// Opción para el <select> de partidos (id + etiqueta visible)
type MatchOption = { id: number; label: string };

@Component({
  selector: 'app-reportes',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reportes.component.html',
  styleUrls: ['./reportes.component.css']
})
export class ReportesComponent implements OnInit {
  // Datos
  teams: Team[] = [];
  players: Player[] = [];
  partidos: MatchOption[] = [];
  seasons: Season[] = [];

  // Estado UI
  selectedTeamId: number | null = null;           // para "Jugadores por equipo"
  selectedTeamIdForStats: number | null = null;   // para "Estadísticas por jugador"
  selectedPlayerId: number | null = null;
  temporadaId: number | null = null;
  partidoId: number | null = null;

  constructor(
    private http: HttpClient,
    private location: Location,
  ) {}

  ngOnInit(): void {
    this.loadTeams();
    this.loadPartidos();
    this.loadSeasons();
  }

  // ---------- Carga de datos ----------
  private loadTeams() {
    this.http.get<Team[]>('/api/equipos').subscribe({
      next: (data) => (this.teams = data ?? []),
      error: (err) => console.error('Error equipos:', err)
    });
  }

  private loadSeasons() {
    this.http.get<Season[]>('/api/Torneos').subscribe({
      next: (data) => (this.seasons = data ?? []),
      error: (err) => console.error('Error temporadas:', err)
    });
  }

  /**
   * Intenta /api/Partidos/historial; si no existe o devuelve otra forma, prueba historial paginado.
   * Normaliza a { id, label } para el select.
   */
  private loadPartidos() {
    this.http.get<any>('/api/Partidos/historial').subscribe({
      next: (res) => {
        const list = Array.isArray(res) ? res : (res?.items ?? []);
        this.partidos = (list ?? [])
          .map((p: any) => this.toMatchOption(p))
          .filter((m: MatchOption) => !!m.id);
      },
      error: () => {
        // Fallback: historial (paginado)
        const params = new HttpParams().set('page', 1).set('pageSize', 100);
        this.http.get<any>('/api/Partidos/historial', { params }).subscribe({
          next: (res2) => {
            const list2 = Array.isArray(res2) ? res2 : (res2?.items ?? []);
            this.partidos = (list2 ?? [])
              .map((p: any) => this.toMatchOption(p))
              .filter((m: MatchOption) => !!m.id);
          },
          error: (err) => {
            console.error('Error partidos:', err);
            this.partidos = [];
          }
        });
      }
    });
  }

  private toMatchOption(p: any): MatchOption {
    const id =
      p?.id ?? p?.Id ?? p?.partidoId ?? p?.PartidoId ?? null;

    const local =
      p?.equipoLocalNombre ?? p?.EquipoLocalNombre ?? p?.equipoLocalId ?? p?.EquipoLocalId ?? 'Local';
    const visit =
      p?.equipoVisitanteNombre ?? p?.EquipoVisitanteNombre ?? p?.equipoVisitanteId ?? p?.EquipoVisitanteId ?? 'Visitante';
    const hora =
      p?.fechaHora ?? p?.FechaHora ?? p?.fecha ?? p?.Fecha ?? '';

    const label = `${local} vs ${visit}${hora ? ` (${hora})` : ''}`;
    return { id, label };
  }

  onTeamChange(id: number | null) {
    this.selectedTeamId = id ?? null;
  }

  onTeamChangeForStats(id: number | null) {
    this.selectedTeamIdForStats = id ?? null;
    this.selectedPlayerId = null;
    this.players = [];

    if (id == null) return;

    const teamId = Number(id); // asegurar número
    const params = new HttpParams().set('equipoId', teamId);
    this.http.get<Player[]>('/api/jugadores', { params }).subscribe({
      next: (data) => (this.players = data ?? []),
      error: (err) => {
        console.error('Error jugadores:', err);
        this.players = [];
      }
    });
  }

  // ---------- Navegación ----------
  goBack() { this.location.back(); }

  // ---------- Helpers ----------
  private open(path: string) {
    const base = Global.reportesUrl.replace(/\/$/, ''); // '/api/reportes'
    const p = path.startsWith('/') ? path : `/${path}`; // conserva '/pdf/...'
    window.open(`${base}${p}`, '_blank', 'noopener');   // => '/api/reportes/pdf/...'
  }

  // ---------- Acciones (descarga PDF) ----------
  openEquipos() { this.open('/pdf/equipos'); }

  openJugadoresPorEquipo() {
    if (this.selectedTeamId == null) return;
    this.open(`/pdf/jugadores-por-equipo?equipoId=${encodeURIComponent(this.selectedTeamId)}`);
  }

  openHistorial() {
    const q = this.temporadaId != null ? `?temporadaId=${encodeURIComponent(this.temporadaId)}` : '';
    this.open(`/pdf/historial-partidos${q}`);
  }

  openRoster() {
    if (this.partidoId == null) return;
    this.open(`/pdf/roster?partidoId=${encodeURIComponent(this.partidoId)}`);
  }

  openScouting() {
    if (this.selectedPlayerId == null) return;
    this.open(`/pdf/scouting?jugadorId=${encodeURIComponent(this.selectedPlayerId)}`);
  }

  // Para *ngFor
  trackById = (_: number, it: { id: number }) => it.id;
}

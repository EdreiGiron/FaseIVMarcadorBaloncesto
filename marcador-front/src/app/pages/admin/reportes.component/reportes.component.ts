import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { Location } from '@angular/common';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Global } from '../../../servicios/global';

type Team = { id: number; nombre: string };
type Player = { id: number; nombre: string; equipoId: number };
type Season = { id: number; nombre: string };

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
  ) { }

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

  // ---------- Descarga de PDFs (nuevo) ----------
  /** Une base + path sin duplicar /pdf cuando Global.reportesUrl ya termina en /pdf */
  private buildReportUrl(path: string): string {
    const base = (Global.reportesUrl || '').replace(/\/$/, '');
    if (!base) return path;                       // si no hay base, usa path tal cual (/pdf/...)
    if (base.endsWith('/pdf') && path.startsWith('/pdf')) return path; // evita /pdf/pdf/...
    return `${base}${path}`;
  }

  /** Descarga genérica con HttpClient (usa interceptor para el Bearer) */
  private descargar(
    path: string,
    fileNameFallback: string,
    params?: Record<string, string | number | undefined>
  ) {
    const url = this.buildReportUrl(path);

    let httpParams = new HttpParams();
    if (params) {
      for (const [k, v] of Object.entries(params)) {
        if (v !== undefined && v !== null) httpParams = httpParams.set(k, String(v));
      }
    }

    this.http.get(url, { params: httpParams, responseType: 'blob', observe: 'response' as const })
      .subscribe({
        next: async (res) => {
          const ct = (res.headers.get('Content-Type') || '').toLowerCase();
          const blob = res.body!;

          // Si NO es PDF, mostramos el texto de error (login, JSON, etc.) y no descargamos
          if (!ct.includes('application/pdf')) {
            const text = await new Response(blob).text().catch(() => '(sin cuerpo)');
            console.error('El backend no devolvió PDF:', ct, text);
            alert(
              `No se generó el PDF (Content-Type: ${ct}).\n\n` +
              `Respuesta del servidor:\n${text.slice(0, 400)}`
            );
            return;
          }

          // Intentar nombre desde Content-Disposition
          let fileName = fileNameFallback;
          const cd = res.headers.get('Content-Disposition');
          const m = cd && /filename\*=UTF-8''([^;]+)|filename="?([^"]+)"?/i.exec(cd);
          if (m) fileName = decodeURIComponent(m[1] || m[2]);

          const a = document.createElement('a');
          a.href = URL.createObjectURL(blob);
          a.download = fileName;
          a.rel = 'noopener';
          document.body.appendChild(a);
          a.click();
          a.remove();
          URL.revokeObjectURL(a.href);
        },
        error: (err) => {
          console.error('Error al descargar PDF:', err);
          alert('No se pudo generar el PDF. Revisa tu sesión y que el servicio de reportes esté arriba.');
        }
      });
  }

  // ---------- Acciones (descarga PDF) ----------
  openEquipos() {
    this.descargar('/pdf/equipos', 'Equipos_Registrados.pdf');
  }

  openJugadoresPorEquipo() {
    if (this.selectedTeamId == null) return;
    this.descargar(
      '/pdf/jugadores-por-equipo',
      `JugadoresXEquipo_${this.selectedTeamId}.pdf`,
      { equipoId: this.selectedTeamId }
    );
  }

  openHistorial() {
    this.descargar(
      '/pdf/historial-partidos',
      'Historial_Partidos.pdf',
      this.temporadaId != null ? { temporadaId: this.temporadaId } : undefined
    );
  }

  openRoster() {
    if (this.partidoId == null) return;
    this.descargar(
      '/pdf/roster',
      `Roster_Partido_${this.partidoId}.pdf`,
      { partidoId: this.partidoId }
    );
  }

  openScouting() {
    if (this.selectedPlayerId == null) return;
    this.descargar(
      '/pdf/scouting',
      `estadisticas_jugador_${this.selectedPlayerId}.pdf`,
      { jugadorId: this.selectedPlayerId }
    );
  }

  // Para *ngFor
  trackById = (_: number, it: { id: number }) => it.id;
}

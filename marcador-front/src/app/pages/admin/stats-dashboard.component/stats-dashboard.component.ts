import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { Location } from '@angular/common';

type Team = { id: number; nombre: string };
type Player = {
  id: number;
  nombre: string;
  posicion?: string | null;
  equipoId: number;
  equipoNombre?: string;
  fotoUrl?: string | null;
 
  puntos?: number;
  faltas?: number;
};

type Metric = {
  id?: number;
  key: 'puntos' | 'faltas';
  label: string;
  field: keyof Player; // 'puntos' | 'faltas'
  decimals?: number;
};

@Component({
  selector: 'app-stats-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './stats-dashboard.component.html',
  styleUrls: ['./stats-dashboard.component.css'],
})
export class StatsDashboardComponent implements OnInit {
  teams: Team[] = [];
  selectedTeamId: number | null = null;

  metrics: Metric[] = [
    { key: 'puntos', label: 'Puntos por juego', field: 'puntos', decimals: 1 },
    { key: 'faltas', label: 'Faltas personales', field: 'faltas', decimals: 1 },
    // cuando agregues rebotes/asistencias, solo añade aquí:
    // { key: 'rebotes', label: 'Rebotes', field: 'rebotes', decimals: 1 },
  ];
  selectedMetric: Metric = this.metrics[0];

  leadersTop3: Player[] = [];
  leaderboard: Player[] = [];

  isLoading = false;
  errorMsg = '';

  constructor(private http: HttpClient, private location: Location) {}

  ngOnInit(): void {
    this.loadTeams();
    this.loadLeaders();
  }

  goBack() {
   
    if (window.history.length > 1) this.location.back();
    else window.location.href = '/admin';
  }

  trackById = (_: number, it: any) => it.id || it.key;

  onTeamChange(_: any) {
    this.loadLeaders();
  }
  onMetricChange(m: Metric) {
    this.selectedMetric = m;
    this.loadLeaders();
  }

  private loadTeams() {
    this.http.get<Team[]>('/api/equipos').subscribe({
      next: (data) => (this.teams = data ?? []),
      error: () => (this.teams = []),
    });
  }


  private loadLeaders() {
    this.isLoading = true;
    this.errorMsg = '';

    const equipoId = this.selectedTeamId ?? '';
    const urlApi = `/api/estadisticas/lideres?metric=${this.selectedMetric.key}${equipoId ? `&equipoId=${equipoId}` : ''}`;

    this.http.get<Player[]>(urlApi).subscribe({
      next: (arr) => {
        this.fillBoards(arr ?? []);
        this.isLoading = false;
      },
      error: () => {
       
        const urlJug = this.selectedTeamId
          ? `/api/jugadores?equipoId=${this.selectedTeamId}`
          : '/api/jugadores';

        this.http.get<Player[]>(urlJug).subscribe({
          next: (players) => {
            const field = this.selectedMetric.field;
            const clean = (players ?? []).map(p => ({
              ...p,
              puntos: p.puntos ?? (p as any).Puntos ?? 0,
              faltas: p.faltas ?? (p as any).Faltas ?? 0,
            }));
            const ordered = clean.sort((a, b) => (b[field] as number) - (a[field] as number));
            this.fillBoards(ordered);
            this.isLoading = false;
          },
          error: (err) => {
            this.errorMsg = 'No se pudieron cargar datos.';
            this.isLoading = false;
            console.error(err);
          },
        });
      },
    });
  }

  private fillBoards(list: Player[]) {
    this.leadersTop3 = list.slice(0, 3);
    this.leaderboard = list.slice(3, 13);
  }

 
  formatValue(value: any): string {
    const num = Number(value) || 0;
    const decimals = this.selectedMetric.decimals ? 1 : 0;
    return num.toFixed(decimals);
  }

  openLeadersPdf() {
    const equipo = this.selectedTeamId ? `&equipoId=${this.selectedTeamId}` : '';
    window.open(`/api/reportes/lideres?metric=${this.selectedMetric.key}${equipo}`, '_blank');
  }
}

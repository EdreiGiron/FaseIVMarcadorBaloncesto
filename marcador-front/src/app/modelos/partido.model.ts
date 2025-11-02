export type PartidoEstado = 'Programado'|'EnJuego'|'Finalizado'|'Pospuesto'|'Cancelado';

export interface Partido {
  id: number; torneoId: number; seriePlayoffId: number; gameNumber: number;
  fechaHora: string; estado: PartidoEstado;
  equipoLocalId: number; equipoVisitanteId: number;
  marcadorLocal?: number; marcadorVisitante?: number;
  ronda: 'Final'|'Semifinal'|'Cuartos'|'Octavos';
  seedA: number; seedB: number;
}

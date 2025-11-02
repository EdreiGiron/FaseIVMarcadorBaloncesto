export interface Torneo {
  id: number; nombre: string; temporada: number; bestOf: number; estado: 'Planificado'|'Activo'|'Finalizado';
}

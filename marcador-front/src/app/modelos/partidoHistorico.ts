export interface PartidoHistorico {
  id: number;
  fecha: string;        
  estado: string;
  equipoLocalId: number;
  equipoVisitanteId: number;
  puntosLocal: number;
  puntosVisitante: number;
  observaciones: string;
}
export interface Falta {
  id: number;
  jugadorId: number;
  equipoId: number;
  tipo: string;   // personal, técnica, etc.
  minuto: number;
}

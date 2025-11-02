import { Equipo } from './equipo';

export interface Partido {
  equipoLocal: Equipo;
  equipoVisitante: Equipo;
  // Campos opcionales por si ya los manejas:
  cuartoActual?: number;
  tiempoRestante?: number;   // en segundos
  enProrroga?: boolean;
  numeroProrroga?: number;
  relojCorriendo?: boolean;
}
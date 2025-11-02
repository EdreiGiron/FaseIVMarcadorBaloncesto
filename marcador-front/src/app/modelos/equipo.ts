import { Jugador } from './jugador';

export interface Equipo {
  id: number;
  nombre: string;
  puntos: number;
  faltas: number;
  jugadores: Jugador[];
}

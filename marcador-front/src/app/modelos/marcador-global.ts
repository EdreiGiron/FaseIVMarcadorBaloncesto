// Ajusta la ruta si usas otra carpeta para modelos
export interface Equipo {
  nombre: string;
  puntos: number;
  faltas: number;
}

export interface MarcadorGlobal {
  id?: number;
  equipoLocal: Equipo;
  equipoVisitante: Equipo;
  cuartoActual: number;
  tiempoRestante: number;   // en segundos
  enProrroga: boolean;
  numeroProrroga: number;
  relojCorriendo: boolean; //  indica si el reloj está corriendo
}

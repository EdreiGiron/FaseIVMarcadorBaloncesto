export interface JugadorAdminDto {
  id: number;
  nombre: string;
  puntos: number;
  faltas: number;
  posicion?: string | null;
  equipoId: number;
  equipoNombre: string;
}

export interface JugadorCreateDto {
  nombre: string;
  equipoId: number;
  posicion?: string | null;
}

export interface JugadorUpdateDto {
  nombre: string;
  equipoId: number;
  posicion?: string | null;
}

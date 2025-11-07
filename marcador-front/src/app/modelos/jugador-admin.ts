export interface JugadorAdminDto {
  id: number;
  nombre: string;
  numero: number;
  puntos: number;
  faltas: number;
  posicion?: string | null;
  equipoId: number;
  equipoNombre: string;
}

export interface JugadorCreateDto {
  nombre: string;
  numero: number;
  equipoId: number;
  posicion?: string | null;
}

export interface JugadorUpdateDto {
  nombre: string;
  numero: number;
  equipoId: number;
  posicion?: string | null;
}

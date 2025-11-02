// src/app/modelos/equipo-admin.ts
export interface EquipoAdminDto {
  id: number;
  nombre: string;
  puntos: number;
  faltas: number;
  ciudad: string;           // en backend ya es NOT NULL
  logoUrl?: string | null;  // puede venir null si no hay logo
}

// Para formularios multipart/form-data
export interface EquipoCreateForm {
  nombre: string;
  ciudad: string;
  logo?: File | null;
}

export interface EquipoUpdateForm {
  nombre: string;
  ciudad: string;
  logo?: File | null;
}

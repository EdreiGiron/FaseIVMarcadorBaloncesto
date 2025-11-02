/**
 * Servicio para consultar y administrar roles.
 * Provee endpoints para listar y (si aplica) CRUD de roles.
 * Base URL: `Global.url`.
 */
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Global } from '../servicios/global';
import { Observable } from 'rxjs';

export interface Role {
  id: number;
  name: string;
}

@Injectable({
  providedIn: 'root'
})
export class RolesService {
  private url = `${Global.url}/roles`;

  constructor(private http: HttpClient) { }

  listar(): Observable<Role[]> {
    return this.http.get<Role[]>(this.url);
  }

  crear(role: { name: string }): Observable<Role> {
    return this.http.post<Role>(this.url, role);
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}

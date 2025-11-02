import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Global } from '../servicios/global';
import { Observable } from 'rxjs';
import { PagedResult } from '../modelos/paged';

export interface Usuario {
  id: number;
  username: string;
  roleName: string;
}


@Injectable({
  providedIn: 'root'
})
export class UsuariosService {

  private url = `${Global.url}/users`;

  constructor(private http: HttpClient) { }

  listPaged(opts: { page: number; pageSize: number; search?: string }): Observable<PagedResult<Usuario>> {
    let params = new HttpParams()
      .set('page', opts.page)
      .set('pageSize', opts.pageSize);
    if (opts.search) params = params.set('search', opts.search);

    return this.http.get<PagedResult<Usuario>>(`${this.url}/paged`, { params });
  }

  
  crear(usuario: { username: string; password: string; roleId: number }): Observable<Usuario> {
    return this.http.post<Usuario>(this.url, usuario);
  }

  actualizar(id: number, usuario: Partial<Usuario>): Observable<Usuario> {
    return this.http.put<Usuario>(`${this.url}/${id}`, usuario);
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}

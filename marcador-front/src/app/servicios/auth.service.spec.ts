/**
 * Servicio para administración de usuarios (listado paginado, CRUD, búsquedas).
 *
 * Endpoints típicos:
 * - GET /api/users/paged
 * - GET /api/users/{id}
 * - POST /api/users
 * - PUT /api/users/{id}
 * - DELETE /api/users/{id}
 *
 * Todas las URLs parten de `Global.url`.
 */
import { TestBed } from '@angular/core/testing';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AuthService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

# Sistema de Marcador de Baloncesto - Arquitectura de Microservicios

## Descripción General
Sistema completo de gestión de marcador de baloncesto desarrollado con arquitectura de microservicios, que incluye gestión de equipos, jugadores, partidos, torneos, autenticación y reportes.

## Arquitectura de Microservicios

### 1. **Frontend (Angular)**
- **Puerto**: 4200
- **Tecnología**: Angular 20.1.1
- **Descripción**: Interfaz de usuario principal del sistema
- **Dockerfile**: `./marcador-front/Dockerfile`
- **Configuración**: nginx.conf para servir la aplicación

### 2. **Auth Service (C# .NET)**
- **Puerto**: 8080
- **Tecnología**: ASP.NET Core
- **Base de Datos**: SQL Server (Puerto 14333)
- **Descripción**: Servicio de autenticación con JWT, OAuth (Google, GitHub)
- **Características**:
  - Autenticación JWT con RSA
  - OAuth con Google y GitHub
  - Refresh tokens
  - Claves públicas/privadas para validación

### 3. **Teams Service (PHP Laravel)**
- **Puerto**: 8081
- **Tecnología**: PHP Laravel
- **Base de Datos**: PostgreSQL (Puerto 5433)
- **Descripción**: Gestión de equipos de baloncesto
- **Características**:
  - CRUD de equipos
  - Validación con Auth Service
  - API RESTful

### 4. **Players Service (PHP Laravel)**
- **Puerto**: 8082
- **Tecnología**: PHP Laravel
- **Base de Datos**: MySQL (Puerto 3307)
- **Descripción**: Gestión de jugadores
- **Características**:
  - CRUD de jugadores
  - Asignación a equipos
  - Estadísticas de jugadores

### 5. **Matches Service (PHP Laravel)**
- **Puerto**: 8084
- **Tecnología**: PHP Laravel
- **Base de Datos**: SQL Server (Puerto 14332)
- **Descripción**: Gestión de partidos y marcadores
- **Características**:
  - Creación y gestión de partidos
  - Marcador en tiempo real
  - Estadísticas de partidos

### 6. **Tournaments Service (PHP Laravel)**
- **Puerto**: 8085
- **Tecnología**: PHP Laravel
- **Base de Datos**: SQL Server (Puerto 14334)
- **Descripción**: Gestión de torneos
- **Características**:
  - Creación de torneos
  - Gestión de brackets
  - Programación de partidos

### 7. **Reports Service (Python Flask)**
- **Puerto**: 5004
- **Tecnología**: Python Flask
- **Base de Datos**: SQL Server (Puerto 14331)
- **Descripción**: Generación de reportes en PDF
- **Características**:
  - Reportes de estadísticas
  - Generación de PDFs
  - Análisis de datos

## Bases de Datos

| Servicio | Motor | Puerto | Base de Datos | Usuario | Contraseña |
|----------|-------|--------|---------------|---------|------------|
| Auth | SQL Server | 14333 | MarcadorAuthDb | sa | YourStrong!Passw0rd |
| Teams | PostgreSQL | 5433 | teams | teams | teams123 |
| Players | MySQL | 3307 | players | players | players123 |
| Matches | SQL Server | 14332 | desaweb | sa | Str0ngP@ssw0rd2025! |
| Tournaments | SQL Server | 14334 | desaweb | sa | Str0ngP@ssw0rd2025! |
| Reports | SQL Server | 14331 | desaweb | sa | D3saweb.2025$ |

## Configuración con Docker

### Perfiles de Docker Compose
- `auth`: Servicio de autenticación
- `teams`: Servicio de equipos
- `players`: Servicio de jugadores
- `matches`: Servicio de partidos
- `tournaments`: Servicio de torneos
- `reports`: Servicio de reportes
- `front`: Frontend Angular

### Comandos de Despliegue

```bash
# Levantar todos los servicios
docker-compose --profile auth --profile teams --profile players --profile matches --profile tournaments --profile reports --profile front up -d

# Levantar servicios específicos
docker-compose --profile auth --profile front up -d

# Ver logs
docker-compose logs -f [servicio]

# Detener servicios
docker-compose down
```

## Red y Comunicación
- **Red**: `basketball_net` (bridge)
- **Comunicación**: HTTP/REST entre microservicios
- **Autenticación**: JWT con validación de clave pública compartida
- **CORS**: Configurado para permitir comunicación desde el frontend

## Seguridad
- **JWT**: Tokens firmados con RSA
- **OAuth**: Integración con Google y GitHub
- **Claves**: Almacenadas en `./secrets/`
- **Validación**: Cada microservicio valida tokens con clave pública

## Desarrollo Local (Frontend)

### Servidor de desarrollo
```bash
ng serve
```
Navegar a `http://localhost:4200/`

### Construcción
```bash
ng build
```

### Pruebas
```bash
ng test
ng e2e
```

### Scaffolding
```bash
ng generate component component-name
ng generate --help
```

## Estructura del Proyecto
```
FaseIVMarcadorBaloncesto/
├── auth-service.Api/          # Servicio de autenticación (C#)
├── MicroservicioTeams/        # Servicio de equipos (PHP)
├── MicroservicioPlayers/      # Servicio de jugadores (PHP)
├── micro_partidos/            # Servicio de partidos (PHP)
├── api_torneos/              # Servicio de torneos (PHP)
├── MarcadorReportesPDF-Fase3/ # Servicio de reportes (Python)
├── marcador-front/           # Frontend (Angular)
├── secrets/                  # Claves RSA para JWT
├── backups/                  # Respaldos de BD
└── docker-compose.yml        # Orquestación de servicios
```

## Recursos Adicionales
- [Angular CLI Reference](https://angular.dev/tools/cli)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [JWT.io](https://jwt.io/) para debugging de tokens

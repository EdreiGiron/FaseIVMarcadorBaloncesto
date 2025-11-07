# Manual Técnico - Sistema Marcador de Baloncesto

## Integrantes
- Jenny Sofia Morales López 7690 08 6790          |         Aporte 20%
- Cristian Alejandro Melgar Ordoñez 7690 21 8342  |         Aporte 20%
- Edrei Andrés Girón Leonardo 7690-21-218         |         Aporte 20%
- Edward Alexander Aguilar Flores 7690-21-7651    |         Aporte 20%
- Diego Fernando Velásquez Pichilla 7690-16-3882  |         Aporte 20%


Dominio registrado basketmarcador.online

Llave ssh -i ~/.ssh/id_ed25519 melgust@91.99.197.226

IP Pública 91.99.197.226

## Tabla de Contenidos
1. [Arquitectura del Sistema](#arquitectura-del-sistema)
2. [Tecnologías Utilizadas](#tecnologías-utilizadas)
3. [Instalación y Configuración](#instalación-y-configuración)
4. [Microservicios](#microservicios)
5. [Base de Datos](#base-de-datos)
6. [APIs y Endpoints](#apis-y-endpoints)
7. [Seguridad](#seguridad)
8. [Despliegue](#despliegue)
9. [Monitoreo y Logs](#monitoreo-y-logs)
10. [Mantenimiento](#mantenimiento)

## Arquitectura del Sistema

### Arquitectura de Microservicios

El sistema está construido con una arquitectura de microservicios que incluye:

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Frontend      │    │  Auth Service   │    │ Players Service │
│   (Angular)     │    │   (.NET Core)   │    │     (PHP)       │
│   Port: 4200    │    │   Port: 8080    │    │   Port: 8082    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         └───────────────────────┼───────────────────────┘
                                 │
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│  Teams Service  │    │ Matches Service │    │Tournaments Svc  │
│   (Laravel)     │    │   (Laravel)     │    │   (Laravel)     │
│   Port: 5003    │    │   Port: 8084    │    │   Port: 8085    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         └───────────────────────┼───────────────────────┘
                                 │
                    ┌─────────────────┐
                    │ Reports Service │
                    │    (Python)     │
                    │   Port: 5004    │
                    └─────────────────┘
```

### Componentes Principales

- **Frontend**: Aplicación Angular con Material Design
- **API Gateway**: Nginx como proxy reverso
- **Microservicios**: Servicios independientes por dominio
- **Bases de Datos**: MySQL y SQL Server
- **Autenticación**: JWT con RSA + OAuth2

## Tecnologías Utilizadas

### Frontend
- **Angular 20**: Framework principal
- **Angular Material**: Componentes UI
- **TypeScript**: Lenguaje de programación
- **RxJS**: Programación reactiva
- **Sass**: Preprocesador CSS

### Backend
- **.NET Core 8**: Auth Service
- **PHP 8.2 + Laravel 11**: Players, Teams, Matches, Tournaments
- **Python 3.11 + Flask**: Reports Service

### Bases de Datos
- **MySQL 8.0**: Servicio de jugadores
- **SQL Server 2022**: Servicios principales y autenticación

### Infraestructura
- **Docker**: Containerización
- **Docker Compose**: Orquestación local
- **Nginx**: Servidor web y proxy

## Instalación y Configuración

### Prerrequisitos

```bash
# Software requerido
- Docker Desktop 4.0+
- Docker Compose 2.0+
- Git
- Node.js 18+ (para desarrollo)
- .NET 8 SDK (para desarrollo)
- PHP 8.2+ (para desarrollo)
- Python 3.11+ (para desarrollo)
```

### Configuración del Entorno

1. **Clonar el repositorio**:
```bash
git clone <repository-url>
cd FaseIVMarcadorBaloncesto
```

2. **Configurar variables de entorno**:
```bash
# Copiar archivos de configuración
cp auth-service.Api/scr/appsettings.example.json auth-service.Api/scr/appsettings.json
cp MicroservicioPlayers/.env.example MicroservicioPlayers/.env
# Repetir para otros servicios
```

3. **Generar claves RSA para JWT**:
```bash
mkdir -p secrets
openssl genrsa -out secrets/auth_private.pem 2048
openssl rsa -in secrets/auth_private.pem -pubout -out secrets/auth_public.pem
```

### Despliegue con Docker Compose

```bash
# Levantar todos los servicios
docker-compose --profile auth --profile players --profile teams --profile matches --profile tournaments --profile reports --profile front up -d

# Levantar servicios específicos
docker-compose --profile auth --profile front up -d

# Ver logs
docker-compose logs -f [service-name]

# Detener servicios
docker-compose down
```

## Microservicios

### Auth Service (.NET Core)

**Responsabilidades**:
- Autenticación y autorización
- Gestión de usuarios y roles
- Tokens JWT con RSA
- OAuth2 (Google, GitHub)

**Estructura**:
```
auth-service.Api/
├── Controllers/         # Controladores API
├── Data/               # DbContext y configuración
├── Models/             # Entidades del dominio
├── Repositories/       # Acceso a datos
├── Services/           # Lógica de negocio
├── Security/           # JWT y RSA
└── Migrations/         # Migraciones EF Core
```

**Endpoints principales**:
- `POST /api/auth/register` - Registro de usuarios
- `POST /api/auth/login` - Inicio de sesión
- `POST /api/auth/refresh` - Renovar token
- `GET /api/auth/oauth/{provider}` - OAuth externo

### Players Service (PHP/Laravel)

**Responsabilidades**:
- CRUD de jugadores
- Gestión de estadísticas
- Validación de datos

**Base de datos**: MySQL
**Puerto**: 8082

### Teams Service (Laravel)

**Responsabilidades**:
- Gestión de equipos
- Plantillas de jugadores
- Relaciones equipo-jugador

**Base de datos**: SQL Server
**Puerto**: 5003

### Matches Service (Laravel)

**Responsabilidades**:
- Programación de partidos
- Registro de resultados
- Estadísticas de partidos

**Base de datos**: SQL Server
**Puerto**: 8084

### Tournaments Service (Laravel)

**Responsabilidades**:
- Creación de torneos
- Gestión de fixtures
- Clasificaciones

**Base de datos**: SQL Server
**Puerto**: 8085

### Reports Service (Python/Flask)

**Responsabilidades**:
- Generación de reportes PDF
- Análisis estadístico
- Exportación de datos

**Puerto**: 5004

## Base de Datos

### Esquema de Autenticación (SQL Server)

```sql
-- Tabla de usuarios
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Email NVARCHAR(255) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(MAX),
    Provider NVARCHAR(50),
    ProviderId NVARCHAR(255),
    RoleId UNIQUEIDENTIFIER,
    CreatedAt DATETIME2,
    UpdatedAt DATETIME2
);

-- Tabla de roles
CREATE TABLE Roles (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(50) UNIQUE NOT NULL
);

-- Tabla de refresh tokens
CREATE TABLE RefreshTokens (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Token NVARCHAR(MAX) NOT NULL,
    UserId UNIQUEIDENTIFIER,
    ExpiresAt DATETIME2,
    CreatedAt DATETIME2
);
```

### Esquema de Jugadores (MySQL)

```sql
-- Tabla de jugadores
CREATE TABLE players (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    birth_date DATE,
    position ENUM('Base', 'Escolta', 'Alero', 'Ala-Pívot', 'Pívot'),
    jersey_number INT,
    team_id INT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);
```

### Configuración de Conexiones

**SQL Server**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sqlserver,1433;Database=desaweb;User Id=sa;Password=D3saweb.2025$;TrustServerCertificate=true;"
  }
}
```

**MySQL**:
```env
DB_HOST=players-db
DB_DATABASE=players
DB_USERNAME=players
DB_PASSWORD=players123
```

## APIs y Endpoints

### Autenticación

```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123",
  "confirmPassword": "password123"
}
```

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}
```

### Jugadores

```http
GET /api/players
Authorization: Bearer {jwt-token}

POST /api/players
Content-Type: application/json
Authorization: Bearer {jwt-token}

{
  "name": "Juan Pérez",
  "birth_date": "1995-05-15",
  "position": "Base",
  "jersey_number": 10
}
```

### Equipos

```http
GET /api/teams
Authorization: Bearer {jwt-token}

POST /api/teams
Content-Type: application/json
Authorization: Bearer {jwt-token}

{
  "name": "Lakers",
  "city": "Los Angeles",
  "coach": "Phil Jackson",
  "founded_year": 1947
}
```

## Seguridad

### Autenticación JWT

El sistema utiliza JWT con claves RSA para garantizar la seguridad:

```csharp
// Configuración JWT
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://auth-service",
            ValidateAudience = true,
            ValidAudience = "marcador-clients",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = rsaKeyService.PublicKey
        };
    });
```

### OAuth2 Providers

**Google OAuth**:
```json
{
  "OAuth": {
    "Google": {
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret"
    }
  }
}
```

**GitHub OAuth**:
```json
{
  "OAuth": {
    "GitHub": {
      "ClientId": "your-github-client-id",
      "ClientSecret": "your-github-client-secret"
    }
  }
}
```

### CORS Configuration

```csharp
services.AddCors(options =>
{
    options.AddPolicy("cors", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

## Despliegue

### Profiles de Docker Compose

El sistema utiliza profiles para despliegue modular:

```yaml
# Perfiles disponibles
profiles:
  - auth      # Servicio de autenticación
  - players   # Servicio de jugadores
  - teams     # Servicio de equipos
  - matches   # Servicio de partidos
  - tournaments # Servicio de torneos
  - reports   # Servicio de reportes
  - front     # Frontend Angular
```

### Comandos de Despliegue

```bash
# Desarrollo completo
docker-compose --profile auth --profile players --profile teams --profile matches --profile tournaments --profile reports --profile front up -d

# Solo backend
docker-compose --profile auth --profile players --profile teams up -d

# Solo frontend
docker-compose --profile front up -d
```

### Health Checks

Todos los servicios incluyen health checks:

```yaml
healthcheck:
  test: ["CMD-SHELL", "curl -f http://localhost:8080/health || exit 1"]
  interval: 10s
  timeout: 5s
  retries: 10
```

## Monitoreo y Logs

### Logs de Aplicación

```bash
# Ver logs de todos los servicios
docker-compose logs -f

# Ver logs de un servicio específico
docker-compose logs -f auth-service

# Ver logs con timestamp
docker-compose logs -f -t auth-service
```

### Métricas de Salud

Cada servicio expone endpoints de salud:

- Auth Service: `http://localhost:8080/health`
- Players Service: `http://localhost:8082/health`
- Teams Service: `http://localhost:5003/health`

### Monitoreo de Base de Datos

```sql
-- SQL Server - Verificar conexiones
SELECT 
    session_id,
    login_name,
    host_name,
    program_name,
    login_time
FROM sys.dm_exec_sessions
WHERE is_user_process = 1;

-- MySQL - Verificar procesos
SHOW PROCESSLIST;
```

## Mantenimiento

### Backup de Base de Datos

**SQL Server**:
```bash
# Backup automático configurado en docker-compose
docker exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'D3saweb.2025$' -Q "BACKUP DATABASE desaweb TO DISK = '/var/opt/mssql/backups/desaweb.bak'"
```

**MySQL**:
```bash
# Backup de la base de datos de jugadores
docker exec players-db mysqldump -u players -pplayers123 players > backup_players.sql
```

### Actualización de Servicios

```bash
# Reconstruir servicios
docker-compose build --no-cache

# Actualizar servicios específicos
docker-compose up -d --force-recreate auth-service

# Limpiar imágenes no utilizadas
docker system prune -a
```

### Migración de Base de Datos

**.NET Core (Auth Service)**:
```bash
# Ejecutar migraciones
docker exec auth-service dotnet ef database update

# Crear nueva migración
docker exec auth-service dotnet ef migrations add MigrationName
```

**Laravel Services**:
```bash
# Ejecutar migraciones
docker exec teams-service php artisan migrate

# Rollback migraciones
docker exec teams-service php artisan migrate:rollback
```

### Troubleshooting

**Problemas comunes**:

1. **Puerto ocupado**:
```bash
# Verificar puertos en uso
netstat -tulpn | grep :8080

# Detener servicios conflictivos
docker-compose down
```

2. **Problemas de conexión a BD**:
```bash
# Verificar estado de contenedores
docker-compose ps

# Reiniciar servicios de BD
docker-compose restart sqlserver players-db
```

3. **Problemas de autenticación**:
```bash
# Verificar claves RSA
ls -la secrets/
cat secrets/auth_public.pem
```

---

**Versión del Manual**: 1.0  
**Última actualización**: Enero 2025  
**Sistema**: Marcador de Baloncesto v4.0  
**Arquitectura**: Microservicios con Docker

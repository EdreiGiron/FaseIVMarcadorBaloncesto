# Manual de Usuario - Sistema Marcador de Baloncesto

## Tabla de Contenidos
1. [Introducción](#introducción)
2. [Acceso al Sistema](#acceso-al-sistema)
3. [Gestión de Jugadores](#gestión-de-jugadores)
4. [Gestión de Equipos](#gestión-de-equipos)
5. [Gestión de Torneos](#gestión-de-torneos)
6. [Gestión de Partidos](#gestión-de-partidos)
7. [Reportes](#reportes)
8. [Solución de Problemas](#solución-de-problemas)

## Introducción

El Sistema Marcador de Baloncesto es una aplicación web completa que permite gestionar torneos, equipos, jugadores y partidos de baloncesto. El sistema está diseñado con una arquitectura de microservicios que garantiza escalabilidad y mantenibilidad.

### Características Principales
- Gestión completa de jugadores y equipos
- Organización de torneos y partidos
- Sistema de autenticación con OAuth (Google, GitHub)
- Generación de reportes en PDF
- Interfaz web intuitiva y responsive

## Acceso al Sistema

### Requisitos del Sistema
- Navegador web moderno (Chrome, Firefox, Safari, Edge)
- Conexión a internet
- JavaScript habilitado

### Inicio de Sesión

1. **Acceso a la aplicación**: Navegue a `http://localhost:4200`

2. **Opciones de autenticación**:
   - **Registro local**: Complete el formulario con email y contraseña
   - **Google OAuth**: Haga clic en "Iniciar sesión con Google"
   - **GitHub OAuth**: Haga clic en "Iniciar sesión con GitHub"

3. **Roles de usuario**:
   - **ADMIN**: Acceso completo a todas las funcionalidades
   - **USER**: Acceso limitado según permisos asignados

## Gestión de Jugadores

### Agregar Nuevo Jugador

1. Navegue a la sección "Jugadores"
2. Haga clic en "Agregar Jugador"
3. Complete los campos requeridos:
   - Nombre completo
   - Fecha de nacimiento
   - Posición (Base, Escolta, Alero, Ala-Pívot, Pívot)
   - Número de camiseta
   - Equipo (opcional)

### Editar Jugador

1. En la lista de jugadores, haga clic en el ícono de edición
2. Modifique los campos necesarios
3. Guarde los cambios

### Eliminar Jugador

1. Seleccione el jugador a eliminar
2. Haga clic en el ícono de eliminación
3. Confirme la acción

## Gestión de Equipos

### Crear Nuevo Equipo

1. Acceda a la sección "Equipos"
2. Haga clic en "Crear Equipo"
3. Ingrese la información:
   - Nombre del equipo
   - Ciudad
   - Entrenador
   - Año de fundación

### Gestionar Plantilla

1. Seleccione un equipo
2. Haga clic en "Gestionar Plantilla"
3. Agregue o retire jugadores del equipo
4. Asigne números de camiseta únicos

## Gestión de Torneos

### Crear Torneo

1. Vaya a la sección "Torneos"
2. Haga clic en "Nuevo Torneo"
3. Configure:
   - Nombre del torneo
   - Fechas de inicio y fin
   - Tipo de torneo (Liga, Eliminación directa)
   - Equipos participantes

### Gestionar Fixture

1. Seleccione el torneo
2. Acceda a "Fixture"
3. Programe los partidos
4. Asigne fechas, horarios y sedes

## Gestión de Partidos

### Programar Partido

1. En la sección "Partidos", haga clic en "Nuevo Partido"
2. Seleccione:
   - Equipos participantes
   - Fecha y hora
   - Sede del partido
   - Torneo (si aplica)

### Registrar Resultado

1. Seleccione el partido finalizado
2. Haga clic en "Registrar Resultado"
3. Ingrese:
   - Puntuación final de cada equipo
   - Estadísticas individuales (opcional)
   - Observaciones del partido

### Seguimiento en Vivo

1. Durante el partido, acceda a "Marcador en Vivo"
2. Actualice la puntuación en tiempo real
3. Registre eventos importantes (faltas, cambios, etc.)

## Reportes

### Generar Reportes

El sistema permite generar varios tipos de reportes en formato PDF:

#### Reporte de Torneo
1. Seleccione "Reportes" > "Torneo"
2. Elija el torneo deseado
3. Haga clic en "Generar PDF"

#### Reporte de Equipo
1. Vaya a "Reportes" > "Equipo"
2. Seleccione el equipo
3. Defina el período de tiempo
4. Genere el reporte

#### Estadísticas de Jugador
1. Acceda a "Reportes" > "Jugador"
2. Seleccione el jugador
3. Elija las estadísticas a incluir
4. Descargue el PDF

### Tipos de Información en Reportes
- Estadísticas de partidos
- Clasificaciones de torneos
- Rendimiento individual
- Historial de enfrentamientos
- Análisis de temporada

## Solución de Problemas

### Problemas Comunes

#### No puedo iniciar sesión
- Verifique sus credenciales
- Asegúrese de que su cuenta esté activa
- Intente con un proveedor OAuth diferente

#### Los datos no se cargan
- Verifique su conexión a internet
- Actualice la página (F5)
- Limpie la caché del navegador

#### Error al generar reportes
- Verifique que existan datos para el período seleccionado
- Intente con un rango de fechas diferente
- Contacte al administrador si persiste

#### Problemas de rendimiento
- Cierre pestañas innecesarias del navegador
- Verifique que no haya otros procesos consumiendo recursos
- Actualice su navegador a la última versión

### Contacto de Soporte

Para problemas técnicos o consultas adicionales:
- Email: soporte@marcadorbaloncesto.com
- Teléfono: +1 (555) 123-4567
- Horario de atención: Lunes a Viernes, 9:00 AM - 6:00 PM

### Actualizaciones del Sistema

El sistema se actualiza automáticamente. Las nuevas funcionalidades y correcciones se implementan sin interrumpir el servicio. Los usuarios serán notificados de cambios importantes a través del sistema de notificaciones interno.

---

**Versión del Manual**: 1.0  
**Última actualización**: Enero 2025  
**Sistema**: Marcador de Baloncesto v4.0
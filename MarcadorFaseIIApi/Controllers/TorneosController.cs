using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarcadorFaseIIApi.Data;
using MarcadorFaseIIApi.Models;
using MarcadorFaseIIApi.Models.DTOs.Playoffs;

namespace MarcadorFaseIIApi.Controllers
{
    /// <summary>
    /// Controlador para la administración del ciclo de vida de torneos:
    /// creación, consulta, generación de rondas y siembra de datos de demostración.
    /// </summary>
    /// <remarks>
    /// Ruta base: <c>/api/torneos</c>. 
    /// Utiliza <see cref="MarcadorDbContext"/> para persistencia y modelos de dominio como
    /// <see cref="Torneo"/>, <see cref="SeriePlayoff"/> y <see cref="Partido"/>.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TorneosController : ControllerBase
    {
        private readonly MarcadorDbContext _db;

        /// <summary>
        /// Crea una nueva instancia del controlador de torneos.
        /// </summary>
        /// <param name="db">Contexto de base de datos.</param>
        public TorneosController(MarcadorDbContext db) { _db = db; }

        // POST api/torneos

        /// <summary>
        /// Crea un torneo, genera la ronda inicial en base a los seeds recibidos
        /// y activa el torneo.
        /// </summary>
        /// <param name="dto">
        /// Datos para crear el torneo: nombre, temporada, formato <c>BestOf</c> y lista
        /// de IDs de equipos ordenados por seed (del más alto al más bajo).
        /// </param>
        /// <returns>
        /// <response code="201">Torneo creado y activado; devuelve <see cref="TorneoDto"/>.</response>
        /// <response code="400">Solicitud inválida (menos de 2 equipos o equipo inexistente).</response>
        /// </returns>
        /// <remarks>
        /// - Valida que existan al menos 2 equipos y que todos los IDs enviados correspondan a equipos reales.<br/>
        /// - Establece <c>BestOf</c> por defecto en 5 si viene ≤ 0.<br/>
        /// - Cambia el estado del torneo a <c>Activo</c> tras generar la ronda inicial.
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(TorneoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TorneoDto>> Crear([FromBody] CrearTorneoDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest("El nombre es requerido.");
                
            if (dto.EquipoIdsSeed is null || dto.EquipoIdsSeed.Count < 2)
                return BadRequest("Se requieren al menos 2 equipos.");

            var existenTodos = await _db.Equipos
                .Where(e => dto.EquipoIdsSeed.Contains(e.Id))
                .CountAsync() == dto.EquipoIdsSeed.Count;

            if (!existenTodos) return BadRequest("Algún equipo no existe.");

            var t = new Torneo
            {
                Nombre = dto.Nombre,
                Temporada = dto.Temporada,
                BestOf = dto.BestOf <= 0 ? 5 : dto.BestOf,
                Estado = TorneoEstado.Planificado
            };
            _db.Torneos.Add(t);
            await _db.SaveChangesAsync();

            await GenerarRondaInicialInterno(t.Id, dto.EquipoIdsSeed);
            t.Estado = TorneoEstado.Activo;
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = t.Id },
                new TorneoDto(t.Id, t.Nombre, t.Temporada, t.BestOf, t.Estado.ToString()));
        }

        /// <summary>
        /// Obtiene los datos de un torneo por su identificador.
        /// </summary>
        /// <param name="id">Identificador del torneo.</param>
        /// <returns>
        /// <response code="200">Devuelve <see cref="TorneoDto"/>.</response>
        /// <response code="404">No existe el torneo solicitado.</response>
        /// </returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(TorneoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TorneoDto>> GetById(int id)
        {
            var t = await _db.Torneos.FindAsync(id);
            if (t is null) return NotFound();
            return Ok(new TorneoDto(t.Id, t.Nombre, t.Temporada, t.BestOf, t.Estado.ToString()));
        }

        // POST api/torneos/{id}/generar-siguiente-ronda

        /// <summary>
        /// Genera la siguiente ronda del torneo a partir de los ganadores de la ronda actual.
        /// </summary>
        /// <param name="id">Identificador del torneo.</param>
        /// <returns>
        /// <response code="200">Siguiente ronda generada o torneo finalizado.</response>
        /// <response code="400">Aún hay series abiertas en la ronda actual.</response>
        /// <response code="404">No existe el torneo solicitado.</response>
        /// </returns>
        /// <remarks>
        /// Proceso:
        /// <list type="number">
        /// <item>Valida que no existan series abiertas en la ronda actual.</item>
        /// <item>Si la última ronda fue <c>Final</c>, marca el torneo como <c>Finalizado</c>.</item>
        /// <item>Determina ganadores (por <c>GanadorEquipoId</c> o por mayor número de victorias).</item>
        /// <item>Empareja 1-2, 3-4, ... y crea nuevas <see cref="SeriePlayoff"/> con <c>BestOf=0</c> (heredable por lógica superior).</item>
        /// <item>Programa el Juego 1 de cada serie a partir de hoy+2 días a las 19:00, incrementando un día por serie.</item>
        /// </list>
        /// </remarks>
        [HttpPost("{id:int}/generar-siguiente-ronda")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GenerarSiguienteRonda(int id)
        {
            var t = await _db.Torneos.Include(x => x.Series).ThenInclude(s => s.Partidos)
                                     .FirstOrDefaultAsync(x => x.Id == id);
            if (t is null) return NotFound();

            // Si hay series abiertas en la ronda actual, no se puede avanzar
            if (t.Series.Any(s => !s.Cerrada))
                return BadRequest("Aún hay series abiertas.");

            // Si la última ronda fue la Final -> torneo finalizado
            var rondaMax = t.Series.Max(s => s.Ronda);
            if (rondaMax == RondaTipo.Final)
            {
                t.Estado = TorneoEstado.Finalizado;
                await _db.SaveChangesAsync();
                return Ok(new { message = "Torneo finalizado." });
            }

            // Parear ganadores 1-2, 3-4...
            var ganadores = t.Series.Where(s => s.Ronda == rondaMax)
                                    .OrderBy(s => s.SeedA)
                                    .Select(s => s.GanadorEquipoId ?? (s.WinsA > s.WinsB ? s.EquipoAId : s.EquipoBId))
                                    .ToList();

            var nuevaRonda = (RondaTipo)((int)rondaMax / 2);
            var seriesNuevas = new List<SeriePlayoff>();
            for (int i = 0; i < ganadores.Count; i += 2)
            {
                seriesNuevas.Add(new SeriePlayoff
                {
                    TorneoId = t.Id,
                    Ronda = nuevaRonda,
                    SeedA = i + 1,
                    SeedB = i + 2,
                    EquipoAId = ganadores[i],
                    EquipoBId = ganadores[i + 1],
                    BestOf = 0
                });
            }
            _db.Series.AddRange(seriesNuevas);
            await _db.SaveChangesAsync();

            // Programa Juego 1 de cada serie: hoy+2 a las 19:00
            var baseDt = DateTime.Now.Date.AddDays(2).AddHours(19);
            int offset = 0;
            foreach (var s in seriesNuevas)
            {
                _db.Partidos.Add(new Partido
                {
                    TorneoId = t.Id,
                    SeriePlayoffId = s.Id,
                    GameNumber = 1,
                    FechaHora = baseDt.AddDays(offset++),
                    EquipoLocalId = s.EquipoAId,
                    EquipoVisitanteId = s.EquipoBId
                });
            }
            await _db.SaveChangesAsync();
            return Ok(new { message = "Siguiente ronda generada." });
        }

        /// <summary>
        /// Genera la ronda inicial y agenda el Juego 1 de cada serie.
        /// </summary>
        /// <param name="torneoId">Identificador del torneo previamente creado.</param>
        /// <param name="seeds">
        /// Lista de IDs de equipos en orden de seed; la cantidad debe ser compatible con bracket: 2, 4, 8 o 16.
        /// </param>
        /// <remarks>
        /// <para>
        /// Determina automáticamente el tipo de ronda (<c>Final</c>, <c>Semifinal</c>, <c>Cuartos</c>, <c>Octavos</c>)
        /// según el número de equipos. Empareja por semilla (1 vs N, 2 vs N-1, etc.).
        /// </para>
        /// <para>
        /// Programa el Juego 1 de cada serie para mañana a las 19:00, aumentando un día por serie.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Cuando el torneo no existe o la cantidad de equipos no es válida para un bracket.
        /// </exception>
        private async Task GenerarRondaInicialInterno(int torneoId, List<int> seeds)
        {
            var t = await _db.Torneos.FindAsync(torneoId)
                ?? throw new InvalidOperationException("Torneo no existe.");

            int n = seeds.Count;
            RondaTipo ronda = n switch
            {
                2 => RondaTipo.Final,
                4 => RondaTipo.Semifinal,
                8 => RondaTipo.Cuartos,
                16 => RondaTipo.Octavos,
                _ => throw new InvalidOperationException("Cantidad de equipos no válida para bracket.")
            };

            var series = new List<SeriePlayoff>();
            for (int i = 0; i < n / 2; i++)
            {
                series.Add(new SeriePlayoff
                {
                    TorneoId = torneoId,
                    Ronda = ronda,
                    SeedA = i + 1,
                    SeedB = n - i,
                    EquipoAId = seeds[i],
                    EquipoBId = seeds[n - 1 - i],
                    BestOf = 0
                });
            }
            _db.Series.AddRange(series);
            await _db.SaveChangesAsync();

            // Programa Juego 1 (hoy + 1 día a las 19:00)
            var baseDt = DateTime.Now.Date.AddDays(1).AddHours(19);
            int d = 0;
            foreach (var s in series)
            {
                _db.Partidos.Add(new Partido
                {
                    TorneoId = torneoId,
                    SeriePlayoffId = s.Id,
                    GameNumber = 1,
                    FechaHora = baseDt.AddDays(d++),
                    EquipoLocalId = s.EquipoAId,
                    EquipoVisitanteId = s.EquipoBId
                });
            }
            await _db.SaveChangesAsync();
        }

        // SEED DEMO: crea un torneo en juego con partidos pasados/hoy/futuros
        // POST api/torneos/seed-demo

        /// <summary>
        /// Siembra datos de demostración creando un torneo con 4 equipos,
        /// ajustando partidos a pasado/presente/futuro.
        /// </summary>
        /// <returns>
        /// <response code="200">Seed de demo creado; devuelve el <c>torneoId</c>.</response>
        /// <response code="400">No hay suficientes equipos (mínimo 4) en la base.</response>
        /// </returns>
        /// <remarks>
        /// - Toma los primeros 4 equipos ordenados por Id.<br/>
        /// - Crea el torneo y ajusta estados: un partido finalizado (ayer-2), uno en juego (ahora) y otro programado (mañana).
        /// </remarks>
        [HttpPost("seed-demo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SeedDemo()
        {
            var equipos = await _db.Equipos.OrderBy(e => e.Id).Take(4).Select(e => e.Id).ToListAsync();
            if (equipos.Count < 4) return BadRequest("Se requieren al menos 4 equipos en la base.");

            var dto = new CrearTorneoDto("Playoffs Apertura (Demo)", DateTime.UtcNow.Year, 5, equipos);
            var created = await Crear(dto);
            var torneoId = ((TorneoDto)((CreatedAtActionResult)created.Result!).Value!).Id;

            var partidos = await _db.Partidos.Where(p => p.TorneoId == torneoId).OrderBy(p => p.Id).ToListAsync();
            if (partidos.Count >= 3)
            {
                // Partido pasado (finalizado)
                partidos[0].FechaHora = DateTime.Now.AddDays(-2).Date.AddHours(19);
                partidos[0].MarcadorLocal = 82; partidos[0].MarcadorVisitante = 76;
                partidos[0].Estado = PartidoEstado.Finalizado;
                var s0 = await _db.Series.FirstAsync(x => x.Id == partidos[0].SeriePlayoffId);
                if (partidos[0].EquipoLocalId == s0.EquipoAId) s0.WinsA++; else s0.WinsB++;

                // Hoy en juego
                partidos[1].FechaHora = DateTime.Now;
                partidos[1].Estado = PartidoEstado.EnJuego;

                // Mañana
                partidos[2].FechaHora = DateTime.Now.AddDays(1).Date.AddHours(19);
                partidos[2].Estado = PartidoEstado.Programado;

                await _db.SaveChangesAsync();
            }
            return Ok(new { message = "Seed demo creado.", torneoId });
        }
    }
}

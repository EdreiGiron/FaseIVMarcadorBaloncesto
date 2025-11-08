using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MarcadorFaseIIApi.Data;
using MarcadorFaseIIApi.Models;

using MarcadorFaseIIApi.Models.DTOs;
using MarcadorFaseIIApi.Models.DTOs.Playoffs;
using MarcadorFaseIIApi.Models.DTOs.Partidos;

using CommonDtos = MarcadorFaseIIApi.Models.DTOs;
using PlayoffDtos = MarcadorFaseIIApi.Models.DTOs.Playoffs;

namespace MarcadorFaseIIApi.Controllers
{
    /// <summary>
    /// Controlador para la administración y consulta de partidos.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PartidosController : ControllerBase
    {
        private readonly MarcadorDbContext _db;

        public PartidosController(MarcadorDbContext db) => _db = db;

        // =========================================================================
        // PUT: api/partidos/{id}/marcador
        // =========================================================================

        /// <summary>
        /// Cierra un partido, actualiza serie (si aplica) y programa el siguiente juego si corresponde.
        /// </summary>
        [HttpPut("{id:int}/marcador")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Cerrar(int id, [FromBody] CerrarPartidoDto dto)
        {
            var p = await _db.Partidos
                .Include(x => x.Serie)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (p is null) return NotFound();

            p.MarcadorLocal = dto.MarcadorLocal;
            p.MarcadorVisitante = dto.MarcadorVisitante;
            p.Estado = PartidoEstado.Finalizado;

            var s = p.Serie;

            // Si NO hay serie (amistoso) solo guarda
            if (s is null)
            {
                await _db.SaveChangesAsync();
                return NoContent();
            }

            // === Series de playoff ===
            int bestOf = s.BestOf > 0
                ? s.BestOf
                : (await _db.Torneos
                        .Where(t => t.Id == s.TorneoId)
                        .Select(t => (int?)t.BestOf)
                        .FirstOrDefaultAsync()) ?? 0;

            int winsNecesarios = bestOf > 0 ? (bestOf / 2 + 1) : 0;

            // Decide ganador con los valores recién asignados
            int ml = dto.MarcadorLocal;
            int mv = dto.MarcadorVisitante;
            int ganadorId = ml > mv ? p.EquipoLocalId : p.EquipoVisitanteId;

            if (ganadorId == s.EquipoAId) s.WinsA++; else s.WinsB++;

            // ¿Se cerró la serie?
            if (winsNecesarios > 0 && (s.WinsA >= winsNecesarios || s.WinsB >= winsNecesarios))
            {
                s.Cerrada = true;
                s.GanadorEquipoId = s.WinsA > s.WinsB ? s.EquipoAId : s.EquipoBId;
            }
            else
            {
                // Programa el siguiente juego de la serie
                int nextGame = (await _db.Partidos.CountAsync(x => x.SeriePlayoffId == s.Id)) + 1;
                bool localEsA = (nextGame % 2 == 1);

                _db.Partidos.Add(new Partido
                {
                    TorneoId = s.TorneoId,
                    SeriePlayoffId = s.Id,
                    GameNumber = nextGame,
                    FechaHora = (p.FechaHora.Date.AddDays(1)).AddHours(19),
                    EquipoLocalId = localEsA ? s.EquipoAId : s.EquipoBId,
                    EquipoVisitanteId = localEsA ? s.EquipoBId : s.EquipoAId,
                    Estado = PartidoEstado.Programado
                });
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // =========================================================================
        // POST: api/partidos/{id}/roster
        // =========================================================================

        /// <summary>
        /// Asigna (o reemplaza) el roster de un equipo para un partido.
        /// Elimina previamente los registros existentes de ese equipo en el partido.
        /// </summary>
        [HttpPost("{id:int}/roster")] 
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AsignarRoster(int id, [FromBody] AsignarRosterDto dto)
        {
            var p = await _db.Partidos
                .Include(x => x.Roster)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (p is null) return NotFound();

            var actuales = (p.Roster ?? new List<PartidoJugador>())
                .Where(r => r.EquipoId == dto.EquipoId)
                .ToList();

            _db.PartidosJugadores.RemoveRange(actuales);

            var nuevos = dto.Jugadores?.Take(12).Select(j => new PartidoJugador
            {
                PartidoId = id,
                EquipoId = dto.EquipoId,
                JugadorId = j.JugadorId,
                Titular = j.Titular
            }).ToList() ?? new List<PartidoJugador>();

            await _db.PartidosJugadores.AddRangeAsync(nuevos);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // =========================================================================
        // GET: api/partidos/{id}/roster
        // =========================================================================

        /// <summary>
        /// Devuelve el roster del partido (jugadores por equipo) ya asignado.-- Para reportes
        /// </summary>
        [HttpGet("{id:int}/roster")]
         [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<PartidoRosterItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<PartidoRosterItemDto>>> GetRoster(int id, CancellationToken ct)
        {
            // Si el partido no existe puedes validar primero:
            // if (!await _db.Partidos.AnyAsync(x => x.Id == id, ct)) return NotFound();

            var rows = await _db.PartidosJugadores
                .AsNoTracking()
                .Where(pj => pj.PartidoId == id)
                .Join(
                    _db.Jugadores,
                    pj => pj.JugadorId,
                    j => j.Id,
                    (pj, j) => new PartidoRosterItemDto
                    {
                        PartidoId = pj.PartidoId,
                        EquipoId = pj.EquipoId,
                        JugadorId = pj.JugadorId,
                        JugadorNombre = j.Nombre,
                        Posicion = j.Posicion
                    }
                )
                .ToListAsync(ct);

            return Ok(rows);
        }

        // =========================================================================
        // POST: api/partidos 
        // =========================================================================

        /// <summary>
        /// Crea un partido amistoso (sin asociación a serie de playoff).
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<object>> CrearAmistoso([FromBody] CrearAmistosoDto dto)
        {
            if (dto.EquipoLocalId == dto.EquipoVisitanteId)
                return BadRequest("Los equipos no pueden ser iguales.");

            var p = new Partido
            {
                FechaHora = dto.FechaHora,
                EquipoLocalId = dto.EquipoLocalId,
                EquipoVisitanteId = dto.EquipoVisitanteId,
                Estado = PartidoEstado.Programado
            };

            _db.Partidos.Add(p);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = p.Id }, new { id = p.Id });
        }

        // =========================================================================
        // GET: api/partidos
        // =========================================================================

        /// <summary>
        /// Lista todos los partidos.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<PartidoDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PartidoDto>>> GetPartidos()
        {
            var partidos = await _db.Partidos
                .Include(p => p.Serie)
                .Join(_db.Equipos, p => p.EquipoLocalId, e => e.Id, (p, el) => new { p, el })
                .Join(_db.Equipos, x => x.p.EquipoVisitanteId, ev => ev.Id, (x, ev) => new PartidoDto(
                    x.p.Id,
                    x.p.TorneoId,
                    x.p.SeriePlayoffId,
                    x.p.GameNumber,
                    x.p.FechaHora,
                    x.p.Estado.ToString(),
                    x.p.EquipoLocalId,
                    x.p.EquipoVisitanteId,
                    x.p.MarcadorLocal,
                    x.p.MarcadorVisitante,
                    x.p.Serie != null ? x.p.Serie.Ronda.ToString() : null,
                    x.p.Serie != null ? x.p.Serie.SeedA : (int?)null,
                    x.p.Serie != null ? x.p.Serie.SeedB : (int?)null,
                    x.el.Nombre,
                    ev.Nombre
                ))
                .OrderByDescending(p => p.FechaHora)
                .ToListAsync();

            return Ok(partidos);
        }

        // =========================================================================
        // GET: api/partidos/{id}
        // =========================================================================

        /// <summary>
        /// Detalle del partido por id.
        /// </summary>
        [HttpGet("{id:int}")]
         [AllowAnonymous]
        [ProducesResponseType(typeof(PartidoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PartidoDto>> GetById(int id)
        {
            var p = await _db.Partidos
                .Include(x => x.Serie)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (p is null) return NotFound();

            string? ronda = p.Serie != null ? p.Serie.Ronda.ToString() : null;
            int? seedA = p.Serie?.SeedA;
            int? seedB = p.Serie?.SeedB;

            var equipoLocal = await _db.Equipos.FirstOrDefaultAsync(e => e.Id == p.EquipoLocalId);
            var equipoVisitante = await _db.Equipos.FirstOrDefaultAsync(e => e.Id == p.EquipoVisitanteId);

            return new PartidoDto(
                p.Id, p.TorneoId, p.SeriePlayoffId, p.GameNumber,
                p.FechaHora, p.Estado.ToString(),
                p.EquipoLocalId, p.EquipoVisitanteId,
                p.MarcadorLocal, p.MarcadorVisitante,
                ronda, seedA, seedB,
                equipoLocal?.Nombre, equipoVisitante?.Nombre
            );
        }

        // =========================================================================
        // PUT: api/partidos/{id}/estado
        // =========================================================================

        /// <summary>
        /// Cambia el estado de un partido (Programado, EnJuego, Finalizado, ...).
        /// </summary>
        [HttpPut("{id:int}/estado")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoDto dto)
        {
            if (!Enum.TryParse<PartidoEstado>(dto.Estado, out var nuevo))
                return BadRequest("Estado inválido.");

            var p = await _db.Partidos.FirstOrDefaultAsync(x => x.Id == id);
            if (p is null) return NotFound();

            p.Estado = nuevo;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // =========================================================================
        // GET: api/partidos/historial
        // =========================================================================

        /// <summary>
        /// Historial paginado de partidos con filtros (torneo, estado, ronda, equipo y fechas).
        /// </summary>
        // GET: api/partidos/historial
        // GET: api/partidos/historial
        [HttpGet("historial")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CommonDtos.PagedResult<PartidoDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommonDtos.PagedResult<PartidoDto>>> Historial([FromQuery] PartidoQuery q)
        {
          
            var qry = _db.Partidos
                .Include(p => p.Serie) 
                .AsQueryable();

            // Filtros
            if (q.TorneoId.HasValue)
                qry = qry.Where(p => p.TorneoId == q.TorneoId.Value);

            if (!string.IsNullOrWhiteSpace(q.Estado) &&
                Enum.TryParse<PartidoEstado>(q.Estado, true, out var est))
                qry = qry.Where(p => p.Estado == est);

            if (!string.IsNullOrWhiteSpace(q.Ronda) &&
                Enum.TryParse<RondaTipo>(q.Ronda, true, out var ronda))
                qry = qry.Where(p => p.Serie != null && p.Serie.Ronda == ronda);

            if (q.EquipoId.HasValue)
                qry = qry.Where(p => p.EquipoLocalId == q.EquipoId.Value ||
                                     p.EquipoVisitanteId == q.EquipoId.Value);

            if (q.FechaDesde.HasValue)
                qry = qry.Where(p => p.FechaHora >= q.FechaDesde.Value);

            if (q.FechaHasta.HasValue)
                qry = qry.Where(p => p.FechaHora <= q.FechaHasta.Value);

           
            var now = DateTime.Now;
            await qry.Where(p => p.Estado == PartidoEstado.Programado &&
                                 p.FechaHora <= now &&
                                 p.MarcadorLocal == null &&
                                 p.MarcadorVisitante == null)
                     .ExecuteUpdateAsync(s => s.SetProperty(p => p.Estado, PartidoEstado.EnJuego));

            // Paginación defensiva
            int page = q.Page <= 0 ? 1 : q.Page;
            int size = q.PageSize <= 0 ? 10 : q.PageSize;

            // Total + items
            var total = await qry.CountAsync();

            var items = await qry.OrderByDescending(p => p.FechaHora)
                                 .Skip((page - 1) * size)
                                 .Take(size)
                                 .Join(_db.Equipos, p => p.EquipoLocalId, e => e.Id, (p, el) => new { p, el })
                                 .Join(_db.Equipos, x => x.p.EquipoVisitanteId, ev => ev.Id, (x, ev) => new PartidoDto(
                                     x.p.Id,
                                     x.p.TorneoId,
                                     x.p.SeriePlayoffId,
                                     x.p.GameNumber,
                                     x.p.FechaHora,
                                     x.p.Estado.ToString(),
                                     x.p.EquipoLocalId,
                                     x.p.EquipoVisitanteId,
                                     x.p.MarcadorLocal,
                                     x.p.MarcadorVisitante,
                                     x.p.Serie != null ? x.p.Serie.Ronda.ToString() : null,
                                     x.p.Serie != null ? x.p.Serie.SeedA : (int?)null,
                                     x.p.Serie != null ? x.p.Serie.SeedB : (int?)null,
                                     x.el.Nombre,
                                     ev.Nombre
                                 ))
                                 .ToListAsync();

           
            return Ok(new CommonDtos.PagedResult<PartidoDto>
            {
                Items = items,
                Total = total
            });
        }
    }
}

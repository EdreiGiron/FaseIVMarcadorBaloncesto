using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarcadorFaseIIApi.Data;
using MarcadorFaseIIApi.Models;
using MarcadorFaseIIApi.Models.DTOs.Stats;


namespace MarcadorFaseIIApi.Controllers
{
    [ApiController]
    [Route("api")]
    [Produces("application/json")]
    public class EstadisticasController : ControllerBase
    {
        private readonly MarcadorDbContext _db;
        public EstadisticasController(MarcadorDbContext db) => _db = db;

        // ---------------------------------------------------------------------
        // POST api/partidos/{id}/stats
        // Upsert masivo de stats de jugadores para un partido
        // ---------------------------------------------------------------------
        [HttpPost("partidos/{id:int}/stats")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpsertStats(int id, [FromBody] IEnumerable<PlayerStatUpsertDto> stats)
        {
            var partidoExiste = await _db.Partidos.AnyAsync(p => p.Id == id);
            if (!partidoExiste) return NotFound($"Partido {id} no existe.");

            foreach (var s in stats)
            {
                var row = await _db.PartidoJugadorStats
                    .FirstOrDefaultAsync(x => x.PartidoId == id && x.JugadorId == s.JugadorId);

                if (row is null)
                {
                    row = new PartidoJugadorStat { PartidoId = id, JugadorId = s.JugadorId };
                    _db.PartidoJugadorStats.Add(row);
                }

                row.EquipoId = s.EquipoId;
                row.Minutos = s.Minutos;
                row.Puntos = s.Puntos;
                row.Rebotes = s.Rebotes;
                row.Asistencias = s.Asistencias;
                row.Robos = s.Robos;
                row.Bloqueos = s.Bloqueos;
                row.Perdidas = s.Perdidas;
                row.Faltas = s.Faltas;
                row.FGM = s.FGM; row.FGA = s.FGA;
                row.TPM = s.TPM; row.TPA = s.TPA;
                row.FTM = s.FTM; row.FTA = s.FTA;
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ---------------------------------------------------------------------
        // GET api/jugadores/{jugadorId}/
        // Devuelve promedios y porcentajes agregados
        // ---------------------------------------------------------------------
        [HttpGet("jugadores/{jugadorId:int}/stats")]
        [ProducesResponseType(typeof(PlayerSeasonStatsDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<PlayerSeasonStatsDto>> GetPlayerStats(
            int jugadorId,
            [FromQuery] int? torneoId,          // si tu Partido tiene TorneoId
            [FromQuery] int? temporadaId)       // o si manejas TemporadaId; usa el que tengas
        {
            var baseQuery = _db.PartidoJugadorStats
                .Include(s => s.Partido)
                .Include(s => s.Jugador).ThenInclude(j => j.Equipo)
                .Where(s => s.JugadorId == jugadorId);

            if (torneoId.HasValue)
                baseQuery = baseQuery.Where(s => s.Partido.TorneoId == torneoId.Value);

            // Si tienes TemporadaId en Partido, filtra aquí
            // if (temporadaId.HasValue)
            //     baseQuery = baseQuery.Where(s => s.Partido.TemporadaId == temporadaId.Value);

            var list = await baseQuery.ToListAsync();
            var juegos = list.Count;

            var dto = new PlayerSeasonStatsDto
            {
                JugadorId = jugadorId,
                Nombre = list.FirstOrDefault()?.Jugador?.Nombre ?? "",
                Posicion = list.FirstOrDefault()?.Jugador?.Posicion,
                Edad = list.FirstOrDefault()?.Jugador?.edad,
                Estatura = list.FirstOrDefault()?.Jugador?.estatura ?? 0,
                Nacionalidad = list.FirstOrDefault()?.Jugador?.nacionalidad,
                EquipoNombre = list.FirstOrDefault()?.Jugador?.Equipo?.Nombre ?? "",
                Juegos = juegos,

                MinutosTotales = list.Sum(x => x.Minutos),
                PuntosTotales = list.Sum(x => x.Puntos),
                RebotesTotales = list.Sum(x => x.Rebotes),
                AsistenciasTotales = list.Sum(x => x.Asistencias),
                RobosTotales = list.Sum(x => x.Robos),
                BloqueosTotales = list.Sum(x => x.Bloqueos),
                PerdidasTotales = list.Sum(x => x.Perdidas),
                FaltasTotales = list.Sum(x => x.Faltas),
                FGM = list.Sum(x => x.FGM),
                FGA = list.Sum(x => x.FGA),
                TPM = list.Sum(x => x.TPM),
                TPA = list.Sum(x => x.TPA),
                FTM = list.Sum(x => x.FTM),
                FTA = list.Sum(x => x.FTA),

                MIN = juegos > 0 ? list.Average(x => x.Minutos) : 0,
                PPG = juegos > 0 ? list.Average(x => x.Puntos) : 0,
                RPG = juegos > 0 ? list.Average(x => x.Rebotes) : 0,
                APG = juegos > 0 ? list.Average(x => x.Asistencias) : 0,
                SPG = juegos > 0 ? list.Average(x => x.Robos) : 0,
                BPG = juegos > 0 ? list.Average(x => x.Bloqueos) : 0,
                TOV = juegos > 0 ? list.Average(x => x.Perdidas) : 0,
                PF = juegos > 0 ? list.Average(x => x.Faltas) : 0,
                FG = list.Sum(x => x.FGA) > 0 ? (double)list.Sum(x => x.FGM) / list.Sum(x => x.FGA) : 0,
                TP3 = list.Sum(x => x.TPA) > 0 ? (double)list.Sum(x => x.TPM) / list.Sum(x => x.TPA) : 0,
                FT = list.Sum(x => x.FTA) > 0 ? (double)list.Sum(x => x.FTM) / list.Sum(x => x.FTA) : 0,

                // Si guardas estos campos en Jugador:
                Fortalezas = list.FirstOrDefault()?.Jugador?.GetType().GetProperty("Fortalezas") != null
                             ? (string?)list.First().Jugador.GetType().GetProperty("Fortalezas")!.GetValue(list.First().Jugador)
                             : null,
                Debilidades = list.FirstOrDefault()?.Jugador?.GetType().GetProperty("Debilidades") != null
                             ? (string?)list.First().Jugador.GetType().GetProperty("Debilidades")!.GetValue(list.First().Jugador)
                             : null
            };

            return Ok(dto);
        }
    }
}



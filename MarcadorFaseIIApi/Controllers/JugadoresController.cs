using Microsoft.AspNetCore.Mvc;
using MarcadorFaseIIApi.Services;
using MarcadorFaseIIApi.Models;
using MarcadorFaseIIApi.Models.DTOs.Jugador;
using MarcadorFaseIIApi.Models.DTOs.Common;

namespace MarcadorFaseIIApi.Controllers
{
    /// <summary>
    /// Gestión de jugadores: listado, detalle y paginado con filtros.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class JugadoresController : ControllerBase
    {
        private readonly JugadorService _service;

        public JugadoresController(JugadorService service) => _service = service;

        // ---------------------------------------------------------------------
        // GET api/jugadores
        // ---------------------------------------------------------------------
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<JugadorResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<JugadorResponseDto>>> Get(
            [FromQuery] string? search,
            [FromQuery] string? equipoNombre,
            [FromQuery] int? equipoId,
            [FromQuery] string? posicion,
            CancellationToken ct)
        {
            var jugadores = await _service.GetListAsync(search, equipoNombre, equipoId, posicion, ct);
            return Ok(jugadores.Select(ToDto));
        }

        // ---------------------------------------------------------------------
        // GET api/jugadores/{id}
        // ---------------------------------------------------------------------
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(JugadorResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<JugadorResponseDto>> GetById(int id, CancellationToken ct)
        {
            var j = await _service.GetByIdAsync(id, ct);
            if (j is null) return NotFound();
            return Ok(ToDto(j));
        }

        // ---------------------------------------------------------------------
        // GET api/jugadores/
        // ---------------------------------------------------------------------
        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedResult<JugadorResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<JugadorResponseDto>>> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? equipoNombre = null,
            [FromQuery] int? equipoId = null,
            [FromQuery] string? posicion = null,
            CancellationToken ct = default)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            
            (IEnumerable<Jugador> items, int total) = await _service.GetPagedAsync(
                search, equipoNombre, equipoId, posicion,
                sortBy: "nombre", asc: true,
                page: page, pageSize: pageSize, ct: ct);

            var dtos = items.Select(ToDto).ToList();
            return Ok(new PagedResult<JugadorResponseDto>(dtos, total, page, pageSize));
        }

        // ---------------------------------------------------------------------
        // Mapeando entidad -> DTO
        // ---------------------------------------------------------------------
        private static JugadorResponseDto ToDto(Jugador j)
        {
            
            int? numero = null;
            if (!string.IsNullOrWhiteSpace(j.numero) && int.TryParse(j.numero, out var n))
                numero = n;

            return new JugadorResponseDto
            {
                Id = j.Id,
                Nombre = j.Nombre,
                Puntos = j.Puntos,
                Faltas = j.Faltas,
                Posicion = j.Posicion,
                EquipoId = j.EquipoId,
                EquipoNombre = j.Equipo?.Nombre ?? string.Empty,

                Numero = numero,
                Edad = j.edad,
                Estatura = j.estatura,
                Nacionalidad = string.IsNullOrWhiteSpace(j.nacionalidad) ? null : j.nacionalidad
            };
        }
    }
}

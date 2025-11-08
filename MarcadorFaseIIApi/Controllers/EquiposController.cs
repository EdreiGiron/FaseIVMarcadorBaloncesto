using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarcadorFaseIIApi.Services;
using MarcadorFaseIIApi.Models;
using MarcadorFaseIIApi.Models.DTOs.Equipo;
using MarcadorFaseIIApi.Models.DTOs.Common;

namespace MarcadorFaseIIApi.Controllers // <- usa el mismo namespace que ya tienes en el proyecto
{
    /// <summary>
    /// Gestión de equipos: listado, detalle, paginación y CRUD (con autorización en entorno no-DEBUG).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EquiposController : ControllerBase
    {
        private readonly EquipoService _service;

        /// <summary>
        /// Crea el controlador de equipos con su servicio de dominio.
        /// </summary>
        /// <param name="service">Servicio de equipos.</param>
        public EquiposController(EquipoService service)
        {
            _service = service;
        }

        // ----------------- GETs -----------------

        /// <summary>
        /// Lista equipos con filtros opcionales.
        /// </summary>
        /// <param name="search">Texto a buscar (nombre/otros campos).</param>
        /// <param name="ciudad">Filtro por ciudad.</param>
        /// <param name="ct">Token de cancelación.</param>
        /// <returns>200 con colección de <see cref="EquipoResponseDto"/>.</returns>
        // GET api/equipos?search=&ciudad=
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<EquipoResponseDto>>> Get(
            [FromQuery] string? search,
            [FromQuery] string? ciudad,
            CancellationToken ct)
        {
            var equipos = await _service.GetListAsync(search, ciudad, ct);
            return Ok(equipos.Select(ToResponseDto));
        }

        /// <summary>
        /// Obtiene un equipo por su id.
        /// </summary>
        /// <param name="id">Identificador del equipo.</param>
        /// <param name="ct">Token de cancelación.</param>
        /// <returns>200 con equipo; 404 si no existe.</returns>
        // GET api/equipos/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<EquipoResponseDto>> GetById(int id, CancellationToken ct)
        {
            var e = await _service.GetByIdAsync(id, ct);
            if (e is null) return NotFound();
            return Ok(ToResponseDto(e));
        }

        /// <summary>
        /// Devuelve equipos paginados con filtros y ordenamiento.
        /// </summary>
        /// <param name="page">Página (>=1).</param>
        /// <param name="pageSize">Tamaño de página.</param>
        /// <param name="search">Texto de búsqueda.</param>
        /// <param name="ciudad">Filtro por ciudad.</param>
        /// <param name="sortBy">Campo de ordenamiento (por defecto <c>nombre</c>).</param>
        /// <param name="sortDir">Dirección: <c>asc</c> o <c>desc</c>.</param>
        /// <param name="ct">Token de cancelación.</param>
        /// <returns>200 con <see cref="PagedResult{T}"/> de <see cref="EquipoResponseDto"/>.</returns>
        // GET api/equipos/paged?page=1&pageSize=10&search=&ciudad=&sortBy=nombre&sortDir=asc
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<EquipoResponseDto>>> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? ciudad = null,
            [FromQuery] string? sortBy = "nombre",
            [FromQuery] string sortDir = "asc",
            CancellationToken ct = default)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;
            var asc = (sortDir ?? "asc").Equals("asc", StringComparison.OrdinalIgnoreCase);

            var (items, total) = await _service.GetPagedAsync(search, ciudad, sortBy, asc, page, pageSize, ct);
            var dtos = items.Select(ToResponseDto).ToList();
            return Ok(new PagedResult<EquipoResponseDto>(dtos, total, page, pageSize));
        }

        // ----------------- Mutaciones -----------------

        /// <summary>
        /// Crea un nuevo equipo (permite subir logo por formulario).
        /// </summary>
        /// <param name="form">Formulario con nombre, ciudad y archivo de logo.</param>
        /// <param name="ct">Token de cancelación.</param>
        /// <returns>201 con el recurso creado; 409 si ya existe un equipo con ese nombre.</returns>
        // POST api/equipos
#if DEBUG
        [AllowAnonymous]
#else
        [Authorize(Roles = "Admin")]
#endif
        [HttpPost]
        [RequestSizeLimit(5_000_000)]
        public async Task<ActionResult<EquipoResponseDto>> Create([FromForm] EquipoCreateFormDto form, CancellationToken ct)
        {
            try
            {
                var created = await _service.CreateAsync(form.Nombre, form.Ciudad, form.Logo, ct);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToResponseDto(created));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("existe un equipo", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new { message = ex.Message }); // 409 duplicado
            }
        }

        /// <summary>
        /// Actualiza un equipo existente (permite reemplazar el logo).
        /// </summary>
        /// <param name="id">Id del equipo.</param>
        /// <param name="form">Formulario con campos a actualizar.</param>
        /// <param name="ct">Token de cancelación.</param>
        /// <returns>200 con el equipo; 404 si no existe; 409 si hay conflicto de nombre.</returns>
        // PUT api/equipos/{id}
#if DEBUG
        [AllowAnonymous]
#else
        [Authorize(Roles = "Admin")]
#endif
        [HttpPut("{id:int}")]
        [RequestSizeLimit(5_000_000)]
        public async Task<ActionResult<EquipoResponseDto>> Update(int id, [FromForm] EquipoUpdateFormDto form, CancellationToken ct)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, form.Nombre, form.Ciudad, form.Logo, ct);
                if (updated is null) return NotFound();
                return Ok(ToResponseDto(updated));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("existe un equipo", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un equipo por id.
        /// </summary>
        /// <param name="id">Id del equipo.</param>
        /// <param name="ct">Token de cancelación.</param>
        /// <returns>204 si se elimina; 404 si no existe; 409 si está en uso (si el servicio lo valida).</returns>
        // DELETE api/equipos/{id}
#if DEBUG
        [AllowAnonymous]
#else
        [Authorize(Roles = "Admin")]
#endif
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            try
            {
                var ok = await _service.DeleteAsync(id, ct);
                if (!ok) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("está en uso", StringComparison.OrdinalIgnoreCase))
            {
                // Si activaste el chequeo "en uso" en el service, devolvemos 409 aquí
                return Conflict(new { message = ex.Message });
            }
        }

        // ----------------- Helpers -----------------

        /// <summary>
        /// Proyección de entidad <see cref="Equipo"/> a DTO de respuesta.
        /// </summary>
        private EquipoResponseDto ToResponseDto(Equipo e) => new()
        {
            Id = e.Id,
            Nombre = e.Nombre,
            Puntos = e.Puntos,
            Faltas = e.Faltas,
            Ciudad = e.Ciudad ?? "",
            LogoUrl = BuildLogoUrl(e.LogoFileName)
        };

        /// <summary>
        /// Construye URL absoluta del logo a partir del nombre de archivo.
        /// </summary>
        private string? BuildLogoUrl(string? fileName)
            => string.IsNullOrWhiteSpace(fileName)
                ? null
                : $"{Request.Scheme}://{Request.Host}/uploads/logos/{fileName}";
    }
}

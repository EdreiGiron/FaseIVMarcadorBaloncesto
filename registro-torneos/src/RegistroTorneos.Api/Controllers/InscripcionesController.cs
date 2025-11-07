using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using RegistroTorneos.Api.Domain;
using RegistroTorneos.Api.Infrastructure;

namespace RegistroTorneos.Api.Controllers;

[ApiController]
[Route("api/registro-torneos/inscripciones")]
[Produces("application/json")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class InscripcionesController : ControllerBase
{
    private readonly InscripcionRepositorio _repo;
    private readonly TorneosClient _torneos;
    public InscripcionesController(InscripcionRepositorio repo, TorneosClient torneos)
    {
        _repo = repo;
        _torneos = torneos;
    }

    private string? ObtenerUsuarioDeToken() =>
        User.FindFirst(ClaimTypes.Name)?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("name")?.Value
        ?? User.FindFirst("preferred_username")?.Value
        ?? User.FindFirst("unique_name")?.Value
        ?? User.FindFirst("sub")?.Value
        ?? User.Identity?.Name;

    public record CrearInscripcionDto(
        [Required, MinLength(1)] string TorneoId,
        Preferencias? Preferencias
    );

    [HttpPost]
    [Consumes("application/json")]
    public async Task<IActionResult> Crear([FromBody] CrearInscripcionDto dto, CancellationToken ct)
    {
        var usuarioId = ObtenerUsuarioDeToken();
        if (string.IsNullOrWhiteSpace(usuarioId))
            return Unauthorized(new { ok = false, mensaje = "Token sin usuario." });

        var torneoId = dto.TorneoId.Trim();

        try
        {
            var existeTorneo = await _torneos.ExistsAsync(torneoId);
            if (!existeTorneo)
                return NotFound(new { ok = false, mensaje = "El torneo no existe." });
        }
        catch
        {
            return StatusCode(502, new { ok = false, mensaje = "No se pudo validar el torneo en api_torneos." });
        }

        if (await _repo.ExisteAsync(usuarioId, torneoId))
            return Conflict(new { ok = false, mensaje = "Ya existe la inscripción." });

        var entidad = new Inscripcion
        {
            UsuarioId = usuarioId,
            TorneoId = torneoId,
            Preferencias = dto.Preferencias,
            CreadoUtc = DateTime.UtcNow
        };

        await _repo.InsertarAsync(entidad);

        return CreatedAtAction(nameof(Check), new { torneoId }, new { ok = true, id = entidad.Id });
    }

    [HttpGet("mis")]
    public async Task<IActionResult> Mis(CancellationToken ct)
    {
        var usuarioId = ObtenerUsuarioDeToken();
        if (string.IsNullOrWhiteSpace(usuarioId))
            return Unauthorized(new { ok = false, mensaje = "Token sin usuario." });

        var lista = await _repo.ListarPorUsuarioAsync(usuarioId);
        return Ok(new { ok = true, total = lista.Count, datos = lista });
    }

    [HttpGet("check/{torneoId}")]
    public async Task<IActionResult> Check([FromRoute] string torneoId, CancellationToken ct)
    {
        var usuarioId = ObtenerUsuarioDeToken();
        if (string.IsNullOrWhiteSpace(usuarioId))
            return Unauthorized(new { ok = false, mensaje = "Token sin usuario." });

        torneoId = (torneoId ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(torneoId))
            return BadRequest(new { ok = false, mensaje = "TorneoId es requerido." });

        var existe = await _repo.ExisteAsync(usuarioId, torneoId);
        return Ok(new { ok = true, registrado = existe });
    }

    [HttpDelete("{torneoId}")]
    public async Task<IActionResult> Eliminar([FromRoute] string torneoId, CancellationToken ct)
    {
        var usuarioId = ObtenerUsuarioDeToken();
        if (string.IsNullOrWhiteSpace(usuarioId))
            return Unauthorized(new { ok = false, mensaje = "Token sin usuario." });

        torneoId = (torneoId ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(torneoId))
            return BadRequest(new { ok = false, mensaje = "TorneoId es requerido." });

        var ok = await _repo.EliminarAsync(usuarioId, torneoId);
        return ok ? NoContent() : NotFound(new { ok = false, mensaje = "No existía la inscripción." });
    }
}
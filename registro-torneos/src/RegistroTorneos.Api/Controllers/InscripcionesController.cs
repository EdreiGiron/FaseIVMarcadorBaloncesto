using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RegistroTorneos.Api.Domain;
using RegistroTorneos.Api.Infrastructure;

namespace RegistroTorneos.Api.Controllers;

[ApiController]
[Route("api/registro-torneos/inscripciones")]
[Authorize] 
public class InscripcionesController : ControllerBase
{
    private readonly InscripcionRepositorio _repo;

    public InscripcionesController(InscripcionRepositorio repo) => _repo = repo;

    // Obtiene el usuario desde el token
    private string? ObtenerUsuarioDeToken() =>
        User.FindFirst("name")?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value
        ?? User.Identity?.Name;

    public record CrearInscripcionDto(string TorneoId, Preferencias? Preferencias);

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearInscripcionDto dto)
    {
        var usuarioId = ObtenerUsuarioDeToken();
        if (string.IsNullOrWhiteSpace(usuarioId))
            return Unauthorized(new { ok = false, mensaje = "Token sin usuario." });

        if (string.IsNullOrWhiteSpace(dto.TorneoId))
            return BadRequest(new { ok = false, mensaje = "TorneoId es requerido." });

        if (await _repo.ExisteAsync(usuarioId, dto.TorneoId))
            return Conflict(new { ok = false, mensaje = "Ya existe la inscripción." });

        var entidad = new Inscripcion
        {
            UsuarioId = usuarioId,
            TorneoId = dto.TorneoId.Trim(),
            Preferencias = dto.Preferencias,
            CreadoUtc = DateTime.UtcNow
        };

        await _repo.InsertarAsync(entidad);

        return CreatedAtAction(nameof(Check), new { torneoId = dto.TorneoId },
            new { ok = true, id = entidad.Id });
    }

    [HttpGet("mis")]
    public async Task<IActionResult> Mis()
    {
        var usuarioId = ObtenerUsuarioDeToken();
        if (string.IsNullOrWhiteSpace(usuarioId))
            return Unauthorized(new { ok = false, mensaje = "Token sin usuario." });

        var lista = await _repo.ListarPorUsuarioAsync(usuarioId);
        return Ok(new { ok = true, total = lista.Count, datos = lista });
    }

    [HttpGet("check/{torneoId}")]
    public async Task<IActionResult> Check([FromRoute] string torneoId)
    {
        var usuarioId = ObtenerUsuarioDeToken();
        if (string.IsNullOrWhiteSpace(usuarioId))
            return Unauthorized(new { ok = false, mensaje = "Token sin usuario." });

        if (string.IsNullOrWhiteSpace(torneoId))
            return BadRequest(new { ok = false, mensaje = "TorneoId es requerido." });

        var existe = await _repo.ExisteAsync(usuarioId, torneoId.Trim());
        return Ok(new { ok = true, registrado = existe });
    }

    [HttpDelete("{torneoId}")]
    public async Task<IActionResult> Eliminar([FromRoute] string torneoId)
    {
        var usuarioId = ObtenerUsuarioDeToken();
        if (string.IsNullOrWhiteSpace(usuarioId))
            return Unauthorized(new { ok = false, mensaje = "Token sin usuario." });

        if (string.IsNullOrWhiteSpace(torneoId))
            return BadRequest(new { ok = false, mensaje = "TorneoId es requerido." });

        var ok = await _repo.EliminarAsync(usuarioId, torneoId.Trim());
        return ok ? NoContent() : NotFound(new { ok = false, mensaje = "No existía la inscripción." });
    }
}

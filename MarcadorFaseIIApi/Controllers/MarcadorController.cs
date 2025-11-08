using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarcadorFaseIIApi.Models;
using MarcadorFaseIIApi.Models.DTOs;
using MarcadorFaseIIApi.Services;
using MarcadorFaseIIApi.Constrollers;

namespace MarcadorFaseIIApi.Constrollers;

/// <summary>
/// Controlador del marcador en vivo: estado global, tiempo, puntos, faltas y control de partido.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MarcadorController : ControllerBase
{
    private readonly MarcadorService _service;

    /// <summary>
    /// Crea el controlador del marcador con el servicio de dominio.
    /// </summary>
    /// <param name="service">Servicio de marcador.</param>
    public MarcadorController(MarcadorService service) => _service = service;

    // ---- Lecturas ----

    /// <summary>
    /// Obtiene el estado global del marcador actual.
    /// </summary>
    /// <returns>200 con <see cref="MarcadorGlobal"/>.</returns>
    [HttpGet]
    [AllowAnonymous]
    public ActionResult<MarcadorGlobal> GetMarcador() => Ok(_service.GetMarcador());

    /// <summary>
    /// Obtiene el estado del tiempo (cronómetro).
    /// </summary>
    /// <returns>200 con <see cref="EstadoTiempoDto"/>.</returns>
    [HttpGet("tiempo")]
    [AllowAnonymous]
    public ActionResult<EstadoTiempoDto> GetTiempo() => Ok(_service.GetEstadoTiempo());

    // ---- Puntos ----

    /// <summary>
    /// Suma puntos al equipo indicado.
    /// </summary>
    /// <param name="equipo">"local" o "visitante".</param>
    /// <param name="puntos">Cantidad de puntos a sumar.</param>
    /// <returns>200 con el nuevo marcador.</returns>
    [HttpPost("puntos/sumar")]
    public IActionResult SumarPuntos([FromQuery] string equipo, [FromQuery] int puntos)
    {
        if (!TryNormalizarEquipo(equipo, out var eq)) return BadRequest("Parametro 'equipo' debe ser 'local' o 'visitante'.");
        _service.SumarPuntos(eq, puntos);
        return Ok(_service.GetMarcador());
    }

    /// <summary>
    /// Resta puntos al equipo indicado.
    /// </summary>
    /// <param name="equipo">"local" o "visitante".</param>
    /// <param name="puntos">Cantidad de puntos a restar.</param>
    /// <returns>200 con el nuevo marcador.</returns>
    [HttpPost("puntos/restar")]
    public IActionResult RestarPuntos([FromQuery] string equipo, [FromQuery] int puntos)
    {
        if (!TryNormalizarEquipo(equipo, out var eq)) return BadRequest("Parametro 'equipo' debe ser 'local' o 'visitante'.");
        _service.RestarPuntos(eq, puntos);
        return Ok(_service.GetMarcador());
    }

    // ---- Faltas ----

    /// <summary>
    /// Registra una falta para el equipo indicado.
    /// </summary>
    /// <param name="equipo">"local" o "visitante".</param>
    /// <returns>200 con el nuevo marcador.</returns>
    [HttpPost("falta")]
    public IActionResult RegistrarFalta([FromQuery] string equipo)
    {
        if (!TryNormalizarEquipo(equipo, out var eq)) return BadRequest("Parametro 'equipo' debe ser 'local' o 'visitante'.");
        _service.RegistrarFalta(eq);
        return Ok(_service.GetMarcador());
    }

    // ---- Cuartos ----

    /// <summary>
    /// Avanza al siguiente cuarto del partido.
    /// </summary>
    /// <returns>200 con el número de cuarto actual tras avanzar.</returns>
    [HttpPost("cuarto/siguiente")]
    public IActionResult AvanzarCuarto()
    {
        var siguiente = _service.SiguienteCuarto();
        return Ok(siguiente);
    }

    // ---- Tiempo (rutas estilo tiempo/*) ----

    /// <summary>
    /// Inicia el tiempo del partido.
    /// </summary>
    /// <returns>200 con el marcador actualizado.</returns>
    [HttpPost("tiempo/iniciar")]
    public IActionResult IniciarTiempo()
    {
        _service.IniciarTiempo();
        return Ok(_service.GetMarcador());
    }

    /// <summary>
    /// Pausa el tiempo del partido.
    /// </summary>
    /// <returns>200 con el marcador actualizado.</returns>
    [HttpPost("tiempo/pausar")]
    public IActionResult PausarTiempo()
    {
        _service.PausarTiempo();
        return Ok(_service.GetMarcador());
    }

    /// <summary>
    /// Reanuda el tiempo del partido.
    /// </summary>
    /// <returns>200 con el marcador actualizado.</returns>
    [HttpPost("tiempo/reanudar")]
    public IActionResult ReanudarTiempo()
    {
        _service.ReanudarTiempo();
        return Ok(_service.GetMarcador());
    }

    /// <summary>
    /// Reinicia el tiempo al valor indicado (o al predeterminado si no se envía).
    /// </summary>
    /// <param name="seg">Segundos a establecer (opcional; por defecto, 600).</param>
    /// <returns>200 con <see cref="MarcadorGlobal"/>.</returns>
    [HttpPost("tiempo/reiniciar")]
    public ActionResult<MarcadorGlobal> ReiniciarTiempo([FromQuery] int? seg)
    {
        var resultado = _service.ReiniciarTiempo(seg ?? 600);
        return Ok(resultado);
    }

    /// <summary>
    /// Establece el tiempo directamente a una cantidad de segundos.
    /// </summary>
    /// <param name="seg">Segundos a establecer.</param>
    /// <returns>200 con <see cref="MarcadorGlobal"/>.</returns>
    [HttpPost("tiempo/establecer")]
    public ActionResult<MarcadorGlobal> EstablecerTiempo([FromQuery] int seg)
    {
        var resultado = _service.EstablecerTiempo(seg);

        return Ok(resultado);
    }

    // ---- Tiempo (rutas estilo reloj/*) ----

    /// <summary>
    /// Inicia el reloj (equivalente a iniciar tiempo).
    /// </summary>
    /// <returns>200 con <see cref="MarcadorGlobal"/>.</returns>
    [HttpPost("reloj/iniciar")]
    public ActionResult<MarcadorGlobal> IniciarReloj()
    {
        _service.IniciarReloj();
        return Ok(_service.GetMarcador());
    }

    /// <summary>
    /// Pausa el reloj.
    /// </summary>
    /// <returns>200 con <see cref="MarcadorGlobal"/>.</returns>
    [HttpPost("reloj/pausar")]
    public ActionResult<MarcadorGlobal> PausarReloj()
    {
        _service.PausarReloj();
        return Ok(_service.GetMarcador());
    }

    // ---- Equipos ----

    /// <summary>
    /// DTO de renombrado de equipos.
    /// </summary>
    public record RenombrarEquiposDto(string? Local, string? Visitante);

    /// <summary>
    /// Renombra los equipos actualizando la ficha vigente.
    /// </summary>
    /// <param name="dto">Nuevos nombres (local/visitante).</param>
    /// <returns>200 con <see cref="MarcadorGlobal"/> actualizado.</returns>
    [HttpPost("equipos/renombrar")]
    public ActionResult<MarcadorGlobal> RenombrarEquipos([FromBody] RenombrarEquiposDto dto)
    {
        var res = _service.RenombrarEquipos(dto.Local, dto.Visitante);
        return Ok(res);
    }

    /// <summary>
    /// Renombra equipos creando una nueva ficha de partido.
    /// </summary>
    /// <param name="local">Nombre del local.</param>
    /// <param name="visitante">Nombre del visitante.</param>
    /// <returns>200 con <see cref="MarcadorGlobal"/> de la nueva ficha.</returns>
    [HttpPost("equipos/renombrar-nuevo")]
    public ActionResult<MarcadorGlobal> RenombrarCreandoNuevaFicha([FromQuery] string? local, [FromQuery] string? visitante)
    {
        var nuevaFicha = _service.RenombrarCreandoNuevaFicha(local, visitante);
        return Ok(nuevaFicha);
    }

    /// <summary>
    /// Reinicia el marcador a cero (expuesto para uso desde el front).
    /// </summary>
    /// <returns>200 con <see cref="MarcadorGlobal"/> inicializado.</returns>
    //Exponer el endpoint para usarlo enel front 
    [HttpPost("reset-en-cero")]
    public ActionResult<MarcadorGlobal> ResetEnCero() => Ok(_service.InicializarEnCero());


    //______________PARTIDOS_____________

    /// <summary>
    /// DTO para terminar partido con motivo opcional.
    /// </summary>
    public class FinPartidoDto { public string? Motivo { get; set; } }

    /// <summary>
    /// Termina el partido explicitando un motivo (query o body).
    /// </summary>
    /// <param name="motivo">Motivo por query (opcional).</param>
    /// <param name="body">Body con motivo (opcional).</param>
    /// <returns>200 con <see cref="MarcadorGlobal"/> actualizado.</returns>
    [HttpPost("partido/terminar")]
    public ActionResult<MarcadorGlobal> Terminar([FromQuery] string? motivo, [FromBody] FinPartidoDto? body)
    {
        var reason = body?.Motivo ?? motivo;
        var res = _service.TerminarPartido(MarcadorService.EstadoPartido.Terminado, reason);
        return Ok(res);
    }

    /// <summary>
    /// Finaliza automáticamente el partido (aplica reglas internas del servicio).
    /// </summary>
    /// <returns>200 con datos de partido finalizado.</returns>
    [HttpPost("partido/finalizar-auto")]
    public ActionResult<MarcadorGlobal> FinalizarAuto()
    {
        var data = _service.TerminarPartido(motivo: null);
        return Ok(data);
    }

    /// <summary>
    /// Crea un nuevo partido reseteando estado y tiempos.
    /// </summary>
    /// <returns>200 con <see cref="MarcadorGlobal"/> del nuevo partido.</returns>
    // ---- Nuevo partido (resetea todo a 0 / duración actual) ----
    [HttpPost("nuevo")]
    public ActionResult<MarcadorGlobal> Nuevo() => Ok(_service.NuevoPartido());

    /// <summary>
    /// Inicializa el marcador con un partido específico del historial.
    /// </summary>
    /// <param name="partidoId">ID del partido a cargar.</param>
    /// <returns>200 con <see cref="MarcadorGlobal"/> inicializado.</returns>
    [HttpPost("inicializar/{partidoId:int}")]
    public ActionResult<MarcadorGlobal> InicializarConPartido(int partidoId) => Ok(_service.InicializarConPartido(partidoId));

    // -------- helpers --------

    /// <summary>
    /// Normaliza el texto "equipo" a "Local" o "Visitante".
    /// </summary>
    /// <param name="equipo">Texto entrante ("local"/"visitante").</param>
    /// <param name="normalizado">Resultado normalizado si es válido.</param>
    /// <returns><c>true</c> si es válido; en caso contrario, <c>false</c>.</returns>
    private static bool TryNormalizarEquipo(string? equipo, out string normalizado)
    {
        normalizado = "";
        if (string.IsNullOrWhiteSpace(equipo)) return false;
        var e = equipo.Trim().ToLowerInvariant();
        if (e == "local") { normalizado = "Local"; return true; }
        if (e == "visitante") { normalizado = "Visitante"; return true; }
        return false;
    }
}

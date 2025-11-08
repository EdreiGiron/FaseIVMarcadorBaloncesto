namespace MarcadorFaseIIApi.Models;

public class EstadisticaJugador
{
    public int EstadisticaJugadorId { get; set; }

    // FK al partido jugado (usaremos PartidoHistorico.Id)
    public int PartidoId { get; set; }
    public PartidoHistorico Partido { get; set; } = null!;

    // FK al jugador
    public int JugadorId { get; set; }
    public Jugador Jugador { get; set; } = null!;

    // Métricas básicas
    public int Puntos { get; set; }
    public int Rebotes { get; set; }
    public int Asistencias { get; set; }
    public int Robos { get; set; }
    public int Bloqueos { get; set; }
    public int Minutos { get; set; }
}

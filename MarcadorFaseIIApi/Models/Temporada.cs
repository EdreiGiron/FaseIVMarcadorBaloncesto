namespace MarcadorFaseIIApi.Models;

public class Temporada
{
    public int TemporadaId { get; set; }
    public string Nombre { get; set; } = null!;
    public int Anio { get; set; }
    public bool Activa { get; set; } = true;

    public ICollection<PartidoHistorico> Partidos { get; set; } = new List<PartidoHistorico>();
}

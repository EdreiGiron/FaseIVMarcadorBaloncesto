namespace MarcadorFaseIIApi.Models.DTOs.Partidos;

public class PartidoRosterItemDto
{
    public int PartidoId { get; set; }
    public int EquipoId { get; set; }
    public int JugadorId { get; set; }
    public string JugadorNombre { get; set; } = "";
    public string? Posicion { get; set; }
}

namespace MarcadorFaseIIApi.Models.DTOs;

public class RosterItemDto
{
    public int PartidoId { get; init; }
    public int EquipoId { get; init; }
    public int JugadorId { get; init; }
    public string JugadorNombre { get; init; } = "";
    public string Posicion { get; init; } = "";
}

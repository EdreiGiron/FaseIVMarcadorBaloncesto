using System.Text.Json.Serialization;

namespace MarcadorFaseIIApi.Models.DTOs.Playoffs;

public record CrearTorneoDto(
    [property: JsonPropertyName("nombre")] string Nombre, 
    [property: JsonPropertyName("temporada")] int Temporada, 
    [property: JsonPropertyName("bestOf")] int BestOf, 
    [property: JsonPropertyName("equipoIdsSeed")] List<int> EquipoIdsSeed);
public record TorneoDto(int Id, string Nombre, int Temporada, int BestOf, string Estado);

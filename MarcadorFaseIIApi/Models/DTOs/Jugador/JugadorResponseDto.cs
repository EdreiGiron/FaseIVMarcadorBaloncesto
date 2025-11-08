namespace MarcadorFaseIIApi.Models.DTOs.Jugador;

public class JugadorResponseDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Puntos { get; set; }
    public int Faltas { get; set; }
    public string? Posicion { get; set; }
    public int EquipoId { get; set; }
    public string EquipoNombre { get; set; } = string.Empty;
    public int? Numero { get; set; }         
    public int? Edad { get; set; }          
    public int Estatura { get; set; }    
    public string? Nacionalidad { get; set; } 

    

}

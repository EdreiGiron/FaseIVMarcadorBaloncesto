using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarcadorFaseIIApi.Models;

public class Jugador
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public string Nombre { get; set; } = string.Empty;
    public int Puntos { get; set; }
    public int Faltas { get; set; }
    public string? Posicion { get; set; }
    public int EquipoId { get; set; }
    public Equipo Equipo { get; set; } = null!;
    public string numero { get; set; } = string.Empty;
    public int edad { get; set; }
    public int estatura { get; set; }
    public string nacionalidad { get; set; } = string.Empty;
}

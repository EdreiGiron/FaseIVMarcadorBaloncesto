using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarcadorFaseIIApi.Models;

/// <summary>Estadísticas de un jugador en un partido.</summary>
public class PartidoJugadorStat
{
    [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int PartidoId { get; set; }
        public int JugadorId { get; set; }
        public int EquipoId  { get; set; }

        public int Minutos { get; set; }
        public int Puntos  { get; set; }
        public int Rebotes { get; set; }
        public int Asistencias { get; set; }
        public int Robos   { get; set; }
        public int Bloqueos { get; set; }
        public int Perdidas { get; set; }
        public int Faltas   { get; set; }

        public int FGM { get; set; }
        public int FGA { get; set; }
        public int TPM { get; set; }
        public int TPA { get; set; }
        public int FTM { get; set; }
        public int FTA { get; set; }

        // Navs
        public Partido Partido { get; set; } = null!;
        public Jugador Jugador { get; set; } = null!;   
}

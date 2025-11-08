namespace MarcadorFaseIIApi.Models.DTOs.Stats;

public record PlayerStatUpsertDto
{
  public int JugadorId { get; set; }
  public int EquipoId { get; set; }

  public int Minutos { get; set; }
  public int Puntos { get; set; }
  public int Rebotes { get; set; }
  public int Asistencias { get; set; }
  public int Robos { get; set; }
  public int Bloqueos { get; set; }
  public int Perdidas { get; set; }
  public int Faltas { get; set; }

  public int FGM { get; set; }  // Field Goals Made
  public int FGA { get; set; }  // Field Goals Attempted
  public int TPM { get; set; }  // 3pt Made
  public int TPA { get; set; }  // 3pt Attempted
  public int FTM { get; set; }  // Free Throws Made
  public int FTA { get; set; }  // Free Throws Attempted
};

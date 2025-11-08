namespace MarcadorFaseIIApi.Models.DTOs.Stats;

public class PlayerSeasonStatsDto
{
      // Identidad
        public int    JugadorId     { get; set; }
        public string Nombre        { get; set; } = string.Empty;
        public string? Posicion     { get; set; }
        public int?   Edad          { get; set; }
        public int    Estatura      { get; set; }
        public string? Nacionalidad { get; set; }
        public string EquipoNombre  { get; set; } = string.Empty;

        // Acumulados
        public int Juegos             { get; set; }
        public int MinutosTotales     { get; set; }
        public int PuntosTotales      { get; set; }
        public int RebotesTotales     { get; set; }
        public int AsistenciasTotales { get; set; }
        public int RobosTotales       { get; set; }
        public int BloqueosTotales    { get; set; }
        public int PerdidasTotales    { get; set; }
        public int FaltasTotales      { get; set; }

        public int FGM { get; set; }
        public int FGA { get; set; }
        public int TPM { get; set; }
        public int TPA { get; set; }
        public int FTM { get; set; }
        public int FTA { get; set; }

        // Promedios por juego
        public double MIN { get; set; }
        public double PPG { get; set; }
        public double RPG { get; set; }
        public double APG { get; set; }
        public double SPG { get; set; }
        public double BPG { get; set; }
        public double TOV { get; set; }
        public double PF  { get; set; }

        // Porcentajes (0..1)
        public double FG  { get; set; }
        public double TP3 { get; set; }
        public double FT  { get; set; }

        // Opcional (si luego los almacenas)
        public string? Fortalezas  { get; set; }
        public string? Debilidades { get; set; }
}

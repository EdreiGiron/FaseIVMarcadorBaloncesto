using Microsoft.EntityFrameworkCore;
using MarcadorFaseIIApi.Models;

namespace MarcadorFaseIIApi.Data;

public class MarcadorDbContext : DbContext
{
    public MarcadorDbContext(DbContextOptions<MarcadorDbContext> options)
            : base(options) { }

    public DbSet<Equipo> Equipos { get; set; }
    public DbSet<Jugador> Jugadores { get; set; }
    public DbSet<MarcadorGlobal> Marcadores { get; set; }
    public DbSet<Falta> Faltas { get; set; }
    public DbSet<PartidoHistorico> PartidosHistoricos { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Torneo> Torneos { get; set; }
    public DbSet<SeriePlayoff> Series { get; set; }
    public DbSet<Partido> Partidos { get; set; }
    public DbSet<PartidoJugador> PartidosJugadores { get; set; }
    public DbSet<Temporada> Temporadas => Set<Temporada>();
    public DbSet<EstadisticaJugador> EstadisticasJugador => Set<EstadisticaJugador>();
    public DbSet<PartidoJugadorStat> PartidoJugadorStats { get; set; } = null!;

    /// <summary>
    /// Configura el modelo y las relaciones mediante Fluent API.
    /// Define claves, restricciones, tipos de columna, índices y relaciones entre entidades.
    /// </summary>
    /// <param name="modelBuilder">Constructor de modelo de EF Core.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<PartidoJugadorStat>(e =>
        {
            e.HasIndex(x => new { x.PartidoId, x.JugadorId }).IsUnique();

            e.HasOne(x => x.Partido)
            .WithMany()
            .HasForeignKey(x => x.PartidoId)
            .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Jugador)
            .WithMany()
            .HasForeignKey(x => x.JugadorId)
            .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Temporada>().ToTable("Temporada")
            .HasKey(t => t.TemporadaId);

        modelBuilder.Entity<EstadisticaJugador>().ToTable("EstadisticaJugador")
            .HasKey(x => x.EstadisticaJugadorId);

        modelBuilder.Entity<PartidoHistorico>()
            .HasOne(p => p.Temporada)
            .WithMany(t => t.Partidos)
            .HasForeignKey(p => p.TemporadaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EstadisticaJugador>()
            .HasOne(e => e.Partido)
            .WithMany()
            .HasForeignKey(e => e.PartidoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EstadisticaJugador>()
            .HasOne(e => e.Jugador)
            .WithMany()
            .HasForeignKey(e => e.JugadorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Equipo>(e =>
        {
            e.Property(x => x.Id).UseIdentityColumn();
            e.Property(x => x.Nombre)
            .IsRequired()
            .HasColumnType("nvarchar(max)");
            e.Property(x => x.Ciudad)
            .IsRequired()
            .HasMaxLength(80);
            e.Property(x => x.LogoFileName).HasMaxLength(128);
        });

        modelBuilder.Entity<MarcadorGlobal>()
            .HasOne(m => m.EquipoLocal)
            .WithMany()
            .HasForeignKey("EquipoLocalId")
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MarcadorGlobal>()
            .HasOne(m => m.EquipoVisitante)
            .WithMany()
            .HasForeignKey("EquipoVisitanteId")
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PartidoHistorico>()
            .Property(p => p.Id)
            .UseIdentityColumn();

        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey("RoleId")
            .OnDelete(DeleteBehavior.Restrict);

        // Torneo
        modelBuilder.Entity<Torneo>(t =>
        {
            t.Property(x => x.Id).UseIdentityColumn();
            t.Property(x => x.Nombre).IsRequired().HasMaxLength(120);
        });

        // SeriePlayoff
        modelBuilder.Entity<SeriePlayoff>(s =>
        {
            s.Property(x => x.Id).UseIdentityColumn();
            s.HasOne(x => x.Torneo)
                .WithMany(t => t.Series)
                .HasForeignKey(x => x.TorneoId)
                .OnDelete(DeleteBehavior.Cascade);

            s.HasIndex(x => new { x.TorneoId, x.Ronda });
        });

        // Partido
        modelBuilder.Entity<Partido>(p =>
        {
            p.Property(x => x.Id).UseIdentityColumn();
            p.HasOne(x => x.Serie)
                .WithMany(s => s.Partidos)
                .HasForeignKey(x => x.SeriePlayoffId)
                .OnDelete(DeleteBehavior.Cascade);

            p.HasIndex(x => new { x.SeriePlayoffId, x.GameNumber }).IsUnique();
        });

        // PartidoJugador (Roster)
        modelBuilder.Entity<PartidoJugador>(r =>
        {
            r.HasKey(x => new { x.PartidoId, x.EquipoId, x.JugadorId });
            r.HasOne(x => x.Partido)
                .WithMany(p => p.Roster)
                .HasForeignKey(x => x.PartidoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        //Indice de username
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        //Indices sobre FK
        modelBuilder.Entity<PartidoHistorico>()
            .HasIndex(p => p.EquipoLocalId);

        modelBuilder.Entity<PartidoHistorico>()
            .HasIndex(p => p.EquipoVisitanteId);

        modelBuilder.Entity<Jugador>(j =>
        {
            j.Property(x => x.Id).UseIdentityColumn();
            j.Property(x => x.Nombre).IsRequired().HasColumnType("nvarchar(max)");
            j.Property(x => x.Posicion).HasMaxLength(40); // opcionalmente 40–60
            j.HasOne(x => x.Equipo)
                .WithMany(e => e.Jugadores)
                .HasForeignKey(x => x.EquipoId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

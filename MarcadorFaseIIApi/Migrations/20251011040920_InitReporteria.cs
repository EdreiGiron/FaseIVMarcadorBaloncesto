using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarcadorFaseIIApi.Migrations
{
    /// <inheritdoc />
    public partial class InitReporteria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TemporadaId",
                table: "PartidosHistoricos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EstadisticaJugador",
                columns: table => new
                {
                    EstadisticaJugadorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartidoId = table.Column<int>(type: "int", nullable: false),
                    JugadorId = table.Column<int>(type: "int", nullable: false),
                    Puntos = table.Column<int>(type: "int", nullable: false),
                    Rebotes = table.Column<int>(type: "int", nullable: false),
                    Asistencias = table.Column<int>(type: "int", nullable: false),
                    Robos = table.Column<int>(type: "int", nullable: false),
                    Bloqueos = table.Column<int>(type: "int", nullable: false),
                    Minutos = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadisticaJugador", x => x.EstadisticaJugadorId);
                    table.ForeignKey(
                        name: "FK_EstadisticaJugador_Jugadores_JugadorId",
                        column: x => x.JugadorId,
                        principalTable: "Jugadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EstadisticaJugador_PartidosHistoricos_PartidoId",
                        column: x => x.PartidoId,
                        principalTable: "PartidosHistoricos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Temporada",
                columns: table => new
                {
                    TemporadaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Anio = table.Column<int>(type: "int", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Temporada", x => x.TemporadaId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartidosHistoricos_TemporadaId",
                table: "PartidosHistoricos",
                column: "TemporadaId");

            migrationBuilder.CreateIndex(
                name: "IX_EstadisticaJugador_JugadorId",
                table: "EstadisticaJugador",
                column: "JugadorId");

            migrationBuilder.CreateIndex(
                name: "IX_EstadisticaJugador_PartidoId",
                table: "EstadisticaJugador",
                column: "PartidoId");

            migrationBuilder.AddForeignKey(
                name: "FK_PartidosHistoricos_Temporada_TemporadaId",
                table: "PartidosHistoricos",
                column: "TemporadaId",
                principalTable: "Temporada",
                principalColumn: "TemporadaId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartidosHistoricos_Temporada_TemporadaId",
                table: "PartidosHistoricos");

            migrationBuilder.DropTable(
                name: "EstadisticaJugador");

            migrationBuilder.DropTable(
                name: "Temporada");

            migrationBuilder.DropIndex(
                name: "IX_PartidosHistoricos_TemporadaId",
                table: "PartidosHistoricos");

            migrationBuilder.DropColumn(
                name: "TemporadaId",
                table: "PartidosHistoricos");
        }
    }
}

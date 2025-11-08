using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarcadorFaseIIApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PartidoJugadorStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartidoId = table.Column<int>(type: "int", nullable: false),
                    JugadorId = table.Column<int>(type: "int", nullable: false),
                    EquipoId = table.Column<int>(type: "int", nullable: false),
                    Minutos = table.Column<int>(type: "int", nullable: false),
                    Puntos = table.Column<int>(type: "int", nullable: false),
                    Rebotes = table.Column<int>(type: "int", nullable: false),
                    Asistencias = table.Column<int>(type: "int", nullable: false),
                    Robos = table.Column<int>(type: "int", nullable: false),
                    Bloqueos = table.Column<int>(type: "int", nullable: false),
                    Perdidas = table.Column<int>(type: "int", nullable: false),
                    Faltas = table.Column<int>(type: "int", nullable: false),
                    FGM = table.Column<int>(type: "int", nullable: false),
                    FGA = table.Column<int>(type: "int", nullable: false),
                    TPM = table.Column<int>(type: "int", nullable: false),
                    TPA = table.Column<int>(type: "int", nullable: false),
                    FTM = table.Column<int>(type: "int", nullable: false),
                    FTA = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartidoJugadorStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartidoJugadorStats_Jugadores_JugadorId",
                        column: x => x.JugadorId,
                        principalTable: "Jugadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartidoJugadorStats_Partidos_PartidoId",
                        column: x => x.PartidoId,
                        principalTable: "Partidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartidoJugadorStats_JugadorId",
                table: "PartidoJugadorStats",
                column: "JugadorId");

            migrationBuilder.CreateIndex(
                name: "IX_PartidoJugadorStats_PartidoId_JugadorId",
                table: "PartidoJugadorStats",
                columns: new[] { "PartidoId", "JugadorId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartidoJugadorStats");
        }
    }
}

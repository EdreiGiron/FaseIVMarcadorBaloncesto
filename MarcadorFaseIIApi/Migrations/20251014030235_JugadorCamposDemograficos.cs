using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarcadorFaseIIApi.Migrations
{
    /// <inheritdoc />
    public partial class JugadorCamposDemograficos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "edad",
                table: "Jugadores",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "estatura",
                table: "Jugadores",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "nacionalidad",
                table: "Jugadores",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "numero",
                table: "Jugadores",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "edad",
                table: "Jugadores");

            migrationBuilder.DropColumn(
                name: "estatura",
                table: "Jugadores");

            migrationBuilder.DropColumn(
                name: "nacionalidad",
                table: "Jugadores");

            migrationBuilder.DropColumn(
                name: "numero",
                table: "Jugadores");
        }
    }
}

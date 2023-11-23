using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KlaskApi.Migrations
{
    /// <inheritdoc />
    public partial class AnzahlGruppenRenamed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AnazhlGruppen",
                table: "Turniere",
                newName: "AnzahlGruppen");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AnzahlGruppen",
                table: "Turniere",
                newName: "AnazhlGruppen");
        }
    }
}

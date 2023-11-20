using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KlaskApi.Migrations
{
    /// <inheritdoc />
    public partial class ModifyTheModelsContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SpielId1",
                table: "SpieleTeilnehmer",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpieleTeilnehmer_SpielId1",
                table: "SpieleTeilnehmer",
                column: "SpielId1");

            migrationBuilder.AddForeignKey(
                name: "FK_SpieleTeilnehmer_Spiele_SpielId1",
                table: "SpieleTeilnehmer",
                column: "SpielId1",
                principalTable: "Spiele",
                principalColumn: "SpielId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpieleTeilnehmer_Spiele_SpielId1",
                table: "SpieleTeilnehmer");

            migrationBuilder.DropIndex(
                name: "IX_SpieleTeilnehmer_SpielId1",
                table: "SpieleTeilnehmer");

            migrationBuilder.DropColumn(
                name: "SpielId1",
                table: "SpieleTeilnehmer");
        }
    }
}

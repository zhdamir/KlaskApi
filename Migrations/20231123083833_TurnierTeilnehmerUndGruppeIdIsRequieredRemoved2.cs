using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KlaskApi.Migrations
{
    /// <inheritdoc />
    public partial class TurnierTeilnehmerUndGruppeIdIsRequieredRemoved2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TurnierTeilnehmer_Gruppen_GruppeId",
                table: "TurnierTeilnehmer");

            migrationBuilder.AlterColumn<long>(
                name: "GruppeId",
                table: "TurnierTeilnehmer",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_TurnierTeilnehmer_Gruppen_GruppeId",
                table: "TurnierTeilnehmer",
                column: "GruppeId",
                principalTable: "Gruppen",
                principalColumn: "GruppeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TurnierTeilnehmer_Gruppen_GruppeId",
                table: "TurnierTeilnehmer");

            migrationBuilder.AlterColumn<long>(
                name: "GruppeId",
                table: "TurnierTeilnehmer",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TurnierTeilnehmer_Gruppen_GruppeId",
                table: "TurnierTeilnehmer",
                column: "GruppeId",
                principalTable: "Gruppen",
                principalColumn: "GruppeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

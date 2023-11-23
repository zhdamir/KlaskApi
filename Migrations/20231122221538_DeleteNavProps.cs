using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KlaskApi.Migrations
{
    /// <inheritdoc />
    public partial class DeleteNavProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpieleTeilnehmer_Spiele_SpielId1",
                table: "SpieleTeilnehmer");

            migrationBuilder.DropForeignKey(
                name: "FK_TurnierTeilnehmer_Teilnehmer_TurnierId",
                table: "TurnierTeilnehmer");

            migrationBuilder.DropIndex(
                name: "IX_SpieleTeilnehmer_SpielId1",
                table: "SpieleTeilnehmer");

            migrationBuilder.DropColumn(
                name: "SpielId1",
                table: "SpieleTeilnehmer");

            migrationBuilder.AlterColumn<string>(
                name: "Vorname",
                table: "Teilnehmer",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Nachname",
                table: "Teilnehmer",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Teilnehmer",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "RundeBezeichnung",
                table: "Runden",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "LoginDaten",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Passwort",
                table: "LoginDaten",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Gruppenname",
                table: "Gruppen",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "BereichName",
                table: "Bereich",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "BezeichnungRolle",
                table: "BenutzerRolle",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_TurnierTeilnehmer_TeilnehmerId",
                table: "TurnierTeilnehmer",
                column: "TeilnehmerId");

            migrationBuilder.AddForeignKey(
                name: "FK_TurnierTeilnehmer_Teilnehmer_TeilnehmerId",
                table: "TurnierTeilnehmer",
                column: "TeilnehmerId",
                principalTable: "Teilnehmer",
                principalColumn: "TeilnehmerId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TurnierTeilnehmer_Teilnehmer_TeilnehmerId",
                table: "TurnierTeilnehmer");

            migrationBuilder.DropIndex(
                name: "IX_TurnierTeilnehmer_TeilnehmerId",
                table: "TurnierTeilnehmer");

            migrationBuilder.AlterColumn<string>(
                name: "Vorname",
                table: "Teilnehmer",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nachname",
                table: "Teilnehmer",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Teilnehmer",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SpielId1",
                table: "SpieleTeilnehmer",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RundeBezeichnung",
                table: "Runden",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "LoginDaten",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Passwort",
                table: "LoginDaten",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Gruppenname",
                table: "Gruppen",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BereichName",
                table: "Bereich",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BezeichnungRolle",
                table: "BenutzerRolle",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_TurnierTeilnehmer_Teilnehmer_TurnierId",
                table: "TurnierTeilnehmer",
                column: "TurnierId",
                principalTable: "Teilnehmer",
                principalColumn: "TeilnehmerId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

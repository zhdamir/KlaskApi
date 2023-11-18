using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KlaskApi.Migrations
{
    /// <inheritdoc />
    public partial class ModifyTheRelationshipTurnierTeilnehmerGruppe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teilnehmer_BenutzerRolle_RoleId",
                table: "Teilnehmer");

            migrationBuilder.DropForeignKey(
                name: "FK_Teilnehmer_Gruppen_GruppeId",
                table: "Teilnehmer");

            migrationBuilder.DropIndex(
                name: "IX_Teilnehmer_GruppeId",
                table: "Teilnehmer");

            migrationBuilder.DropIndex(
                name: "IX_Teilnehmer_RoleId",
                table: "Teilnehmer");

            migrationBuilder.DropColumn(
                name: "GruppeId",
                table: "Teilnehmer");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Teilnehmer");

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
                name: "Gruppenname",
                table: "Gruppen",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "TurnierTeilnehmer",
                columns: table => new
                {
                    TurnierTeilnehmerId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TurnierId = table.Column<long>(type: "bigint", nullable: false),
                    TeilnehmerId = table.Column<long>(type: "bigint", nullable: false),
                    GruppeId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TurnierTeilnehmer", x => x.TurnierTeilnehmerId);
                    table.ForeignKey(
                        name: "FK_TurnierTeilnehmer_Gruppen_GruppeId",
                        column: x => x.GruppeId,
                        principalTable: "Gruppen",
                        principalColumn: "GruppeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TurnierTeilnehmer_Teilnehmer_TurnierId",
                        column: x => x.TurnierId,
                        principalTable: "Teilnehmer",
                        principalColumn: "TeilnehmerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TurnierTeilnehmer_Turniere_TurnierId",
                        column: x => x.TurnierId,
                        principalTable: "Turniere",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Teilnehmer_RolleId",
                table: "Teilnehmer",
                column: "RolleId");

            migrationBuilder.CreateIndex(
                name: "IX_TurnierTeilnehmer_GruppeId",
                table: "TurnierTeilnehmer",
                column: "GruppeId");

            migrationBuilder.CreateIndex(
                name: "IX_TurnierTeilnehmer_TurnierId",
                table: "TurnierTeilnehmer",
                column: "TurnierId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teilnehmer_BenutzerRolle_RolleId",
                table: "Teilnehmer",
                column: "RolleId",
                principalTable: "BenutzerRolle",
                principalColumn: "RolleId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teilnehmer_BenutzerRolle_RolleId",
                table: "Teilnehmer");

            migrationBuilder.DropTable(
                name: "TurnierTeilnehmer");

            migrationBuilder.DropIndex(
                name: "IX_Teilnehmer_RolleId",
                table: "Teilnehmer");

            migrationBuilder.AddColumn<long>(
                name: "GruppeId",
                table: "Teilnehmer",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "RoleId",
                table: "Teilnehmer",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<string>(
                name: "RundeBezeichnung",
                table: "Runden",
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

            migrationBuilder.CreateIndex(
                name: "IX_Teilnehmer_GruppeId",
                table: "Teilnehmer",
                column: "GruppeId");

            migrationBuilder.CreateIndex(
                name: "IX_Teilnehmer_RoleId",
                table: "Teilnehmer",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teilnehmer_BenutzerRolle_RoleId",
                table: "Teilnehmer",
                column: "RoleId",
                principalTable: "BenutzerRolle",
                principalColumn: "RolleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Teilnehmer_Gruppen_GruppeId",
                table: "Teilnehmer",
                column: "GruppeId",
                principalTable: "Gruppen",
                principalColumn: "GruppeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

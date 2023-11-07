using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KlaskApi.Migrations
{
    /// <inheritdoc />
    public partial class AddBenutzerRolle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "RolleId",
                table: "Teilnehmer",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "BenutzerRolle",
                columns: table => new
                {
                    RolleId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BezeichnungRolle = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BenutzerRolle", x => x.RolleId);
                });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teilnehmer_BenutzerRolle_RoleId",
                table: "Teilnehmer");

            migrationBuilder.DropTable(
                name: "BenutzerRolle");

            migrationBuilder.DropIndex(
                name: "IX_Teilnehmer_RoleId",
                table: "Teilnehmer");

            migrationBuilder.DropColumn(
                name: "RolleId",
                table: "Teilnehmer");
        }
    }
}

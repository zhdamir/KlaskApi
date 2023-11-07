using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KlaskApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSpielRundeModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Runden",
                columns: table => new
                {
                    RundeId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RundeBezeichnung = table.Column<string>(type: "text", nullable: true),
                    TurnierId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Runden", x => x.RundeId);
                    table.ForeignKey(
                        name: "FK_Runden_Turniere_TurnierId",
                        column: x => x.TurnierId,
                        principalTable: "Turniere",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Spiele",
                columns: table => new
                {
                    SpielId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RundeId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spiele", x => x.SpielId);
                    table.ForeignKey(
                        name: "FK_Spiele_Runden_RundeId",
                        column: x => x.RundeId,
                        principalTable: "Runden",
                        principalColumn: "RundeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpieleTeilnehmer",
                columns: table => new
                {
                    SpielTeilnehmerId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SpielId = table.Column<long>(type: "bigint", nullable: false),
                    TeilnehmerId = table.Column<long>(type: "bigint", nullable: false),
                    Punkte = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpieleTeilnehmer", x => x.SpielTeilnehmerId);
                    table.ForeignKey(
                        name: "FK_SpieleTeilnehmer_Spiele_SpielId",
                        column: x => x.SpielId,
                        principalTable: "Spiele",
                        principalColumn: "SpielId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpieleTeilnehmer_Teilnehmer_TeilnehmerId",
                        column: x => x.TeilnehmerId,
                        principalTable: "Teilnehmer",
                        principalColumn: "TeilnehmerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Runden_TurnierId",
                table: "Runden",
                column: "TurnierId");

            migrationBuilder.CreateIndex(
                name: "IX_Spiele_RundeId",
                table: "Spiele",
                column: "RundeId");

            migrationBuilder.CreateIndex(
                name: "IX_SpieleTeilnehmer_SpielId",
                table: "SpieleTeilnehmer",
                column: "SpielId");

            migrationBuilder.CreateIndex(
                name: "IX_SpieleTeilnehmer_TeilnehmerId",
                table: "SpieleTeilnehmer",
                column: "TeilnehmerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpieleTeilnehmer");

            migrationBuilder.DropTable(
                name: "Spiele");

            migrationBuilder.DropTable(
                name: "Runden");
        }
    }
}

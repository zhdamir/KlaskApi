using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KlaskApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTeilnehmerBereichModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bereich",
                columns: table => new
                {
                    BereichId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BereichName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bereich", x => x.BereichId);
                });

            migrationBuilder.CreateTable(
                name: "Teilnehmer",
                columns: table => new
                {
                    TeilnehmerId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Vorname = table.Column<string>(type: "text", nullable: false),
                    Nachname = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    BereichId = table.Column<long>(type: "bigint", nullable: false),
                    GruppeId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teilnehmer", x => x.TeilnehmerId);
                    table.ForeignKey(
                        name: "FK_Teilnehmer_Bereich_BereichId",
                        column: x => x.BereichId,
                        principalTable: "Bereich",
                        principalColumn: "BereichId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Teilnehmer_Gruppen_GruppeId",
                        column: x => x.GruppeId,
                        principalTable: "Gruppen",
                        principalColumn: "GruppeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Teilnehmer_BereichId",
                table: "Teilnehmer",
                column: "BereichId");

            migrationBuilder.CreateIndex(
                name: "IX_Teilnehmer_GruppeId",
                table: "Teilnehmer",
                column: "GruppeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Teilnehmer");

            migrationBuilder.DropTable(
                name: "Bereich");
        }
    }
}

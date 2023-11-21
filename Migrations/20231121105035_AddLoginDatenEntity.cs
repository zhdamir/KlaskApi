using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KlaskApi.Migrations
{
    /// <inheritdoc />
    public partial class AddLoginDatenEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoginDaten",
                columns: table => new
                {
                    LoginId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    Passwort = table.Column<string>(type: "text", nullable: false),
                    TeilnehmerId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginDaten", x => x.LoginId);
                    table.ForeignKey(
                        name: "FK_LoginDaten_Teilnehmer_TeilnehmerId",
                        column: x => x.TeilnehmerId,
                        principalTable: "Teilnehmer",
                        principalColumn: "TeilnehmerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoginDaten_TeilnehmerId",
                table: "LoginDaten",
                column: "TeilnehmerId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoginDaten");
        }
    }
}

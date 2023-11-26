using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KlaskApi.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusProptoTurnier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Turniere",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Turniere");
        }
    }
}

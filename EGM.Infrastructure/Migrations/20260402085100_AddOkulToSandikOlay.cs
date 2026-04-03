using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOkulToSandikOlay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Okul",
                table: "SandikOlaylar",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Okul",
                table: "SandikOlaylar");
        }
    }
}

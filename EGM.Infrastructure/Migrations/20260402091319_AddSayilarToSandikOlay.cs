using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSayilarToSandikOlay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GozaltiSayisi",
                table: "SandikOlaylar",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "KatilimciSayisi",
                table: "SandikOlaylar",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OluSayisi",
                table: "SandikOlaylar",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SehitSayisi",
                table: "SandikOlaylar",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GozaltiSayisi",
                table: "SandikOlaylar");

            migrationBuilder.DropColumn(
                name: "KatilimciSayisi",
                table: "SandikOlaylar");

            migrationBuilder.DropColumn(
                name: "OluSayisi",
                table: "SandikOlaylar");

            migrationBuilder.DropColumn(
                name: "SehitSayisi",
                table: "SandikOlaylar");
        }
    }
}

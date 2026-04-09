using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTakipNo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TakipNo",
                table: "VIPZiyaretler",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TakipNo",
                table: "SosyalMedyaOlaylar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TakipNo",
                table: "SandikOlaylar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TakipNo",
                table: "Olaylar",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TakipNo",
                table: "VIPZiyaretler");

            migrationBuilder.DropColumn(
                name: "TakipNo",
                table: "SosyalMedyaOlaylar");

            migrationBuilder.DropColumn(
                name: "TakipNo",
                table: "SandikOlaylar");

            migrationBuilder.DropColumn(
                name: "TakipNo",
                table: "Olaylar");
        }
    }
}

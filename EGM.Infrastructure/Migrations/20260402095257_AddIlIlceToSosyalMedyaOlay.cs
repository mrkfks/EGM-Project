using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIlIlceToSosyalMedyaOlay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Il",
                table: "SosyalMedyaOlaylar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ilce",
                table: "SosyalMedyaOlaylar",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Il",
                table: "SosyalMedyaOlaylar");

            migrationBuilder.DropColumn(
                name: "Ilce",
                table: "SosyalMedyaOlaylar");
        }
    }
}

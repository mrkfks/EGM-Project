using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSosyalSignalSkoru : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SosyalSignalSkoru",
                table: "SosyalMedyaOlaylar");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "SosyalSignalSkoru",
                table: "SosyalMedyaOlaylar",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}

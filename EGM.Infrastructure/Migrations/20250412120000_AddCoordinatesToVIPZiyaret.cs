using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCoordinatesToVIPZiyaret : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "VIPZiyaretler",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "VIPZiyaretler",
                type: "REAL",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "VIPZiyaretler");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "VIPZiyaretler");
        }
    }
}

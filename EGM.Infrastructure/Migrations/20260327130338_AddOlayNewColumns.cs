using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOlayNewColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EvrakNumarasi",
                table: "Olaylar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GozaltiSayisi",
                table: "Olaylar",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SehitOluSayisi",
                table: "Olaylar",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EvrakNumarasi",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "GozaltiSayisi",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "SehitOluSayisi",
                table: "Olaylar");
        }
    }
}

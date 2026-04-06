using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBaslikRiskKaynakKurum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Baslik",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "KaynakKurum",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "RiskPuani",
                table: "Olaylar");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Baslik",
                table: "Olaylar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KaynakKurum",
                table: "Olaylar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RiskPuani",
                table: "Olaylar",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}

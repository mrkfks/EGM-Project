using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RolveYetki : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "YuruyusRotasi",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "VIPZiyaretler",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Supheliler",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "SosyalMedyaOlaylar",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Sehitler",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "SecimSonuclari",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "SecimKaynaklar",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Partiler",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Organizatorler",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "OperasyonelFaaliyetler",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Oluler",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "Olaylar",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Olaylar",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Konular",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "KatilimciGruplar",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "KategoriOrganizatorler",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "GuvenlikPlanlari",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Ekipler",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "AuditLoglar",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Adaylar",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "YuruyusRotasi");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "VIPZiyaretler");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Supheliler");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "SosyalMedyaOlaylar");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Sehitler");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "SecimSonuclari");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "SecimKaynaklar");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Partiler");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Organizatorler");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "OperasyonelFaaliyetler");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Oluler");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Konular");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "KatilimciGruplar");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "KategoriOrganizatorler");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "GuvenlikPlanlari");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Ekipler");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "AuditLoglar");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Adaylar");
        }
    }
}

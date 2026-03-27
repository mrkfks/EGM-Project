using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSosyalMedyaEkranGoruntusu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SosyalMedyaOlaylar_Olaylar_OlayId",
                table: "SosyalMedyaOlaylar");

            migrationBuilder.AlterColumn<Guid>(
                name: "OlayId",
                table: "SosyalMedyaOlaylar",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "EkranGoruntusu",
                table: "SosyalMedyaOlaylar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SosyalMedyaOlaylar_Olaylar_OlayId",
                table: "SosyalMedyaOlaylar",
                column: "OlayId",
                principalTable: "Olaylar",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SosyalMedyaOlaylar_Olaylar_OlayId",
                table: "SosyalMedyaOlaylar");

            migrationBuilder.DropColumn(
                name: "EkranGoruntusu",
                table: "SosyalMedyaOlaylar");

            migrationBuilder.AlterColumn<Guid>(
                name: "OlayId",
                table: "SosyalMedyaOlaylar",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SosyalMedyaOlaylar_Olaylar_OlayId",
                table: "SosyalMedyaOlaylar",
                column: "OlayId",
                principalTable: "Olaylar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

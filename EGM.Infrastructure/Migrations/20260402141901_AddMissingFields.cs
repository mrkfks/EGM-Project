using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Logo",
                table: "Organizatorler",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GerceklesenKatilimciSayisi",
                table: "Olaylar",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GerceklesmeSekliId",
                table: "Olaylar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OlayBitisTarihi",
                table: "Olaylar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Olaylar_GerceklesmeSekliId",
                table: "Olaylar",
                column: "GerceklesmeSekliId");

            migrationBuilder.AddForeignKey(
                name: "FK_Olaylar_GerceklesmeSekilleri_GerceklesmeSekliId",
                table: "Olaylar",
                column: "GerceklesmeSekliId",
                principalTable: "GerceklesmeSekilleri",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Olaylar_GerceklesmeSekilleri_GerceklesmeSekliId",
                table: "Olaylar");

            migrationBuilder.DropIndex(
                name: "IX_Olaylar_GerceklesmeSekliId",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "Logo",
                table: "Organizatorler");

            migrationBuilder.DropColumn(
                name: "GerceklesenKatilimciSayisi",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "GerceklesmeSekliId",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "OlayBitisTarihi",
                table: "Olaylar");
        }
    }
}

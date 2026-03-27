using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKonuHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tur",
                table: "Konular",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UstKonuId",
                table: "Konular",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Konular_UstKonuId",
                table: "Konular",
                column: "UstKonuId");

            migrationBuilder.AddForeignKey(
                name: "FK_Konular_Konular_UstKonuId",
                table: "Konular",
                column: "UstKonuId",
                principalTable: "Konular",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Konular_Konular_UstKonuId",
                table: "Konular");

            migrationBuilder.DropIndex(
                name: "IX_Konular_UstKonuId",
                table: "Konular");

            migrationBuilder.DropColumn(
                name: "Tur",
                table: "Konular");

            migrationBuilder.DropColumn(
                name: "UstKonuId",
                table: "Konular");
        }
    }
}

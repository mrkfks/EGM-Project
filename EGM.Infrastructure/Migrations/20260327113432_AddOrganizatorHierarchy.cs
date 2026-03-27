using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizatorHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Aciklama",
                table: "Organizatorler",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tur",
                table: "Organizatorler",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UstKurulusId",
                table: "Organizatorler",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizatorler_UstKurulusId",
                table: "Organizatorler",
                column: "UstKurulusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Organizatorler_Organizatorler_UstKurulusId",
                table: "Organizatorler",
                column: "UstKurulusId",
                principalTable: "Organizatorler",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organizatorler_Organizatorler_UstKurulusId",
                table: "Organizatorler");

            migrationBuilder.DropIndex(
                name: "IX_Organizatorler_UstKurulusId",
                table: "Organizatorler");

            migrationBuilder.DropColumn(
                name: "Aciklama",
                table: "Organizatorler");

            migrationBuilder.DropColumn(
                name: "Tur",
                table: "Organizatorler");

            migrationBuilder.DropColumn(
                name: "UstKurulusId",
                table: "Organizatorler");
        }
    }
}

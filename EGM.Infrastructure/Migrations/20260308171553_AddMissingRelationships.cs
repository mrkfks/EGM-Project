using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ekipler_VIPZiyaretler_VIPZiyaretId",
                table: "Ekipler");

            migrationBuilder.DropForeignKey(
                name: "FK_GuvenlikPlanlari_VIPZiyaretler_VIPZiyaretId",
                table: "GuvenlikPlanlari");

            migrationBuilder.AlterColumn<int>(
                name: "VIPZiyaretId",
                table: "GuvenlikPlanlari",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "VIPZiyaretId",
                table: "Ekipler",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Sicil",
                table: "Users",
                column: "Sicil",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Ekipler_VIPZiyaretler_VIPZiyaretId",
                table: "Ekipler",
                column: "VIPZiyaretId",
                principalTable: "VIPZiyaretler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GuvenlikPlanlari_VIPZiyaretler_VIPZiyaretId",
                table: "GuvenlikPlanlari",
                column: "VIPZiyaretId",
                principalTable: "VIPZiyaretler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ekipler_VIPZiyaretler_VIPZiyaretId",
                table: "Ekipler");

            migrationBuilder.DropForeignKey(
                name: "FK_GuvenlikPlanlari_VIPZiyaretler_VIPZiyaretId",
                table: "GuvenlikPlanlari");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Sicil",
                table: "Users");

            migrationBuilder.AlterColumn<int>(
                name: "VIPZiyaretId",
                table: "GuvenlikPlanlari",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "VIPZiyaretId",
                table: "Ekipler",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Ekipler_VIPZiyaretler_VIPZiyaretId",
                table: "Ekipler",
                column: "VIPZiyaretId",
                principalTable: "VIPZiyaretler",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GuvenlikPlanlari_VIPZiyaretler_VIPZiyaretId",
                table: "GuvenlikPlanlari",
                column: "VIPZiyaretId",
                principalTable: "VIPZiyaretler",
                principalColumn: "Id");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_Role",
                table: "Users",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_Olaylar_CityId",
                table: "Olaylar",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Olaylar_CreatedByUserId",
                table: "Olaylar",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Olaylar_Durum",
                table: "Olaylar",
                column: "Durum");

            migrationBuilder.CreateIndex(
                name: "IX_Olaylar_Tarih",
                table: "Olaylar",
                column: "Tarih");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLoglar_EntityName_EntityId",
                table: "AuditLoglar",
                columns: new[] { "EntityName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLoglar_Timestamp",
                table: "AuditLoglar",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLoglar_UserId",
                table: "AuditLoglar",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Role",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Olaylar_CityId",
                table: "Olaylar");

            migrationBuilder.DropIndex(
                name: "IX_Olaylar_CreatedByUserId",
                table: "Olaylar");

            migrationBuilder.DropIndex(
                name: "IX_Olaylar_Durum",
                table: "Olaylar");

            migrationBuilder.DropIndex(
                name: "IX_Olaylar_Tarih",
                table: "Olaylar");

            migrationBuilder.DropIndex(
                name: "IX_AuditLoglar_EntityName_EntityId",
                table: "AuditLoglar");

            migrationBuilder.DropIndex(
                name: "IX_AuditLoglar_Timestamp",
                table: "AuditLoglar");

            migrationBuilder.DropIndex(
                name: "IX_AuditLoglar_UserId",
                table: "AuditLoglar");
        }
    }
}

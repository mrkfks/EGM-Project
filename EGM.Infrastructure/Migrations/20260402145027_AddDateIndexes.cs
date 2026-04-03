using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDateIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // VIPZiyaretler — aktif ziyaret sorgusu için composite index (BaslangicTarihi, BitisTarihi)
            migrationBuilder.CreateIndex(
                name: "IX_VIPZiyaretler_BaslangicBitis",
                table: "VIPZiyaretler",
                columns: new[] { "BaslangicTarihi", "BitisTarihi" });

            // SandikOlaylar — CreatedAt desc sort için index
            migrationBuilder.CreateIndex(
                name: "IX_SandikOlaylar_CreatedAt",
                table: "SandikOlaylar",
                column: "CreatedAt",
                descending: new[] { true });

            // Olaylar — Tarih filtreleri için composite index
            migrationBuilder.CreateIndex(
                name: "IX_Olaylar_Tarih_IsDeleted",
                table: "Olaylar",
                columns: new[] { "Tarih", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VIPZiyaretler_BaslangicBitis",
                table: "VIPZiyaretler");

            migrationBuilder.DropIndex(
                name: "IX_SandikOlaylar_CreatedAt",
                table: "SandikOlaylar");

            migrationBuilder.DropIndex(
                name: "IX_Olaylar_Tarih_IsDeleted",
                table: "Olaylar");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ekipler");

            migrationBuilder.DropTable(
                name: "GuvenlikPlanlari");

            migrationBuilder.DropTable(
                name: "KatilimciGruplar");

            migrationBuilder.DropTable(
                name: "Oluler");

            migrationBuilder.DropTable(
                name: "SecimSonuclari");

            migrationBuilder.DropTable(
                name: "Sehitler");

            migrationBuilder.DropTable(
                name: "Supheliler");

            migrationBuilder.DropTable(
                name: "Adaylar");

            migrationBuilder.DropTable(
                name: "Partiler");

            migrationBuilder.DropTable(
                name: "SecimKaynaklar");

            migrationBuilder.DropTable(
                name: "OperasyonelFaaliyetler");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Adaylar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AdSoyad = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    PartiAdi = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adaylar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ekipler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VIPZiyaretId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ekipler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ekipler_VIPZiyaretler_VIPZiyaretId",
                        column: x => x.VIPZiyaretId,
                        principalTable: "VIPZiyaretler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuvenlikPlanlari",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VIPZiyaretId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    Ad = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuvenlikPlanlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuvenlikPlanlari_VIPZiyaretler_VIPZiyaretId",
                        column: x => x.VIPZiyaretId,
                        principalTable: "VIPZiyaretler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OperasyonelFaaliyetler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OlayId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperasyonelFaaliyetler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperasyonelFaaliyetler_Olaylar_OlayId",
                        column: x => x.OlayId,
                        principalTable: "Olaylar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Partiler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partiler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecimKaynaklar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    KaynakAdi = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecimKaynaklar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KatilimciGruplar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OperasyonelFaaliyetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    GrupAdi = table.Column<string>(type: "TEXT", nullable: true),
                    GrupKatilimciSayisi = table.Column<int>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KatilimciGruplar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KatilimciGruplar_OperasyonelFaaliyetler_OperasyonelFaaliyetId",
                        column: x => x.OperasyonelFaaliyetId,
                        principalTable: "OperasyonelFaaliyetler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Oluler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OperasyonelFaaliyetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    DogumTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    KatilimciDurumu = table.Column<string>(type: "TEXT", nullable: true),
                    Soyad = table.Column<string>(type: "TEXT", nullable: true),
                    TcKimlikNo = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Oluler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Oluler_OperasyonelFaaliyetler_OperasyonelFaaliyetId",
                        column: x => x.OperasyonelFaaliyetId,
                        principalTable: "OperasyonelFaaliyetler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sehitler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OperasyonelFaaliyetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    DogumTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Gorev = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Soyad = table.Column<string>(type: "TEXT", nullable: true),
                    TcKimlikNo = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sehitler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sehitler_OperasyonelFaaliyetler_OperasyonelFaaliyetId",
                        column: x => x.OperasyonelFaaliyetId,
                        principalTable: "OperasyonelFaaliyetler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Supheliler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OperasyonelFaaliyetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    DogumTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Gozaltinda = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Soyad = table.Column<string>(type: "TEXT", nullable: true),
                    TcKimlikNo = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supheliler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Supheliler_OperasyonelFaaliyetler_OperasyonelFaaliyetId",
                        column: x => x.OperasyonelFaaliyetId,
                        principalTable: "OperasyonelFaaliyetler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SecimSonuclari",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AdayId = table.Column<Guid>(type: "TEXT", nullable: false),
                    KaynakId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PartiId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BolgeId = table.Column<int>(type: "INTEGER", nullable: false),
                    BolgeTipi = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    KaynakOnayDurumu = table.Column<bool>(type: "INTEGER", nullable: false),
                    OyOrani = table.Column<double>(type: "REAL", nullable: false),
                    OySayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    SecimTuru = table.Column<string>(type: "TEXT", nullable: true),
                    Tarih = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecimSonuclari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecimSonuclari_Adaylar_AdayId",
                        column: x => x.AdayId,
                        principalTable: "Adaylar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SecimSonuclari_Partiler_PartiId",
                        column: x => x.PartiId,
                        principalTable: "Partiler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SecimSonuclari_SecimKaynaklar_KaynakId",
                        column: x => x.KaynakId,
                        principalTable: "SecimKaynaklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ekipler_VIPZiyaretId",
                table: "Ekipler",
                column: "VIPZiyaretId");

            migrationBuilder.CreateIndex(
                name: "IX_GuvenlikPlanlari_VIPZiyaretId",
                table: "GuvenlikPlanlari",
                column: "VIPZiyaretId");

            migrationBuilder.CreateIndex(
                name: "IX_KatilimciGruplar_OperasyonelFaaliyetId",
                table: "KatilimciGruplar",
                column: "OperasyonelFaaliyetId");

            migrationBuilder.CreateIndex(
                name: "IX_Oluler_OperasyonelFaaliyetId",
                table: "Oluler",
                column: "OperasyonelFaaliyetId");

            migrationBuilder.CreateIndex(
                name: "IX_OperasyonelFaaliyetler_OlayId",
                table: "OperasyonelFaaliyetler",
                column: "OlayId");

            migrationBuilder.CreateIndex(
                name: "IX_SecimSonuclari_AdayId",
                table: "SecimSonuclari",
                column: "AdayId");

            migrationBuilder.CreateIndex(
                name: "IX_SecimSonuclari_KaynakId",
                table: "SecimSonuclari",
                column: "KaynakId");

            migrationBuilder.CreateIndex(
                name: "IX_SecimSonuclari_PartiId",
                table: "SecimSonuclari",
                column: "PartiId");

            migrationBuilder.CreateIndex(
                name: "IX_Sehitler_OperasyonelFaaliyetId",
                table: "Sehitler",
                column: "OperasyonelFaaliyetId");

            migrationBuilder.CreateIndex(
                name: "IX_Supheliler_OperasyonelFaaliyetId",
                table: "Supheliler",
                column: "OperasyonelFaaliyetId");
        }
    }
}

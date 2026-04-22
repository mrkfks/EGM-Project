using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ParticipantOrganizators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupOlay");

            migrationBuilder.DropTable(
                name: "SandikOlaylar");

            migrationBuilder.DropTable(
                name: "SosyalMedyaOlaylar");

            migrationBuilder.DropTable(
                name: "VIPZiyaretler");

            migrationBuilder.AddColumn<Guid>(
                name: "GroupId",
                table: "Olaylar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OlayKatilimciOrganizator",
                columns: table => new
                {
                    KatilimciOlduguOlaylarId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParticipantOrganizatorsId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OlayKatilimciOrganizator", x => new { x.KatilimciOlduguOlaylarId, x.ParticipantOrganizatorsId });
                    table.ForeignKey(
                        name: "FK_OlayKatilimciOrganizator_Olaylar_KatilimciOlduguOlaylarId",
                        column: x => x.KatilimciOlduguOlaylarId,
                        principalTable: "Olaylar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OlayKatilimciOrganizator_Organizatorler_ParticipantOrganizatorsId",
                        column: x => x.ParticipantOrganizatorsId,
                        principalTable: "Organizatorler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Olaylar_GroupId",
                table: "Olaylar",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_OlayKatilimciOrganizator_ParticipantOrganizatorsId",
                table: "OlayKatilimciOrganizator",
                column: "ParticipantOrganizatorsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Olaylar_Groups_GroupId",
                table: "Olaylar",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Olaylar_Groups_GroupId",
                table: "Olaylar");

            migrationBuilder.DropTable(
                name: "OlayKatilimciOrganizator");

            migrationBuilder.DropIndex(
                name: "IX_Olaylar_GroupId",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Olaylar");

            migrationBuilder.CreateTable(
                name: "GroupOlay",
                columns: table => new
                {
                    EventsId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParticipantGroupsId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupOlay", x => new { x.EventsId, x.ParticipantGroupsId });
                    table.ForeignKey(
                        name: "FK_GroupOlay_Groups_ParticipantGroupsId",
                        column: x => x.ParticipantGroupsId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupOlay_Olaylar_EventsId",
                        column: x => x.EventsId,
                        principalTable: "Olaylar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SandikOlaylar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    GozaltiSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    Il = table.Column<string>(type: "TEXT", nullable: true),
                    Ilce = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    KanitDosyasi = table.Column<string>(type: "TEXT", nullable: true),
                    KatilimciSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    Konu = table.Column<string>(type: "TEXT", nullable: true),
                    Mahalle = table.Column<string>(type: "TEXT", nullable: true),
                    MusahitAdi = table.Column<string>(type: "TEXT", nullable: true),
                    Okul = table.Column<string>(type: "TEXT", nullable: true),
                    OlayKategorisi = table.Column<string>(type: "TEXT", nullable: true),
                    OlaySaati = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    OluSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    SandikNo = table.Column<int>(type: "INTEGER", nullable: false),
                    SehitSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    TakipNo = table.Column<string>(type: "TEXT", nullable: true),
                    Tarih = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SandikOlaylar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SosyalMedyaOlaylar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OlayId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    EkranGoruntusu = table.Column<string>(type: "TEXT", nullable: true),
                    Hassasiyet = table.Column<int>(type: "INTEGER", nullable: false),
                    IcerikOzeti = table.Column<string>(type: "TEXT", nullable: true),
                    Il = table.Column<string>(type: "TEXT", nullable: true),
                    Ilce = table.Column<string>(type: "TEXT", nullable: true),
                    IlgiliKisiKurum = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Konu = table.Column<string>(type: "TEXT", nullable: true),
                    PaylasimLinki = table.Column<string>(type: "TEXT", nullable: true),
                    PaylasimTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Platform = table.Column<string>(type: "TEXT", nullable: true),
                    TakipNo = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SosyalMedyaOlaylar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SosyalMedyaOlaylar_Olaylar_OlayId",
                        column: x => x.OlayId,
                        principalTable: "Olaylar",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VIPZiyaretler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    GozlemNoktalari = table.Column<string>(type: "TEXT", nullable: true),
                    GuvenlikSeviyesi = table.Column<string>(type: "TEXT", nullable: true),
                    Hassasiyet = table.Column<int>(type: "INTEGER", nullable: false),
                    Il = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: true),
                    Longitude = table.Column<double>(type: "REAL", nullable: true),
                    Mekan = table.Column<string>(type: "TEXT", nullable: true),
                    TakipNo = table.Column<string>(type: "TEXT", nullable: true),
                    Unvan = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ZiyaretDurumu = table.Column<int>(type: "INTEGER", nullable: false),
                    ZiyaretEdenAdSoyad = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VIPZiyaretler", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupOlay_ParticipantGroupsId",
                table: "GroupOlay",
                column: "ParticipantGroupsId");

            migrationBuilder.CreateIndex(
                name: "IX_SosyalMedyaOlaylar_OlayId",
                table: "SosyalMedyaOlaylar",
                column: "OlayId");
        }
    }
}

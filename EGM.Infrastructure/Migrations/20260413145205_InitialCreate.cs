using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLoglar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntityName = table.Column<string>(type: "TEXT", nullable: false),
                    EntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Changes = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLoglar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bildirimler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    RiskScore = table.Column<double>(type: "REAL", nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bildirimler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KategoriOrganizatorler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KategoriOrganizatorler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Konular",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    Tur = table.Column<string>(type: "TEXT", nullable: true),
                    UstKonuId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Konular", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Konular_Konular_UstKonuId",
                        column: x => x.UstKonuId,
                        principalTable: "Konular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OlayTurleri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OlayTurleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organizatorler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: true),
                    KurulusTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FaaliyetAlani = table.Column<string>(type: "TEXT", nullable: true),
                    Iletisim = table.Column<string>(type: "TEXT", nullable: true),
                    Tur = table.Column<string>(type: "TEXT", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    Telefon = table.Column<string>(type: "TEXT", nullable: true),
                    Eposta = table.Column<string>(type: "TEXT", nullable: true),
                    SosyalMedyaHesaplari = table.Column<string>(type: "TEXT", nullable: true),
                    SiyasiYonelim = table.Column<string>(type: "TEXT", nullable: true),
                    KutukNumarasi = table.Column<string>(type: "TEXT", nullable: true),
                    Logo = table.Column<string>(type: "TEXT", nullable: true),
                    UstKurulusId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizatorler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Organizatorler_Organizatorler_UstKurulusId",
                        column: x => x.UstKurulusId,
                        principalTable: "Organizatorler",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SandikOlaylar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MusahitAdi = table.Column<string>(type: "TEXT", nullable: true),
                    Il = table.Column<string>(type: "TEXT", nullable: true),
                    Ilce = table.Column<string>(type: "TEXT", nullable: true),
                    Mahalle = table.Column<string>(type: "TEXT", nullable: true),
                    Okul = table.Column<string>(type: "TEXT", nullable: true),
                    Konu = table.Column<string>(type: "TEXT", nullable: true),
                    SandikNo = table.Column<int>(type: "INTEGER", nullable: false),
                    OlayKategorisi = table.Column<string>(type: "TEXT", nullable: true),
                    OlaySaati = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    KanitDosyasi = table.Column<string>(type: "TEXT", nullable: true),
                    Tarih = table.Column<DateTime>(type: "TEXT", nullable: false),
                    KatilimciSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    SehitSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    OluSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    GozaltiSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    TakipNo = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SandikOlaylar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Sicil = table.Column<int>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    GSM = table.Column<string>(type: "TEXT", nullable: false),
                    CityId = table.Column<int>(type: "INTEGER", nullable: true),
                    Birim = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VIPZiyaretler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ZiyaretEdenAdSoyad = table.Column<string>(type: "TEXT", nullable: true),
                    Unvan = table.Column<string>(type: "TEXT", nullable: true),
                    BaslangicTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Il = table.Column<string>(type: "TEXT", nullable: true),
                    Mekan = table.Column<string>(type: "TEXT", nullable: true),
                    Latitude = table.Column<double>(type: "REAL", nullable: true),
                    Longitude = table.Column<double>(type: "REAL", nullable: true),
                    Hassasiyet = table.Column<int>(type: "INTEGER", nullable: false),
                    GuvenlikSeviyesi = table.Column<string>(type: "TEXT", nullable: true),
                    GozlemNoktalari = table.Column<string>(type: "TEXT", nullable: true),
                    ZiyaretDurumu = table.Column<int>(type: "INTEGER", nullable: false),
                    TakipNo = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VIPZiyaretler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GerceklesmeSekilleri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    OlayTuruId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GerceklesmeSekilleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GerceklesmeSekilleri_OlayTurleri_OlayTuruId",
                        column: x => x.OlayTuruId,
                        principalTable: "OlayTurleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KategoriOrganizatorOrganizator",
                columns: table => new
                {
                    KategorilerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrganizatorlerId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KategoriOrganizatorOrganizator", x => new { x.KategorilerId, x.OrganizatorlerId });
                    table.ForeignKey(
                        name: "FK_KategoriOrganizatorOrganizator_KategoriOrganizatorler_KategorilerId",
                        column: x => x.KategorilerId,
                        principalTable: "KategoriOrganizatorler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KategoriOrganizatorOrganizator_Organizatorler_OrganizatorlerId",
                        column: x => x.OrganizatorlerId,
                        principalTable: "Organizatorler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Olaylar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OlayTuru = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    OrganizatorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    KonuId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Tarih = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BaslangicSaati = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    BitisSaati = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    Il = table.Column<string>(type: "TEXT", nullable: true),
                    Ilce = table.Column<string>(type: "TEXT", nullable: true),
                    Mahalle = table.Column<string>(type: "TEXT", nullable: true),
                    Mekan = table.Column<string>(type: "TEXT", nullable: true),
                    Latitude = table.Column<double>(type: "REAL", nullable: true),
                    Longitude = table.Column<double>(type: "REAL", nullable: true),
                    KatilimciSayisi = table.Column<int>(type: "INTEGER", nullable: true),
                    GozaltiSayisi = table.Column<int>(type: "INTEGER", nullable: true),
                    SehitOluSayisi = table.Column<int>(type: "INTEGER", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    EvrakNumarasi = table.Column<string>(type: "TEXT", nullable: true),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    Hassasiyet = table.Column<int>(type: "INTEGER", nullable: false),
                    CityId = table.Column<int>(type: "INTEGER", nullable: true),
                    GercekBaslangicTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GercekBitisTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OlayBitisTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GerceklesenKatilimciSayisi = table.Column<int>(type: "INTEGER", nullable: true),
                    GerceklesmeSekliId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TakipNo = table.Column<string>(type: "TEXT", nullable: true),
                    BaslangicBildirimiGonderildi = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Olaylar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Olaylar_GerceklesmeSekilleri_GerceklesmeSekliId",
                        column: x => x.GerceklesmeSekliId,
                        principalTable: "GerceklesmeSekilleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Olaylar_Konular_KonuId",
                        column: x => x.KonuId,
                        principalTable: "Konular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Olaylar_Organizatorler_OrganizatorId",
                        column: x => x.OrganizatorId,
                        principalTable: "Organizatorler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SosyalMedyaOlaylar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OlayId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Platform = table.Column<string>(type: "TEXT", nullable: true),
                    Konu = table.Column<string>(type: "TEXT", nullable: true),
                    PaylasimLinki = table.Column<string>(type: "TEXT", nullable: true),
                    PaylasimTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IcerikOzeti = table.Column<string>(type: "TEXT", nullable: true),
                    IlgiliKisiKurum = table.Column<string>(type: "TEXT", nullable: true),
                    Il = table.Column<string>(type: "TEXT", nullable: true),
                    Ilce = table.Column<string>(type: "TEXT", nullable: true),
                    EkranGoruntusu = table.Column<string>(type: "TEXT", nullable: true),
                    Hassasiyet = table.Column<int>(type: "INTEGER", nullable: false),
                    TakipNo = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false)
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
                name: "YuruyusRotasi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OlayId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NoktaAdi = table.Column<string>(type: "TEXT", nullable: true),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YuruyusRotasi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YuruyusRotasi_Olaylar_OlayId",
                        column: x => x.OlayId,
                        principalTable: "Olaylar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_GerceklesmeSekilleri_OlayTuruId",
                table: "GerceklesmeSekilleri",
                column: "OlayTuruId");

            migrationBuilder.CreateIndex(
                name: "IX_KategoriOrganizatorOrganizator_OrganizatorlerId",
                table: "KategoriOrganizatorOrganizator",
                column: "OrganizatorlerId");

            migrationBuilder.CreateIndex(
                name: "IX_Konular_UstKonuId",
                table: "Konular",
                column: "UstKonuId");

            migrationBuilder.CreateIndex(
                name: "IX_Olay_Latitude_Longitude",
                table: "Olaylar",
                columns: new[] { "Latitude", "Longitude" });

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
                name: "IX_Olaylar_GerceklesmeSekliId",
                table: "Olaylar",
                column: "GerceklesmeSekliId");

            migrationBuilder.CreateIndex(
                name: "IX_Olaylar_KonuId",
                table: "Olaylar",
                column: "KonuId");

            migrationBuilder.CreateIndex(
                name: "IX_Olaylar_OrganizatorId",
                table: "Olaylar",
                column: "OrganizatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Olaylar_Tarih",
                table: "Olaylar",
                column: "Tarih");

            migrationBuilder.CreateIndex(
                name: "IX_Organizatorler_UstKurulusId",
                table: "Organizatorler",
                column: "UstKurulusId");

            migrationBuilder.CreateIndex(
                name: "IX_SosyalMedyaOlaylar_OlayId",
                table: "SosyalMedyaOlaylar",
                column: "OlayId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role",
                table: "Users",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Sicil",
                table: "Users",
                column: "Sicil",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_YuruyusRotasi_OlayId",
                table: "YuruyusRotasi",
                column: "OlayId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLoglar");

            migrationBuilder.DropTable(
                name: "Bildirimler");

            migrationBuilder.DropTable(
                name: "KategoriOrganizatorOrganizator");

            migrationBuilder.DropTable(
                name: "SandikOlaylar");

            migrationBuilder.DropTable(
                name: "SosyalMedyaOlaylar");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "VIPZiyaretler");

            migrationBuilder.DropTable(
                name: "YuruyusRotasi");

            migrationBuilder.DropTable(
                name: "KategoriOrganizatorler");

            migrationBuilder.DropTable(
                name: "Olaylar");

            migrationBuilder.DropTable(
                name: "GerceklesmeSekilleri");

            migrationBuilder.DropTable(
                name: "Konular");

            migrationBuilder.DropTable(
                name: "Organizatorler");

            migrationBuilder.DropTable(
                name: "OlayTurleri");
        }
    }
}

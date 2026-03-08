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
                name: "Adaylar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdSoyad = table.Column<string>(type: "TEXT", nullable: true),
                    PartiAdi = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adaylar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLoglar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Entity = table.Column<string>(type: "TEXT", nullable: false),
                    EntityId = table.Column<int>(type: "INTEGER", nullable: false),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Changes = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLoglar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KategoriOrganizatorler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KategoriOrganizatorler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Konular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Konular", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organizatorler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", nullable: true),
                    KurulusTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FaaliyetAlani = table.Column<string>(type: "TEXT", nullable: true),
                    Iletisim = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizatorler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Partiler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partiler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecimKaynaklar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KaynakAdi = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecimKaynaklar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Sicil = table.Column<int>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    GSM = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VIPZiyaretler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ZiyaretEdenAdSoyad = table.Column<string>(type: "TEXT", nullable: true),
                    Unvan = table.Column<string>(type: "TEXT", nullable: true),
                    BaslangicTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Il = table.Column<string>(type: "TEXT", nullable: true),
                    Mekan = table.Column<string>(type: "TEXT", nullable: true),
                    Hassasiyet = table.Column<int>(type: "INTEGER", nullable: false),
                    GuvenlikSeviyesi = table.Column<string>(type: "TEXT", nullable: true),
                    GozlemNoktalari = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VIPZiyaretler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KategoriOrganizatorOrganizator",
                columns: table => new
                {
                    KategorilerId = table.Column<int>(type: "INTEGER", nullable: false),
                    OrganizatorlerId = table.Column<int>(type: "INTEGER", nullable: false)
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
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Baslik = table.Column<string>(type: "TEXT", nullable: true),
                    OlayTuru = table.Column<string>(type: "TEXT", nullable: true),
                    OrganizatorId = table.Column<int>(type: "INTEGER", nullable: false),
                    KonuId = table.Column<int>(type: "INTEGER", nullable: false),
                    Tarih = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BaslangicSaati = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    BitisSaati = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    Il = table.Column<string>(type: "TEXT", nullable: true),
                    Ilce = table.Column<string>(type: "TEXT", nullable: true),
                    Mekan = table.Column<string>(type: "TEXT", nullable: true),
                    Latitude = table.Column<double>(type: "REAL", nullable: true),
                    Longitude = table.Column<double>(type: "REAL", nullable: true),
                    KatilimciSayisi = table.Column<int>(type: "INTEGER", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    KaynakKurum = table.Column<string>(type: "TEXT", nullable: true),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    Hassasiyet = table.Column<int>(type: "INTEGER", nullable: false),
                    RiskPuani = table.Column<double>(type: "REAL", nullable: false),
                    GercekBaslangicTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GercekBitisTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Olaylar", x => x.Id);
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
                name: "SecimSonuclari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SecimTuru = table.Column<string>(type: "TEXT", nullable: true),
                    Tarih = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BolgeTipi = table.Column<string>(type: "TEXT", nullable: true),
                    BolgeId = table.Column<int>(type: "INTEGER", nullable: false),
                    AdayId = table.Column<int>(type: "INTEGER", nullable: false),
                    PartiId = table.Column<int>(type: "INTEGER", nullable: false),
                    OySayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    OyOrani = table.Column<double>(type: "REAL", nullable: false),
                    KaynakId = table.Column<int>(type: "INTEGER", nullable: false),
                    KaynakOnayDurumu = table.Column<bool>(type: "INTEGER", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Ekipler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", nullable: true),
                    VIPZiyaretId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ekipler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ekipler_VIPZiyaretler_VIPZiyaretId",
                        column: x => x.VIPZiyaretId,
                        principalTable: "VIPZiyaretler",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GuvenlikPlanlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    VIPZiyaretId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuvenlikPlanlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuvenlikPlanlari_VIPZiyaretler_VIPZiyaretId",
                        column: x => x.VIPZiyaretId,
                        principalTable: "VIPZiyaretler",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OperasyonelFaaliyetler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OlayId = table.Column<int>(type: "INTEGER", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true)
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
                name: "SosyalMedyaOlaylar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OlayId = table.Column<int>(type: "INTEGER", nullable: false),
                    Platform = table.Column<string>(type: "TEXT", nullable: true),
                    PaylasimLinki = table.Column<string>(type: "TEXT", nullable: true),
                    PaylasimTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IcerikOzeti = table.Column<string>(type: "TEXT", nullable: true),
                    IlgiliKisiKurum = table.Column<string>(type: "TEXT", nullable: true),
                    Hassasiyet = table.Column<int>(type: "INTEGER", nullable: false),
                    SosyalSignalSkoru = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SosyalMedyaOlaylar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SosyalMedyaOlaylar_Olaylar_OlayId",
                        column: x => x.OlayId,
                        principalTable: "Olaylar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YuruyusRotasi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OlayId = table.Column<int>(type: "INTEGER", nullable: false),
                    NoktaAdi = table.Column<string>(type: "TEXT", nullable: true),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "KatilimciGruplar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OperasyonelFaaliyetId = table.Column<int>(type: "INTEGER", nullable: false),
                    GrupAdi = table.Column<string>(type: "TEXT", nullable: true),
                    GrupKatilimciSayisi = table.Column<int>(type: "INTEGER", nullable: true)
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
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OperasyonelFaaliyetId = table.Column<int>(type: "INTEGER", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: true),
                    Soyad = table.Column<string>(type: "TEXT", nullable: true),
                    TcKimlikNo = table.Column<string>(type: "TEXT", nullable: true),
                    DogumTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    KatilimciDurumu = table.Column<string>(type: "TEXT", nullable: true)
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
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OperasyonelFaaliyetId = table.Column<int>(type: "INTEGER", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: true),
                    Soyad = table.Column<string>(type: "TEXT", nullable: true),
                    TcKimlikNo = table.Column<string>(type: "TEXT", nullable: true),
                    DogumTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Gorev = table.Column<string>(type: "TEXT", nullable: true)
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
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OperasyonelFaaliyetId = table.Column<int>(type: "INTEGER", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: true),
                    Soyad = table.Column<string>(type: "TEXT", nullable: true),
                    TcKimlikNo = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    DogumTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Gozaltinda = table.Column<bool>(type: "INTEGER", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_Ekipler_VIPZiyaretId",
                table: "Ekipler",
                column: "VIPZiyaretId");

            migrationBuilder.CreateIndex(
                name: "IX_GuvenlikPlanlari_VIPZiyaretId",
                table: "GuvenlikPlanlari",
                column: "VIPZiyaretId");

            migrationBuilder.CreateIndex(
                name: "IX_KategoriOrganizatorOrganizator_OrganizatorlerId",
                table: "KategoriOrganizatorOrganizator",
                column: "OrganizatorlerId");

            migrationBuilder.CreateIndex(
                name: "IX_KatilimciGruplar_OperasyonelFaaliyetId",
                table: "KatilimciGruplar",
                column: "OperasyonelFaaliyetId");

            migrationBuilder.CreateIndex(
                name: "IX_Olaylar_KonuId",
                table: "Olaylar",
                column: "KonuId");

            migrationBuilder.CreateIndex(
                name: "IX_Olaylar_OrganizatorId",
                table: "Olaylar",
                column: "OrganizatorId");

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
                name: "IX_SosyalMedyaOlaylar_OlayId",
                table: "SosyalMedyaOlaylar",
                column: "OlayId");

            migrationBuilder.CreateIndex(
                name: "IX_Supheliler_OperasyonelFaaliyetId",
                table: "Supheliler",
                column: "OperasyonelFaaliyetId");

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
                name: "Ekipler");

            migrationBuilder.DropTable(
                name: "GuvenlikPlanlari");

            migrationBuilder.DropTable(
                name: "KategoriOrganizatorOrganizator");

            migrationBuilder.DropTable(
                name: "KatilimciGruplar");

            migrationBuilder.DropTable(
                name: "Oluler");

            migrationBuilder.DropTable(
                name: "SecimSonuclari");

            migrationBuilder.DropTable(
                name: "Sehitler");

            migrationBuilder.DropTable(
                name: "SosyalMedyaOlaylar");

            migrationBuilder.DropTable(
                name: "Supheliler");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "YuruyusRotasi");

            migrationBuilder.DropTable(
                name: "VIPZiyaretler");

            migrationBuilder.DropTable(
                name: "KategoriOrganizatorler");

            migrationBuilder.DropTable(
                name: "Adaylar");

            migrationBuilder.DropTable(
                name: "Partiler");

            migrationBuilder.DropTable(
                name: "SecimKaynaklar");

            migrationBuilder.DropTable(
                name: "OperasyonelFaaliyetler");

            migrationBuilder.DropTable(
                name: "Olaylar");

            migrationBuilder.DropTable(
                name: "Konular");

            migrationBuilder.DropTable(
                name: "Organizatorler");
        }
    }
}

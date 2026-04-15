using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AutoMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Olaylar_GerceklesmeSekilleri_GerceklesmeSekliId",
                table: "Olaylar");

            migrationBuilder.DropIndex(
                name: "IX_Olay_Latitude_Longitude",
                table: "Olaylar");

            migrationBuilder.DropIndex(
                name: "IX_Olaylar_CreatedByUserId",
                table: "Olaylar");

            migrationBuilder.DropIndex(
                name: "IX_Olaylar_GerceklesmeSekliId",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "BaslangicSaati",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "BitisSaati",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "EvrakNumarasi",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "GercekBaslangicTarihi",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "GercekBitisTarihi",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "GerceklesenKatilimciSayisi",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "GerceklesmeSekliId",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "GozaltiSayisi",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "Hassasiyet",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "Il",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "Ilce",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "KatilimciSayisi",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "Mahalle",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "Mekan",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "OlayBitisTarihi",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "OlayTuru",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "SehitOluSayisi",
                table: "Olaylar");

            migrationBuilder.RenameColumn(
                name: "Tarih",
                table: "Olaylar",
                newName: "TurId");

            migrationBuilder.RenameColumn(
                name: "TakipNo",
                table: "Olaylar",
                newName: "BitisTarihi");

            migrationBuilder.RenameIndex(
                name: "IX_Olaylar_Tarih",
                table: "Olaylar",
                newName: "IX_Olaylar_TurId");

            migrationBuilder.AddColumn<DateTime>(
                name: "BaslangicTarihi",
                table: "Olaylar",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "OlayNo",
                table: "Olaylar",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "PersonelId",
                table: "Olaylar",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SekilId",
                table: "Olaylar",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "EventDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OlayId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Hassasiyet = table.Column<int>(type: "INTEGER", nullable: false),
                    KatilimciSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    SupheliSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    GozaltiSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    SehitSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    OluSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventDetails_Olaylar_OlayId",
                        column: x => x.OlayId,
                        principalTable: "Olaylar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OlayId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Il = table.Column<string>(type: "TEXT", nullable: true),
                    Ilce = table.Column<string>(type: "TEXT", nullable: true),
                    Mahalle = table.Column<string>(type: "TEXT", nullable: true),
                    Mekan = table.Column<string>(type: "TEXT", nullable: true),
                    Latitude = table.Column<double>(type: "REAL", nullable: true),
                    Longitude = table.Column<double>(type: "REAL", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Locations_Olaylar_OlayId",
                        column: x => x.OlayId,
                        principalTable: "Olaylar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OlayId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Platform = table.Column<string>(type: "TEXT", nullable: true),
                    KullaniciAdi = table.Column<string>(type: "TEXT", nullable: true),
                    Link = table.Column<string>(type: "TEXT", nullable: true),
                    GorselPath = table.Column<string>(type: "TEXT", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Resources_Olaylar_OlayId",
                        column: x => x.OlayId,
                        principalTable: "Olaylar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Olaylar_BaslangicTarihi",
                table: "Olaylar",
                column: "BaslangicTarihi");

            migrationBuilder.CreateIndex(
                name: "IX_Olaylar_OlayNo",
                table: "Olaylar",
                column: "OlayNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Olaylar_PersonelId",
                table: "Olaylar",
                column: "PersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_Olaylar_SekilId",
                table: "Olaylar",
                column: "SekilId");

            migrationBuilder.CreateIndex(
                name: "IX_EventDetails_OlayId",
                table: "EventDetails",
                column: "OlayId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupOlay_ParticipantGroupsId",
                table: "GroupOlay",
                column: "ParticipantGroupsId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_OlayId",
                table: "Locations",
                column: "OlayId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_OlayId",
                table: "Resources",
                column: "OlayId");

            migrationBuilder.AddForeignKey(
                name: "FK_Olaylar_GerceklesmeSekilleri_SekilId",
                table: "Olaylar",
                column: "SekilId",
                principalTable: "GerceklesmeSekilleri",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Olaylar_OlayTurleri_TurId",
                table: "Olaylar",
                column: "TurId",
                principalTable: "OlayTurleri",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Olaylar_Users_PersonelId",
                table: "Olaylar",
                column: "PersonelId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Olaylar_GerceklesmeSekilleri_SekilId",
                table: "Olaylar");

            migrationBuilder.DropForeignKey(
                name: "FK_Olaylar_OlayTurleri_TurId",
                table: "Olaylar");

            migrationBuilder.DropForeignKey(
                name: "FK_Olaylar_Users_PersonelId",
                table: "Olaylar");

            migrationBuilder.DropTable(
                name: "EventDetails");

            migrationBuilder.DropTable(
                name: "GroupOlay");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "Resources");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Olaylar_BaslangicTarihi",
                table: "Olaylar");

            migrationBuilder.DropIndex(
                name: "IX_Olaylar_OlayNo",
                table: "Olaylar");

            migrationBuilder.DropIndex(
                name: "IX_Olaylar_PersonelId",
                table: "Olaylar");

            migrationBuilder.DropIndex(
                name: "IX_Olaylar_SekilId",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "BaslangicTarihi",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "OlayNo",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "PersonelId",
                table: "Olaylar");

            migrationBuilder.DropColumn(
                name: "SekilId",
                table: "Olaylar");

            migrationBuilder.RenameColumn(
                name: "TurId",
                table: "Olaylar",
                newName: "Tarih");

            migrationBuilder.RenameColumn(
                name: "BitisTarihi",
                table: "Olaylar",
                newName: "TakipNo");

            migrationBuilder.RenameIndex(
                name: "IX_Olaylar_TurId",
                table: "Olaylar",
                newName: "IX_Olaylar_Tarih");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "BaslangicSaati",
                table: "Olaylar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "BitisSaati",
                table: "Olaylar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvrakNumarasi",
                table: "Olaylar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GercekBaslangicTarihi",
                table: "Olaylar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GercekBitisTarihi",
                table: "Olaylar",
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

            migrationBuilder.AddColumn<int>(
                name: "GozaltiSayisi",
                table: "Olaylar",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Hassasiyet",
                table: "Olaylar",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Il",
                table: "Olaylar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ilce",
                table: "Olaylar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KatilimciSayisi",
                table: "Olaylar",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Olaylar",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Olaylar",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mahalle",
                table: "Olaylar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mekan",
                table: "Olaylar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OlayBitisTarihi",
                table: "Olaylar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OlayTuru",
                table: "Olaylar",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SehitOluSayisi",
                table: "Olaylar",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Olay_Latitude_Longitude",
                table: "Olaylar",
                columns: new[] { "Latitude", "Longitude" });

            migrationBuilder.CreateIndex(
                name: "IX_Olaylar_CreatedByUserId",
                table: "Olaylar",
                column: "CreatedByUserId");

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
    }
}

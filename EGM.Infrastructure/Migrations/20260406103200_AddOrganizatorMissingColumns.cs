using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizatorMissingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Eposta",
                table: "Organizatorler",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KutukNumarasi",
                table: "Organizatorler",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiyasiYonelim",
                table: "Organizatorler",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SosyalMedyaHesaplari",
                table: "Organizatorler",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefon",
                table: "Organizatorler",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Eposta",              table: "Organizatorler");
            migrationBuilder.DropColumn(name: "KutukNumarasi",       table: "Organizatorler");
            migrationBuilder.DropColumn(name: "SiyasiYonelim",       table: "Organizatorler");
            migrationBuilder.DropColumn(name: "SosyalMedyaHesaplari",table: "Organizatorler");
            migrationBuilder.DropColumn(name: "Telefon",             table: "Organizatorler");
        }
    }
}

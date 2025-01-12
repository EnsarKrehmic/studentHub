using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaTriBroj : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Korisnik_Predmet_PredmetId",
                table: "Korisnik");

            migrationBuilder.DropForeignKey(
                name: "FK_Korisnik_Zahtjev_ZahtjevId",
                table: "Korisnik");

            migrationBuilder.DropIndex(
                name: "IX_Korisnik_PredmetId",
                table: "Korisnik");

            migrationBuilder.DropIndex(
                name: "IX_Korisnik_ZahtjevId",
                table: "Korisnik");

            migrationBuilder.DropColumn(
                name: "PredmetId",
                table: "Korisnik");

            migrationBuilder.DropColumn(
                name: "ZahtjevId",
                table: "Korisnik");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PredmetId",
                table: "Korisnik",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ZahtjevId",
                table: "Korisnik",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Korisnik_PredmetId",
                table: "Korisnik",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_Korisnik_ZahtjevId",
                table: "Korisnik",
                column: "ZahtjevId");

            migrationBuilder.AddForeignKey(
                name: "FK_Korisnik_Predmet_PredmetId",
                table: "Korisnik",
                column: "PredmetId",
                principalTable: "Predmet",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Korisnik_Zahtjev_ZahtjevId",
                table: "Korisnik",
                column: "ZahtjevId",
                principalTable: "Zahtjev",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

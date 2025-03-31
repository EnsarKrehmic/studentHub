using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaTri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Komentar_Korisnik_StudentId",
                table: "Komentar");

            migrationBuilder.DropForeignKey(
                name: "FK_Ocjena_NastavnaAktivnost_NastavnaAktivnostId",
                table: "Ocjena");

            migrationBuilder.AddForeignKey(
                name: "FK_Komentar_Korisnik_StudentId",
                table: "Komentar",
                column: "StudentId",
                principalTable: "Korisnik",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ocjena_NastavnaAktivnost_NastavnaAktivnostId",
                table: "Ocjena",
                column: "NastavnaAktivnostId",
                principalTable: "NastavnaAktivnost",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Komentar_Korisnik_StudentId",
                table: "Komentar");

            migrationBuilder.DropForeignKey(
                name: "FK_Ocjena_NastavnaAktivnost_NastavnaAktivnostId",
                table: "Ocjena");

            migrationBuilder.AddForeignKey(
                name: "FK_Komentar_Korisnik_StudentId",
                table: "Komentar",
                column: "StudentId",
                principalTable: "Korisnik",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ocjena_NastavnaAktivnost_NastavnaAktivnostId",
                table: "Ocjena",
                column: "NastavnaAktivnostId",
                principalTable: "NastavnaAktivnost",
                principalColumn: "Id");
        }
    }
}

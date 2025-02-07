using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaBrČetiri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Predmet_Korisnik_ProfesorId",
                table: "Predmet");

            migrationBuilder.AddForeignKey(
                name: "FK_Predmet_Korisnik_ProfesorId",
                table: "Predmet",
                column: "ProfesorId",
                principalTable: "Korisnik",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Predmet_Korisnik_ProfesorId",
                table: "Predmet");

            migrationBuilder.AddForeignKey(
                name: "FK_Predmet_Korisnik_ProfesorId",
                table: "Predmet",
                column: "ProfesorId",
                principalTable: "Korisnik",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}

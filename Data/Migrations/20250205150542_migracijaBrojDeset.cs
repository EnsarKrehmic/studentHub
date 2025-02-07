using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaBrojDeset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Korisnik_Predmet_PredmetId",
                table: "Korisnik");

            migrationBuilder.DropForeignKey(
                name: "FK_Korisnik_StudijskiProgram_StudentskaSluzba_StudijskiProgramId",
                table: "Korisnik");

            migrationBuilder.DropIndex(
                name: "IX_Korisnik_PredmetId",
                table: "Korisnik");

            migrationBuilder.DropIndex(
                name: "IX_Korisnik_StudentskaSluzba_StudijskiProgramId",
                table: "Korisnik");

            migrationBuilder.DropColumn(
                name: "PredmetId",
                table: "Korisnik");

            migrationBuilder.DropColumn(
                name: "StudentskaSluzba_StudijskiProgramId",
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
                name: "StudentskaSluzba_StudijskiProgramId",
                table: "Korisnik",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Korisnik_PredmetId",
                table: "Korisnik",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_Korisnik_StudentskaSluzba_StudijskiProgramId",
                table: "Korisnik",
                column: "StudentskaSluzba_StudijskiProgramId");

            migrationBuilder.AddForeignKey(
                name: "FK_Korisnik_Predmet_PredmetId",
                table: "Korisnik",
                column: "PredmetId",
                principalTable: "Predmet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Korisnik_StudijskiProgram_StudentskaSluzba_StudijskiProgramId",
                table: "Korisnik",
                column: "StudentskaSluzba_StudijskiProgramId",
                principalTable: "StudijskiProgram",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

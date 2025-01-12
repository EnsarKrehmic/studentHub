using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaBrojOsam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Korisnik_StudijskiProgram_StudijskiProgramId",
                table: "Korisnik");

            migrationBuilder.DropIndex(
                name: "IX_Korisnik_StudijskiProgramId",
                table: "Korisnik");

            migrationBuilder.DropColumn(
                name: "StudijskiProgramId",
                table: "Korisnik");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "StudijskiProgramId",
                table: "Korisnik",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Korisnik_StudijskiProgramId",
                table: "Korisnik",
                column: "StudijskiProgramId");

            migrationBuilder.AddForeignKey(
                name: "FK_Korisnik_StudijskiProgram_StudijskiProgramId",
                table: "Korisnik",
                column: "StudijskiProgramId",
                principalTable: "StudijskiProgram",
                principalColumn: "Id");
        }
    }
}

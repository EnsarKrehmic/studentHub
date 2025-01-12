using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaBrojŠest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "podaciUplata",
                table: "Korisnik");

            migrationBuilder.AddColumn<long>(
                name: "StudentskaSluzbaId",
                table: "Ispit",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ispit_StudentskaSluzbaId",
                table: "Ispit",
                column: "StudentskaSluzbaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ispit_Korisnik_StudentskaSluzbaId",
                table: "Ispit",
                column: "StudentskaSluzbaId",
                principalTable: "Korisnik",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ispit_Korisnik_StudentskaSluzbaId",
                table: "Ispit");

            migrationBuilder.DropIndex(
                name: "IX_Ispit_StudentskaSluzbaId",
                table: "Ispit");

            migrationBuilder.DropColumn(
                name: "StudentskaSluzbaId",
                table: "Ispit");

            migrationBuilder.AddColumn<string>(
                name: "podaciUplata",
                table: "Korisnik",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

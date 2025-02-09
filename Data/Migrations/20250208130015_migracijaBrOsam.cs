using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaBrOsam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Uvjerenje_Korisnik_StudentId",
                table: "Uvjerenje");

            migrationBuilder.DropForeignKey(
                name: "FK_Uvjerenje_Korisnik_StudentskaSluzbaId",
                table: "Uvjerenje");

            migrationBuilder.RenameColumn(
                name: "Vrsta",
                table: "Uvjerenje",
                newName: "VrstaUvjerenja");

            migrationBuilder.AlterColumn<string>(
                name: "Namjena",
                table: "Uvjerenje",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddForeignKey(
                name: "FK_Uvjerenje_Korisnik_StudentId",
                table: "Uvjerenje",
                column: "StudentId",
                principalTable: "Korisnik",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Uvjerenje_Korisnik_StudentskaSluzbaId",
                table: "Uvjerenje",
                column: "StudentskaSluzbaId",
                principalTable: "Korisnik",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Uvjerenje_Korisnik_StudentId",
                table: "Uvjerenje");

            migrationBuilder.DropForeignKey(
                name: "FK_Uvjerenje_Korisnik_StudentskaSluzbaId",
                table: "Uvjerenje");

            migrationBuilder.RenameColumn(
                name: "VrstaUvjerenja",
                table: "Uvjerenje",
                newName: "Vrsta");

            migrationBuilder.AlterColumn<string>(
                name: "Namjena",
                table: "Uvjerenje",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Uvjerenje_Korisnik_StudentId",
                table: "Uvjerenje",
                column: "StudentId",
                principalTable: "Korisnik",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Uvjerenje_Korisnik_StudentskaSluzbaId",
                table: "Uvjerenje",
                column: "StudentskaSluzbaId",
                principalTable: "Korisnik",
                principalColumn: "Id");
        }
    }
}

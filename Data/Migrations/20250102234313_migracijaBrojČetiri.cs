using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaBrojČetiri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Korisnik_StudijskiProgram_StudijskiProgramId",
                table: "Korisnik");

            migrationBuilder.AlterColumn<long>(
                name: "StudijskiProgramId",
                table: "Korisnik",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_Korisnik_StudijskiProgram_StudijskiProgramId",
                table: "Korisnik",
                column: "StudijskiProgramId",
                principalTable: "StudijskiProgram",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Korisnik_StudijskiProgram_StudijskiProgramId",
                table: "Korisnik");

            migrationBuilder.AlterColumn<long>(
                name: "StudijskiProgramId",
                table: "Korisnik",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Korisnik_StudijskiProgram_StudijskiProgramId",
                table: "Korisnik",
                column: "StudijskiProgramId",
                principalTable: "StudijskiProgram",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

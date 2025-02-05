using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaBrojosam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Obavjestenje_StudijskiProgram_StudijskiProgramId",
                table: "Obavjestenje");

            migrationBuilder.DropIndex(
                name: "IX_Obavjestenje_StudijskiProgramId",
                table: "Obavjestenje");

            migrationBuilder.DropColumn(
                name: "StudijskiProgramId",
                table: "Obavjestenje");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "StudijskiProgramId",
                table: "Obavjestenje",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Obavjestenje_StudijskiProgramId",
                table: "Obavjestenje",
                column: "StudijskiProgramId");

            migrationBuilder.AddForeignKey(
                name: "FK_Obavjestenje_StudijskiProgram_StudijskiProgramId",
                table: "Obavjestenje",
                column: "StudijskiProgramId",
                principalTable: "StudijskiProgram",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

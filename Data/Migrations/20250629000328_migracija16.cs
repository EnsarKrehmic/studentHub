using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracija16 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TerminNastave_Raspored_RasporedId",
                table: "TerminNastave");

            migrationBuilder.AddForeignKey(
                name: "FK_TerminNastave_Raspored_RasporedId",
                table: "TerminNastave",
                column: "RasporedId",
                principalTable: "Raspored",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TerminNastave_Raspored_RasporedId",
                table: "TerminNastave");

            migrationBuilder.AddForeignKey(
                name: "FK_TerminNastave_Raspored_RasporedId",
                table: "TerminNastave",
                column: "RasporedId",
                principalTable: "Raspored",
                principalColumn: "Id");
        }
    }
}

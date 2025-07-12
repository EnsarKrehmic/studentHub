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
                name: "FK_Predmet_StudijskiProgram_StudijskiProgramId",
                table: "Predmet");

            migrationBuilder.DropForeignKey(
                name: "FK_TerminNastave_Raspored_RasporedId",
                table: "TerminNastave");

            migrationBuilder.AlterColumn<long>(
                name: "StudijskiProgramId",
                table: "Predmet",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Semestar",
                table: "Predmet",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GodinaStudija",
                table: "Predmet",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SatiPredavanja",
                table: "Predmet",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SatiVjezbi",
                table: "Predmet",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Predmet_StudijskiProgram_StudijskiProgramId",
                table: "Predmet",
                column: "StudijskiProgramId",
                principalTable: "StudijskiProgram",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TerminNastave_Raspored_RasporedId",
                table: "TerminNastave",
                column: "RasporedId",
                principalTable: "Raspored",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Predmet_StudijskiProgram_StudijskiProgramId",
                table: "Predmet");

            migrationBuilder.DropForeignKey(
                name: "FK_TerminNastave_Raspored_RasporedId",
                table: "TerminNastave");

            migrationBuilder.DropColumn(
                name: "GodinaStudija",
                table: "Predmet");

            migrationBuilder.DropColumn(
                name: "SatiPredavanja",
                table: "Predmet");

            migrationBuilder.DropColumn(
                name: "SatiVjezbi",
                table: "Predmet");

            migrationBuilder.AlterColumn<long>(
                name: "StudijskiProgramId",
                table: "Predmet",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "Semestar",
                table: "Predmet",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Predmet_StudijskiProgram_StudijskiProgramId",
                table: "Predmet",
                column: "StudijskiProgramId",
                principalTable: "StudijskiProgram",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TerminNastave_Raspored_RasporedId",
                table: "TerminNastave",
                column: "RasporedId",
                principalTable: "Raspored",
                principalColumn: "Id");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaBrojŠest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PredhodnoObrazovanje",
                table: "Korisnik",
                newName: "PrethodnoObrazovanje");

            migrationBuilder.AlterColumn<int>(
                name: "Semestar",
                table: "Predmet",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<long>(
                name: "NastavniPlanId",
                table: "Predmet",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "StudijskiProgramId",
                table: "Predmet",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Predmet_StudijskiProgramId",
                table: "Predmet",
                column: "StudijskiProgramId");

            migrationBuilder.AddForeignKey(
                name: "FK_Predmet_StudijskiProgram_StudijskiProgramId",
                table: "Predmet",
                column: "StudijskiProgramId",
                principalTable: "StudijskiProgram",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Predmet_StudijskiProgram_StudijskiProgramId",
                table: "Predmet");

            migrationBuilder.DropIndex(
                name: "IX_Predmet_StudijskiProgramId",
                table: "Predmet");

            migrationBuilder.DropColumn(
                name: "StudijskiProgramId",
                table: "Predmet");

            migrationBuilder.RenameColumn(
                name: "PrethodnoObrazovanje",
                table: "Korisnik",
                newName: "PredhodnoObrazovanje");

            migrationBuilder.AlterColumn<int>(
                name: "Semestar",
                table: "Predmet",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "NastavniPlanId",
                table: "Predmet",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}

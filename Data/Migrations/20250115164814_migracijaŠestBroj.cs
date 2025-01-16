using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaŠestBroj : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "NastavniPlanId",
                table: "Korisnik",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PredmetId",
                table: "Korisnik",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Semestar",
                table: "Korisnik",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "StudijskiProgramId",
                table: "Korisnik",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Korisnik_NastavniPlanId",
                table: "Korisnik",
                column: "NastavniPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Korisnik_PredmetId",
                table: "Korisnik",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_Korisnik_StudijskiProgramId",
                table: "Korisnik",
                column: "StudijskiProgramId");

            migrationBuilder.AddForeignKey(
                name: "FK_Korisnik_NastavniPlan_NastavniPlanId",
                table: "Korisnik",
                column: "NastavniPlanId",
                principalTable: "NastavniPlan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Korisnik_Predmet_PredmetId",
                table: "Korisnik",
                column: "PredmetId",
                principalTable: "Predmet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Korisnik_StudijskiProgram_StudijskiProgramId",
                table: "Korisnik",
                column: "StudijskiProgramId",
                principalTable: "StudijskiProgram",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Korisnik_NastavniPlan_NastavniPlanId",
                table: "Korisnik");

            migrationBuilder.DropForeignKey(
                name: "FK_Korisnik_Predmet_PredmetId",
                table: "Korisnik");

            migrationBuilder.DropForeignKey(
                name: "FK_Korisnik_StudijskiProgram_StudijskiProgramId",
                table: "Korisnik");

            migrationBuilder.DropIndex(
                name: "IX_Korisnik_NastavniPlanId",
                table: "Korisnik");

            migrationBuilder.DropIndex(
                name: "IX_Korisnik_PredmetId",
                table: "Korisnik");

            migrationBuilder.DropIndex(
                name: "IX_Korisnik_StudijskiProgramId",
                table: "Korisnik");

            migrationBuilder.DropColumn(
                name: "NastavniPlanId",
                table: "Korisnik");

            migrationBuilder.DropColumn(
                name: "PredmetId",
                table: "Korisnik");

            migrationBuilder.DropColumn(
                name: "Semestar",
                table: "Korisnik");

            migrationBuilder.DropColumn(
                name: "StudijskiProgramId",
                table: "Korisnik");
        }
    }
}

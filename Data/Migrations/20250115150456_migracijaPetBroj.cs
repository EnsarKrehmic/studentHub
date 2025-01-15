using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaPetBroj : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Predmet_Korisnik_AsistentId",
                table: "Predmet");

            migrationBuilder.DropForeignKey(
                name: "FK_Predmet_Korisnik_ProfesorId",
                table: "Predmet");

            migrationBuilder.DropForeignKey(
                name: "FK_Predmet_NastavniPlan_NastavniPlanId",
                table: "Predmet");

            migrationBuilder.AddForeignKey(
                name: "FK_Predmet_Korisnik_AsistentId",
                table: "Predmet",
                column: "AsistentId",
                principalTable: "Korisnik",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Predmet_Korisnik_ProfesorId",
                table: "Predmet",
                column: "ProfesorId",
                principalTable: "Korisnik",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Predmet_NastavniPlan_NastavniPlanId",
                table: "Predmet",
                column: "NastavniPlanId",
                principalTable: "NastavniPlan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Predmet_Korisnik_AsistentId",
                table: "Predmet");

            migrationBuilder.DropForeignKey(
                name: "FK_Predmet_Korisnik_ProfesorId",
                table: "Predmet");

            migrationBuilder.DropForeignKey(
                name: "FK_Predmet_NastavniPlan_NastavniPlanId",
                table: "Predmet");

            migrationBuilder.AddForeignKey(
                name: "FK_Predmet_Korisnik_AsistentId",
                table: "Predmet",
                column: "AsistentId",
                principalTable: "Korisnik",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Predmet_Korisnik_ProfesorId",
                table: "Predmet",
                column: "ProfesorId",
                principalTable: "Korisnik",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Predmet_NastavniPlan_NastavniPlanId",
                table: "Predmet",
                column: "NastavniPlanId",
                principalTable: "NastavniPlan",
                principalColumn: "Id");
        }
    }
}

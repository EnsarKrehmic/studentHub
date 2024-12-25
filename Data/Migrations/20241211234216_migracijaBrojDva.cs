using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaBrojDva : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Predmet_NastavnoOsoblje_SifraProfesora",
                table: "Predmet");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_Korisnik_KorisnikId",
                table: "Student");

            migrationBuilder.RenameColumn(
                name: "KorisnikId",
                table: "Student",
                newName: "JMBG");

            migrationBuilder.RenameIndex(
                name: "IX_Student_KorisnikId",
                table: "Student",
                newName: "IX_Student_JMBG");

            migrationBuilder.RenameColumn(
                name: "SifraProfesora",
                table: "Predmet",
                newName: "NastavnoOsobljeId");

            migrationBuilder.RenameIndex(
                name: "IX_Predmet_SifraProfesora",
                table: "Predmet",
                newName: "IX_Predmet_NastavnoOsobljeId");

            migrationBuilder.AddColumn<long>(
                name: "JMBG",
                table: "StudentskaSluzba",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "NastavniPlanId",
                table: "Predmet",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "JMBG",
                table: "NastavnoOsoblje",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_StudentskaSluzba_JMBG",
                table: "StudentskaSluzba",
                column: "JMBG");

            migrationBuilder.CreateIndex(
                name: "IX_Predmet_NastavniPlanId",
                table: "Predmet",
                column: "NastavniPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_NastavnoOsoblje_JMBG",
                table: "NastavnoOsoblje",
                column: "JMBG");

            migrationBuilder.AddForeignKey(
                name: "FK_NastavnoOsoblje_Korisnik_JMBG",
                table: "NastavnoOsoblje",
                column: "JMBG",
                principalTable: "Korisnik",
                principalColumn: "JMBG",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Predmet_NastavniPlan_NastavniPlanId",
                table: "Predmet",
                column: "NastavniPlanId",
                principalTable: "NastavniPlan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Predmet_NastavnoOsoblje_NastavnoOsobljeId",
                table: "Predmet",
                column: "NastavnoOsobljeId",
                principalTable: "NastavnoOsoblje",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_Korisnik_JMBG",
                table: "Student",
                column: "JMBG",
                principalTable: "Korisnik",
                principalColumn: "JMBG",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentskaSluzba_Korisnik_JMBG",
                table: "StudentskaSluzba",
                column: "JMBG",
                principalTable: "Korisnik",
                principalColumn: "JMBG",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NastavnoOsoblje_Korisnik_JMBG",
                table: "NastavnoOsoblje");

            migrationBuilder.DropForeignKey(
                name: "FK_Predmet_NastavniPlan_NastavniPlanId",
                table: "Predmet");

            migrationBuilder.DropForeignKey(
                name: "FK_Predmet_NastavnoOsoblje_NastavnoOsobljeId",
                table: "Predmet");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_Korisnik_JMBG",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentskaSluzba_Korisnik_JMBG",
                table: "StudentskaSluzba");

            migrationBuilder.DropIndex(
                name: "IX_StudentskaSluzba_JMBG",
                table: "StudentskaSluzba");

            migrationBuilder.DropIndex(
                name: "IX_Predmet_NastavniPlanId",
                table: "Predmet");

            migrationBuilder.DropIndex(
                name: "IX_NastavnoOsoblje_JMBG",
                table: "NastavnoOsoblje");

            migrationBuilder.DropColumn(
                name: "JMBG",
                table: "StudentskaSluzba");

            migrationBuilder.DropColumn(
                name: "NastavniPlanId",
                table: "Predmet");

            migrationBuilder.DropColumn(
                name: "JMBG",
                table: "NastavnoOsoblje");

            migrationBuilder.RenameColumn(
                name: "JMBG",
                table: "Student",
                newName: "KorisnikId");

            migrationBuilder.RenameIndex(
                name: "IX_Student_JMBG",
                table: "Student",
                newName: "IX_Student_KorisnikId");

            migrationBuilder.RenameColumn(
                name: "NastavnoOsobljeId",
                table: "Predmet",
                newName: "SifraProfesora");

            migrationBuilder.RenameIndex(
                name: "IX_Predmet_NastavnoOsobljeId",
                table: "Predmet",
                newName: "IX_Predmet_SifraProfesora");

            migrationBuilder.AddForeignKey(
                name: "FK_Predmet_NastavnoOsoblje_SifraProfesora",
                table: "Predmet",
                column: "SifraProfesora",
                principalTable: "NastavnoOsoblje",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_Korisnik_KorisnikId",
                table: "Student",
                column: "KorisnikId",
                principalTable: "Korisnik",
                principalColumn: "JMBG",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

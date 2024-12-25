using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaBrojČetiri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dokument_Korisnik_KorisnikId",
                table: "Dokument");

            migrationBuilder.DropForeignKey(
                name: "FK_Dokument_Zahtjev_ZahtjevId",
                table: "Dokument");

            migrationBuilder.DropForeignKey(
                name: "FK_Ocjena_Student_StudentId",
                table: "Ocjena");

            migrationBuilder.DropForeignKey(
                name: "FK_Uvjerenje_Zahtjev_ZahtjevId",
                table: "Uvjerenje");

            migrationBuilder.DropForeignKey(
                name: "FK_Zahtjev_Student_StudentId",
                table: "Zahtjev");

            migrationBuilder.DropForeignKey(
                name: "FK_Zahtjev_StudentskaSluzba_StudentskaSluzbaId",
                table: "Zahtjev");

            migrationBuilder.DropIndex(
                name: "IX_Zahtjev_StudentId",
                table: "Zahtjev");

            migrationBuilder.DropIndex(
                name: "IX_Dokument_KorisnikId",
                table: "Dokument");

            migrationBuilder.DropIndex(
                name: "IX_Dokument_ZahtjevId",
                table: "Dokument");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Zahtjev");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "Zahtjev");

            migrationBuilder.DropColumn(
                name: "KorisnikId",
                table: "Dokument");

            migrationBuilder.DropColumn(
                name: "ZahtjevId",
                table: "Dokument");

            migrationBuilder.RenameColumn(
                name: "TipZahtjeva",
                table: "Zahtjev",
                newName: "tipZahtjeva");

            migrationBuilder.RenameColumn(
                name: "DatumRjesavanja",
                table: "Zahtjev",
                newName: "datumRjesavanja");

            migrationBuilder.RenameColumn(
                name: "DatumPodnosenja",
                table: "Zahtjev",
                newName: "datumPodnosenja");

            migrationBuilder.RenameColumn(
                name: "StudentskaSluzbaId",
                table: "Zahtjev",
                newName: "brojIndeksa");

            migrationBuilder.RenameIndex(
                name: "IX_Zahtjev_StudentskaSluzbaId",
                table: "Zahtjev",
                newName: "IX_Zahtjev_brojIndeksa");

            migrationBuilder.RenameColumn(
                name: "DatumIzdavanja",
                table: "Uvjerenje",
                newName: "datumIzdavanja");

            migrationBuilder.RenameColumn(
                name: "ZahtjevId",
                table: "Uvjerenje",
                newName: "brojIndeksa");

            migrationBuilder.RenameIndex(
                name: "IX_Uvjerenje_ZahtjevId",
                table: "Uvjerenje",
                newName: "IX_Uvjerenje_brojIndeksa");

            migrationBuilder.RenameColumn(
                name: "TrajanjeUGodinama",
                table: "StudijskiProgram",
                newName: "trajanjeUGodinama");

            migrationBuilder.RenameColumn(
                name: "AkademskaGodina",
                table: "StudentNaPredmetu",
                newName: "akademskaGodina");

            migrationBuilder.RenameColumn(
                name: "StudijskiProgram",
                table: "Student",
                newName: "studijskiProgram");

            migrationBuilder.RenameColumn(
                name: "PredhodnoObrazovanje",
                table: "Student",
                newName: "predhodnoObrazovanje");

            migrationBuilder.RenameColumn(
                name: "PodaciUplata",
                table: "Student",
                newName: "podaciUplata");

            migrationBuilder.RenameColumn(
                name: "GodinaStudija",
                table: "Student",
                newName: "godinaStudija");

            migrationBuilder.RenameColumn(
                name: "BrojIndeksa",
                table: "Student",
                newName: "brojIndeksa");

            migrationBuilder.RenameColumn(
                name: "DatumPrijave",
                table: "Prijava",
                newName: "datumPrijave");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Ocjena",
                newName: "brojIndeksa");

            migrationBuilder.RenameIndex(
                name: "IX_Ocjena_StudentId",
                table: "Ocjena",
                newName: "IX_Ocjena_brojIndeksa");

            migrationBuilder.RenameColumn(
                name: "DatumObjave",
                table: "Obavjestenje",
                newName: "datumObjave");

            migrationBuilder.RenameColumn(
                name: "GodinaStudija",
                table: "NastavniPlan",
                newName: "godinaStudija");

            migrationBuilder.AlterColumn<int>(
                name: "tipZahtjeva",
                table: "Zahtjev",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "statusZahtjeva",
                table: "Zahtjev",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "StudentskaSluzbaId",
                table: "Uvjerenje",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "PredmetId",
                table: "StudentskaSluzba",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ZahtjevId",
                table: "StudentskaSluzba",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "StudentskaSluzbaId",
                table: "Dokument",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "brojIndeksa",
                table: "Dokument",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Uvjerenje_StudentskaSluzbaId",
                table: "Uvjerenje",
                column: "StudentskaSluzbaId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentskaSluzba_PredmetId",
                table: "StudentskaSluzba",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentskaSluzba_ZahtjevId",
                table: "StudentskaSluzba",
                column: "ZahtjevId");

            migrationBuilder.CreateIndex(
                name: "IX_Dokument_brojIndeksa",
                table: "Dokument",
                column: "brojIndeksa");

            migrationBuilder.CreateIndex(
                name: "IX_Dokument_StudentskaSluzbaId",
                table: "Dokument",
                column: "StudentskaSluzbaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Dokument_Student_brojIndeksa",
                table: "Dokument",
                column: "brojIndeksa",
                principalTable: "Student",
                principalColumn: "brojIndeksa",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Dokument_StudentskaSluzba_StudentskaSluzbaId",
                table: "Dokument",
                column: "StudentskaSluzbaId",
                principalTable: "StudentskaSluzba",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ocjena_Student_brojIndeksa",
                table: "Ocjena",
                column: "brojIndeksa",
                principalTable: "Student",
                principalColumn: "brojIndeksa",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentskaSluzba_Predmet_PredmetId",
                table: "StudentskaSluzba",
                column: "PredmetId",
                principalTable: "Predmet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentskaSluzba_Zahtjev_ZahtjevId",
                table: "StudentskaSluzba",
                column: "ZahtjevId",
                principalTable: "Zahtjev",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Uvjerenje_Student_brojIndeksa",
                table: "Uvjerenje",
                column: "brojIndeksa",
                principalTable: "Student",
                principalColumn: "brojIndeksa",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Uvjerenje_StudentskaSluzba_StudentskaSluzbaId",
                table: "Uvjerenje",
                column: "StudentskaSluzbaId",
                principalTable: "StudentskaSluzba",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Zahtjev_Student_brojIndeksa",
                table: "Zahtjev",
                column: "brojIndeksa",
                principalTable: "Student",
                principalColumn: "brojIndeksa",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dokument_Student_brojIndeksa",
                table: "Dokument");

            migrationBuilder.DropForeignKey(
                name: "FK_Dokument_StudentskaSluzba_StudentskaSluzbaId",
                table: "Dokument");

            migrationBuilder.DropForeignKey(
                name: "FK_Ocjena_Student_brojIndeksa",
                table: "Ocjena");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentskaSluzba_Predmet_PredmetId",
                table: "StudentskaSluzba");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentskaSluzba_Zahtjev_ZahtjevId",
                table: "StudentskaSluzba");

            migrationBuilder.DropForeignKey(
                name: "FK_Uvjerenje_Student_brojIndeksa",
                table: "Uvjerenje");

            migrationBuilder.DropForeignKey(
                name: "FK_Uvjerenje_StudentskaSluzba_StudentskaSluzbaId",
                table: "Uvjerenje");

            migrationBuilder.DropForeignKey(
                name: "FK_Zahtjev_Student_brojIndeksa",
                table: "Zahtjev");

            migrationBuilder.DropIndex(
                name: "IX_Uvjerenje_StudentskaSluzbaId",
                table: "Uvjerenje");

            migrationBuilder.DropIndex(
                name: "IX_StudentskaSluzba_PredmetId",
                table: "StudentskaSluzba");

            migrationBuilder.DropIndex(
                name: "IX_StudentskaSluzba_ZahtjevId",
                table: "StudentskaSluzba");

            migrationBuilder.DropIndex(
                name: "IX_Dokument_brojIndeksa",
                table: "Dokument");

            migrationBuilder.DropIndex(
                name: "IX_Dokument_StudentskaSluzbaId",
                table: "Dokument");

            migrationBuilder.DropColumn(
                name: "statusZahtjeva",
                table: "Zahtjev");

            migrationBuilder.DropColumn(
                name: "StudentskaSluzbaId",
                table: "Uvjerenje");

            migrationBuilder.DropColumn(
                name: "PredmetId",
                table: "StudentskaSluzba");

            migrationBuilder.DropColumn(
                name: "ZahtjevId",
                table: "StudentskaSluzba");

            migrationBuilder.DropColumn(
                name: "StudentskaSluzbaId",
                table: "Dokument");

            migrationBuilder.DropColumn(
                name: "brojIndeksa",
                table: "Dokument");

            migrationBuilder.RenameColumn(
                name: "tipZahtjeva",
                table: "Zahtjev",
                newName: "TipZahtjeva");

            migrationBuilder.RenameColumn(
                name: "datumRjesavanja",
                table: "Zahtjev",
                newName: "DatumRjesavanja");

            migrationBuilder.RenameColumn(
                name: "datumPodnosenja",
                table: "Zahtjev",
                newName: "DatumPodnosenja");

            migrationBuilder.RenameColumn(
                name: "brojIndeksa",
                table: "Zahtjev",
                newName: "StudentskaSluzbaId");

            migrationBuilder.RenameIndex(
                name: "IX_Zahtjev_brojIndeksa",
                table: "Zahtjev",
                newName: "IX_Zahtjev_StudentskaSluzbaId");

            migrationBuilder.RenameColumn(
                name: "datumIzdavanja",
                table: "Uvjerenje",
                newName: "DatumIzdavanja");

            migrationBuilder.RenameColumn(
                name: "brojIndeksa",
                table: "Uvjerenje",
                newName: "ZahtjevId");

            migrationBuilder.RenameIndex(
                name: "IX_Uvjerenje_brojIndeksa",
                table: "Uvjerenje",
                newName: "IX_Uvjerenje_ZahtjevId");

            migrationBuilder.RenameColumn(
                name: "trajanjeUGodinama",
                table: "StudijskiProgram",
                newName: "TrajanjeUGodinama");

            migrationBuilder.RenameColumn(
                name: "akademskaGodina",
                table: "StudentNaPredmetu",
                newName: "AkademskaGodina");

            migrationBuilder.RenameColumn(
                name: "studijskiProgram",
                table: "Student",
                newName: "StudijskiProgram");

            migrationBuilder.RenameColumn(
                name: "predhodnoObrazovanje",
                table: "Student",
                newName: "PredhodnoObrazovanje");

            migrationBuilder.RenameColumn(
                name: "podaciUplata",
                table: "Student",
                newName: "PodaciUplata");

            migrationBuilder.RenameColumn(
                name: "godinaStudija",
                table: "Student",
                newName: "GodinaStudija");

            migrationBuilder.RenameColumn(
                name: "brojIndeksa",
                table: "Student",
                newName: "BrojIndeksa");

            migrationBuilder.RenameColumn(
                name: "datumPrijave",
                table: "Prijava",
                newName: "DatumPrijave");

            migrationBuilder.RenameColumn(
                name: "brojIndeksa",
                table: "Ocjena",
                newName: "StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Ocjena_brojIndeksa",
                table: "Ocjena",
                newName: "IX_Ocjena_StudentId");

            migrationBuilder.RenameColumn(
                name: "datumObjave",
                table: "Obavjestenje",
                newName: "DatumObjave");

            migrationBuilder.RenameColumn(
                name: "godinaStudija",
                table: "NastavniPlan",
                newName: "GodinaStudija");

            migrationBuilder.AlterColumn<string>(
                name: "TipZahtjeva",
                table: "Zahtjev",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Zahtjev",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "StudentId",
                table: "Zahtjev",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "KorisnikId",
                table: "Dokument",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ZahtjevId",
                table: "Dokument",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Zahtjev_StudentId",
                table: "Zahtjev",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Dokument_KorisnikId",
                table: "Dokument",
                column: "KorisnikId");

            migrationBuilder.CreateIndex(
                name: "IX_Dokument_ZahtjevId",
                table: "Dokument",
                column: "ZahtjevId");

            migrationBuilder.AddForeignKey(
                name: "FK_Dokument_Korisnik_KorisnikId",
                table: "Dokument",
                column: "KorisnikId",
                principalTable: "Korisnik",
                principalColumn: "JMBG");

            migrationBuilder.AddForeignKey(
                name: "FK_Dokument_Zahtjev_ZahtjevId",
                table: "Dokument",
                column: "ZahtjevId",
                principalTable: "Zahtjev",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ocjena_Student_StudentId",
                table: "Ocjena",
                column: "StudentId",
                principalTable: "Student",
                principalColumn: "BrojIndeksa",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Uvjerenje_Zahtjev_ZahtjevId",
                table: "Uvjerenje",
                column: "ZahtjevId",
                principalTable: "Zahtjev",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Zahtjev_Student_StudentId",
                table: "Zahtjev",
                column: "StudentId",
                principalTable: "Student",
                principalColumn: "BrojIndeksa",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Zahtjev_StudentskaSluzba_StudentskaSluzbaId",
                table: "Zahtjev",
                column: "StudentskaSluzbaId",
                principalTable: "StudentskaSluzba",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

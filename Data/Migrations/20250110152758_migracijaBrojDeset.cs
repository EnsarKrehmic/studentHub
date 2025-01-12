using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaBrojDeset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "tipZahtjeva",
                table: "Zahtjev",
                newName: "TipZahtjeva");

            migrationBuilder.RenameColumn(
                name: "statusZahtjeva",
                table: "Zahtjev",
                newName: "StatusZahtjeva");

            migrationBuilder.RenameColumn(
                name: "datumRjesavanja",
                table: "Zahtjev",
                newName: "DatumRjesavanja");

            migrationBuilder.RenameColumn(
                name: "datumPodnosenja",
                table: "Zahtjev",
                newName: "DatumPodnosenja");

            migrationBuilder.RenameColumn(
                name: "datumIzdavanja",
                table: "Uvjerenje",
                newName: "DatumIzdavanja");

            migrationBuilder.RenameColumn(
                name: "trajanjeUGodinama",
                table: "StudijskiProgram",
                newName: "TrajanjeUGodinama");

            migrationBuilder.RenameColumn(
                name: "akademskaGodina",
                table: "StudentNaPredmetu",
                newName: "AkademskaGodina");

            migrationBuilder.RenameColumn(
                name: "datumObjave",
                table: "Obavjestenje",
                newName: "DatumObjave");

            migrationBuilder.RenameColumn(
                name: "godinaStudija",
                table: "NastavniPlan",
                newName: "GodinaStudija");

            migrationBuilder.RenameColumn(
                name: "predhodnoObrazovanje",
                table: "Korisnik",
                newName: "PredhodnoObrazovanje");

            migrationBuilder.RenameColumn(
                name: "godinaStudija",
                table: "Korisnik",
                newName: "GodinaStudija");

            migrationBuilder.RenameColumn(
                name: "brojIndeksa",
                table: "Korisnik",
                newName: "BrojIndeksa");

            migrationBuilder.RenameColumn(
                name: "Profesor_Titula",
                table: "Korisnik",
                newName: "ProfesorTitula");

            migrationBuilder.RenameColumn(
                name: "Asistent_Titula",
                table: "Korisnik",
                newName: "AsistentTitula");

            migrationBuilder.RenameColumn(
                name: "datumOdrzavanja",
                table: "Ispit",
                newName: "DatumOdrzavanja");

            migrationBuilder.RenameColumn(
                name: "datumObjave",
                table: "Ispit",
                newName: "DatumObjave");

            migrationBuilder.RenameColumn(
                name: "brojBodova",
                table: "Ispit",
                newName: "BrojBodova");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TipZahtjeva",
                table: "Zahtjev",
                newName: "tipZahtjeva");

            migrationBuilder.RenameColumn(
                name: "StatusZahtjeva",
                table: "Zahtjev",
                newName: "statusZahtjeva");

            migrationBuilder.RenameColumn(
                name: "DatumRjesavanja",
                table: "Zahtjev",
                newName: "datumRjesavanja");

            migrationBuilder.RenameColumn(
                name: "DatumPodnosenja",
                table: "Zahtjev",
                newName: "datumPodnosenja");

            migrationBuilder.RenameColumn(
                name: "DatumIzdavanja",
                table: "Uvjerenje",
                newName: "datumIzdavanja");

            migrationBuilder.RenameColumn(
                name: "TrajanjeUGodinama",
                table: "StudijskiProgram",
                newName: "trajanjeUGodinama");

            migrationBuilder.RenameColumn(
                name: "AkademskaGodina",
                table: "StudentNaPredmetu",
                newName: "akademskaGodina");

            migrationBuilder.RenameColumn(
                name: "DatumObjave",
                table: "Obavjestenje",
                newName: "datumObjave");

            migrationBuilder.RenameColumn(
                name: "GodinaStudija",
                table: "NastavniPlan",
                newName: "godinaStudija");

            migrationBuilder.RenameColumn(
                name: "PredhodnoObrazovanje",
                table: "Korisnik",
                newName: "predhodnoObrazovanje");

            migrationBuilder.RenameColumn(
                name: "GodinaStudija",
                table: "Korisnik",
                newName: "godinaStudija");

            migrationBuilder.RenameColumn(
                name: "BrojIndeksa",
                table: "Korisnik",
                newName: "brojIndeksa");

            migrationBuilder.RenameColumn(
                name: "ProfesorTitula",
                table: "Korisnik",
                newName: "Profesor_Titula");

            migrationBuilder.RenameColumn(
                name: "AsistentTitula",
                table: "Korisnik",
                newName: "Asistent_Titula");

            migrationBuilder.RenameColumn(
                name: "DatumOdrzavanja",
                table: "Ispit",
                newName: "datumOdrzavanja");

            migrationBuilder.RenameColumn(
                name: "DatumObjave",
                table: "Ispit",
                newName: "datumObjave");

            migrationBuilder.RenameColumn(
                name: "BrojBodova",
                table: "Ispit",
                newName: "brojBodova");
        }
    }
}

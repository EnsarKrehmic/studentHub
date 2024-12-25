using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaBrojPet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Asistent_Korisnik_JMBG",
                table: "Asistent");

            migrationBuilder.DropForeignKey(
                name: "FK_Dokument_Student_brojIndeksa",
                table: "Dokument");

            migrationBuilder.DropForeignKey(
                name: "FK_Dokument_StudentskaSluzba_StudentskaSluzbaId",
                table: "Dokument");

            migrationBuilder.DropForeignKey(
                name: "FK_Ispit_Asistent_AsistentId",
                table: "Ispit");

            migrationBuilder.DropForeignKey(
                name: "FK_Ispit_Profesor_ProfesorId",
                table: "Ispit");

            migrationBuilder.DropForeignKey(
                name: "FK_Obavjestenje_Asistent_AsistentId",
                table: "Obavjestenje");

            migrationBuilder.DropForeignKey(
                name: "FK_Obavjestenje_Profesor_ProfesorId",
                table: "Obavjestenje");

            migrationBuilder.DropForeignKey(
                name: "FK_Obavjestenje_StudentskaSluzba_StudentskaSluzbaId",
                table: "Obavjestenje");

            migrationBuilder.DropForeignKey(
                name: "FK_Ocjena_Profesor_ProfesorId",
                table: "Ocjena");

            migrationBuilder.DropForeignKey(
                name: "FK_Ocjena_Student_brojIndeksa",
                table: "Ocjena");

            migrationBuilder.DropForeignKey(
                name: "FK_Predmet_Asistent_AsistentId",
                table: "Predmet");

            migrationBuilder.DropForeignKey(
                name: "FK_Predmet_Profesor_ProfesorId",
                table: "Predmet");

            migrationBuilder.DropForeignKey(
                name: "FK_PredmetAsistent_Asistent_AsistentId",
                table: "PredmetAsistent");

            migrationBuilder.DropForeignKey(
                name: "FK_PredmetProfesor_Profesor_ProfesorId",
                table: "PredmetProfesor");

            migrationBuilder.DropForeignKey(
                name: "FK_Prijava_Student_StudentId",
                table: "Prijava");

            migrationBuilder.DropForeignKey(
                name: "FK_Profesor_Korisnik_JMBG",
                table: "Profesor");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_Korisnik_JMBG",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentNaPredmetu_Student_StudentId",
                table: "StudentNaPredmetu");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentskaSluzba_Predmet_PredmetId",
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

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentskaSluzba",
                table: "StudentskaSluzba");

            migrationBuilder.DropIndex(
                name: "IX_StudentskaSluzba_JMBG",
                table: "StudentskaSluzba");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Student",
                table: "Student");

            migrationBuilder.DropIndex(
                name: "IX_Student_JMBG",
                table: "Student");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Profesor",
                table: "Profesor");

            migrationBuilder.DropIndex(
                name: "IX_Profesor_JMBG",
                table: "Profesor");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Asistent",
                table: "Asistent");

            migrationBuilder.DropIndex(
                name: "IX_Asistent_JMBG",
                table: "Asistent");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "StudentskaSluzba");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "StudentskaSluzba");

            migrationBuilder.DropColumn(
                name: "Ime",
                table: "StudentskaSluzba");

            migrationBuilder.DropColumn(
                name: "Lozinka",
                table: "StudentskaSluzba");

            migrationBuilder.DropColumn(
                name: "Prezime",
                table: "StudentskaSluzba");

            migrationBuilder.DropColumn(
                name: "brojIndeksa",
                table: "Student");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Student");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Profesor");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Profesor");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Asistent");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Asistent");

            migrationBuilder.AlterColumn<string>(
                name: "Namjena",
                table: "Uvjerenje",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<long>(
                name: "ZahtjevId",
                table: "StudentskaSluzba",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "PredmetId",
                table: "StudentskaSluzba",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "akademskaGodina",
                table: "StudentNaPredmetu",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "studijskiProgram",
                table: "Student",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "predhodnoObrazovanje",
                table: "Student",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<long>(
                name: "StudentskaSluzbaId",
                table: "Obavjestenje",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "ProfesorId",
                table: "Obavjestenje",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "AsistentId",
                table: "Obavjestenje",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "KorisnikId",
                table: "Obavjestenje",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<string>(
                name: "godinaStudija",
                table: "NastavniPlan",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Lokacija",
                table: "Ispit",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Putanja",
                table: "Dokument",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Naziv",
                table: "Dokument",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentskaSluzba",
                table: "StudentskaSluzba",
                column: "JMBG");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Student",
                table: "Student",
                column: "JMBG");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Profesor",
                table: "Profesor",
                column: "JMBG");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Asistent",
                table: "Asistent",
                column: "JMBG");

            migrationBuilder.CreateIndex(
                name: "IX_Obavjestenje_KorisnikId",
                table: "Obavjestenje",
                column: "KorisnikId");

            migrationBuilder.AddForeignKey(
                name: "FK_Asistent_Korisnik_JMBG",
                table: "Asistent",
                column: "JMBG",
                principalTable: "Korisnik",
                principalColumn: "JMBG",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Dokument_Student_brojIndeksa",
                table: "Dokument",
                column: "brojIndeksa",
                principalTable: "Student",
                principalColumn: "JMBG",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Dokument_StudentskaSluzba_StudentskaSluzbaId",
                table: "Dokument",
                column: "StudentskaSluzbaId",
                principalTable: "StudentskaSluzba",
                principalColumn: "JMBG",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ispit_Asistent_AsistentId",
                table: "Ispit",
                column: "AsistentId",
                principalTable: "Asistent",
                principalColumn: "JMBG",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ispit_Profesor_ProfesorId",
                table: "Ispit",
                column: "ProfesorId",
                principalTable: "Profesor",
                principalColumn: "JMBG",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Obavjestenje_Asistent_AsistentId",
                table: "Obavjestenje",
                column: "AsistentId",
                principalTable: "Asistent",
                principalColumn: "JMBG");

            migrationBuilder.AddForeignKey(
                name: "FK_Obavjestenje_Korisnik_KorisnikId",
                table: "Obavjestenje",
                column: "KorisnikId",
                principalTable: "Korisnik",
                principalColumn: "JMBG",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Obavjestenje_Profesor_ProfesorId",
                table: "Obavjestenje",
                column: "ProfesorId",
                principalTable: "Profesor",
                principalColumn: "JMBG");

            migrationBuilder.AddForeignKey(
                name: "FK_Obavjestenje_StudentskaSluzba_StudentskaSluzbaId",
                table: "Obavjestenje",
                column: "StudentskaSluzbaId",
                principalTable: "StudentskaSluzba",
                principalColumn: "JMBG");

            migrationBuilder.AddForeignKey(
                name: "FK_Ocjena_Profesor_ProfesorId",
                table: "Ocjena",
                column: "ProfesorId",
                principalTable: "Profesor",
                principalColumn: "JMBG",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ocjena_Student_brojIndeksa",
                table: "Ocjena",
                column: "brojIndeksa",
                principalTable: "Student",
                principalColumn: "JMBG",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Predmet_Asistent_AsistentId",
                table: "Predmet",
                column: "AsistentId",
                principalTable: "Asistent",
                principalColumn: "JMBG");

            migrationBuilder.AddForeignKey(
                name: "FK_Predmet_Profesor_ProfesorId",
                table: "Predmet",
                column: "ProfesorId",
                principalTable: "Profesor",
                principalColumn: "JMBG");

            migrationBuilder.AddForeignKey(
                name: "FK_PredmetAsistent_Asistent_AsistentId",
                table: "PredmetAsistent",
                column: "AsistentId",
                principalTable: "Asistent",
                principalColumn: "JMBG",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PredmetProfesor_Profesor_ProfesorId",
                table: "PredmetProfesor",
                column: "ProfesorId",
                principalTable: "Profesor",
                principalColumn: "JMBG",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Prijava_Student_StudentId",
                table: "Prijava",
                column: "StudentId",
                principalTable: "Student",
                principalColumn: "JMBG",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Profesor_Korisnik_JMBG",
                table: "Profesor",
                column: "JMBG",
                principalTable: "Korisnik",
                principalColumn: "JMBG",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_Korisnik_JMBG",
                table: "Student",
                column: "JMBG",
                principalTable: "Korisnik",
                principalColumn: "JMBG",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentNaPredmetu_Student_StudentId",
                table: "StudentNaPredmetu",
                column: "StudentId",
                principalTable: "Student",
                principalColumn: "JMBG",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentskaSluzba_Predmet_PredmetId",
                table: "StudentskaSluzba",
                column: "PredmetId",
                principalTable: "Predmet",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Uvjerenje_Student_brojIndeksa",
                table: "Uvjerenje",
                column: "brojIndeksa",
                principalTable: "Student",
                principalColumn: "JMBG",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Uvjerenje_StudentskaSluzba_StudentskaSluzbaId",
                table: "Uvjerenje",
                column: "StudentskaSluzbaId",
                principalTable: "StudentskaSluzba",
                principalColumn: "JMBG",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Zahtjev_Student_brojIndeksa",
                table: "Zahtjev",
                column: "brojIndeksa",
                principalTable: "Student",
                principalColumn: "JMBG",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Asistent_Korisnik_JMBG",
                table: "Asistent");

            migrationBuilder.DropForeignKey(
                name: "FK_Dokument_Student_brojIndeksa",
                table: "Dokument");

            migrationBuilder.DropForeignKey(
                name: "FK_Dokument_StudentskaSluzba_StudentskaSluzbaId",
                table: "Dokument");

            migrationBuilder.DropForeignKey(
                name: "FK_Ispit_Asistent_AsistentId",
                table: "Ispit");

            migrationBuilder.DropForeignKey(
                name: "FK_Ispit_Profesor_ProfesorId",
                table: "Ispit");

            migrationBuilder.DropForeignKey(
                name: "FK_Obavjestenje_Asistent_AsistentId",
                table: "Obavjestenje");

            migrationBuilder.DropForeignKey(
                name: "FK_Obavjestenje_Korisnik_KorisnikId",
                table: "Obavjestenje");

            migrationBuilder.DropForeignKey(
                name: "FK_Obavjestenje_Profesor_ProfesorId",
                table: "Obavjestenje");

            migrationBuilder.DropForeignKey(
                name: "FK_Obavjestenje_StudentskaSluzba_StudentskaSluzbaId",
                table: "Obavjestenje");

            migrationBuilder.DropForeignKey(
                name: "FK_Ocjena_Profesor_ProfesorId",
                table: "Ocjena");

            migrationBuilder.DropForeignKey(
                name: "FK_Ocjena_Student_brojIndeksa",
                table: "Ocjena");

            migrationBuilder.DropForeignKey(
                name: "FK_Predmet_Asistent_AsistentId",
                table: "Predmet");

            migrationBuilder.DropForeignKey(
                name: "FK_Predmet_Profesor_ProfesorId",
                table: "Predmet");

            migrationBuilder.DropForeignKey(
                name: "FK_PredmetAsistent_Asistent_AsistentId",
                table: "PredmetAsistent");

            migrationBuilder.DropForeignKey(
                name: "FK_PredmetProfesor_Profesor_ProfesorId",
                table: "PredmetProfesor");

            migrationBuilder.DropForeignKey(
                name: "FK_Prijava_Student_StudentId",
                table: "Prijava");

            migrationBuilder.DropForeignKey(
                name: "FK_Profesor_Korisnik_JMBG",
                table: "Profesor");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_Korisnik_JMBG",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentNaPredmetu_Student_StudentId",
                table: "StudentNaPredmetu");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentskaSluzba_Predmet_PredmetId",
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

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentskaSluzba",
                table: "StudentskaSluzba");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Student",
                table: "Student");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Profesor",
                table: "Profesor");

            migrationBuilder.DropIndex(
                name: "IX_Obavjestenje_KorisnikId",
                table: "Obavjestenje");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Asistent",
                table: "Asistent");

            migrationBuilder.DropColumn(
                name: "KorisnikId",
                table: "Obavjestenje");

            migrationBuilder.AlterColumn<string>(
                name: "Namjena",
                table: "Uvjerenje",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<long>(
                name: "ZahtjevId",
                table: "StudentskaSluzba",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "PredmetId",
                table: "StudentskaSluzba",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "StudentskaSluzba",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "StudentskaSluzba",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Ime",
                table: "StudentskaSluzba",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Lozinka",
                table: "StudentskaSluzba",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Prezime",
                table: "StudentskaSluzba",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "akademskaGodina",
                table: "StudentNaPredmetu",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "studijskiProgram",
                table: "Student",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "predhodnoObrazovanje",
                table: "Student",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<long>(
                name: "brojIndeksa",
                table: "Student",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Student",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "Profesor",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Profesor",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<long>(
                name: "StudentskaSluzbaId",
                table: "Obavjestenje",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ProfesorId",
                table: "Obavjestenje",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "AsistentId",
                table: "Obavjestenje",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "godinaStudija",
                table: "NastavniPlan",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Lokacija",
                table: "Ispit",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Putanja",
                table: "Dokument",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Naziv",
                table: "Dokument",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "Asistent",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Asistent",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentskaSluzba",
                table: "StudentskaSluzba",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Student",
                table: "Student",
                column: "brojIndeksa");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Profesor",
                table: "Profesor",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Asistent",
                table: "Asistent",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_StudentskaSluzba_JMBG",
                table: "StudentskaSluzba",
                column: "JMBG");

            migrationBuilder.CreateIndex(
                name: "IX_Student_JMBG",
                table: "Student",
                column: "JMBG");

            migrationBuilder.CreateIndex(
                name: "IX_Profesor_JMBG",
                table: "Profesor",
                column: "JMBG");

            migrationBuilder.CreateIndex(
                name: "IX_Asistent_JMBG",
                table: "Asistent",
                column: "JMBG");

            migrationBuilder.AddForeignKey(
                name: "FK_Asistent_Korisnik_JMBG",
                table: "Asistent",
                column: "JMBG",
                principalTable: "Korisnik",
                principalColumn: "JMBG",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_Ispit_Asistent_AsistentId",
                table: "Ispit",
                column: "AsistentId",
                principalTable: "Asistent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ispit_Profesor_ProfesorId",
                table: "Ispit",
                column: "ProfesorId",
                principalTable: "Profesor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Obavjestenje_Asistent_AsistentId",
                table: "Obavjestenje",
                column: "AsistentId",
                principalTable: "Asistent",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Obavjestenje_Profesor_ProfesorId",
                table: "Obavjestenje",
                column: "ProfesorId",
                principalTable: "Profesor",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Obavjestenje_StudentskaSluzba_StudentskaSluzbaId",
                table: "Obavjestenje",
                column: "StudentskaSluzbaId",
                principalTable: "StudentskaSluzba",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ocjena_Profesor_ProfesorId",
                table: "Ocjena",
                column: "ProfesorId",
                principalTable: "Profesor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ocjena_Student_brojIndeksa",
                table: "Ocjena",
                column: "brojIndeksa",
                principalTable: "Student",
                principalColumn: "brojIndeksa",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Predmet_Asistent_AsistentId",
                table: "Predmet",
                column: "AsistentId",
                principalTable: "Asistent",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Predmet_Profesor_ProfesorId",
                table: "Predmet",
                column: "ProfesorId",
                principalTable: "Profesor",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PredmetAsistent_Asistent_AsistentId",
                table: "PredmetAsistent",
                column: "AsistentId",
                principalTable: "Asistent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PredmetProfesor_Profesor_ProfesorId",
                table: "PredmetProfesor",
                column: "ProfesorId",
                principalTable: "Profesor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Prijava_Student_StudentId",
                table: "Prijava",
                column: "StudentId",
                principalTable: "Student",
                principalColumn: "brojIndeksa",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Profesor_Korisnik_JMBG",
                table: "Profesor",
                column: "JMBG",
                principalTable: "Korisnik",
                principalColumn: "JMBG",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_Korisnik_JMBG",
                table: "Student",
                column: "JMBG",
                principalTable: "Korisnik",
                principalColumn: "JMBG",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentNaPredmetu_Student_StudentId",
                table: "StudentNaPredmetu",
                column: "StudentId",
                principalTable: "Student",
                principalColumn: "brojIndeksa",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentskaSluzba_Predmet_PredmetId",
                table: "StudentskaSluzba",
                column: "PredmetId",
                principalTable: "Predmet",
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
    }
}

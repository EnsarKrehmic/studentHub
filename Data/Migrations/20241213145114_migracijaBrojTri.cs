using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaBrojTri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dokument_StudentskaSluzba_StudentskaSluzbaId",
                table: "Dokument");

            migrationBuilder.DropForeignKey(
                name: "FK_Ispit_NastavnoOsoblje_NastavnoOsobljeId",
                table: "Ispit");

            migrationBuilder.DropForeignKey(
                name: "FK_Obavjestenje_NastavnoOsoblje_NastavnoOsobljeId",
                table: "Obavjestenje");

            migrationBuilder.DropForeignKey(
                name: "FK_Obavjestenje_StudentskaSluzba_StudentskaSluzbaId",
                table: "Obavjestenje");

            migrationBuilder.DropForeignKey(
                name: "FK_Ocjena_NastavnoOsoblje_NastavnoOsobljeId",
                table: "Ocjena");

            migrationBuilder.DropForeignKey(
                name: "FK_Predmet_NastavniPlan_NastavniPlanId",
                table: "Predmet");

            migrationBuilder.DropForeignKey(
                name: "FK_Predmet_NastavnoOsoblje_NastavnoOsobljeId",
                table: "Predmet");

            migrationBuilder.DropForeignKey(
                name: "FK_Prijava_Predmet_PredmetId",
                table: "Prijava");

            migrationBuilder.DropForeignKey(
                name: "FK_Uvjerenje_Student_StudentId",
                table: "Uvjerenje");

            migrationBuilder.DropForeignKey(
                name: "FK_Uvjerenje_StudentskaSluzba_StudentskaSluzbaId",
                table: "Uvjerenje");

            migrationBuilder.DropForeignKey(
                name: "FK_Zahtjev_Student_StudentId",
                table: "Zahtjev");

            migrationBuilder.DropForeignKey(
                name: "FK_Zahtjev_StudentskaSluzba_StudentskaSluzbaId",
                table: "Zahtjev");

            migrationBuilder.DropTable(
                name: "NastavnoOsoblje");

            migrationBuilder.DropIndex(
                name: "IX_Uvjerenje_StudentId",
                table: "Uvjerenje");

            migrationBuilder.DropIndex(
                name: "IX_Uvjerenje_StudentskaSluzbaId",
                table: "Uvjerenje");

            migrationBuilder.DropIndex(
                name: "IX_Prijava_PredmetId",
                table: "Prijava");

            migrationBuilder.DropIndex(
                name: "IX_Dokument_StudentskaSluzbaId",
                table: "Dokument");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "Uvjerenje");

            migrationBuilder.DropColumn(
                name: "StudentskaSluzbaId",
                table: "Uvjerenje");

            migrationBuilder.DropColumn(
                name: "PredmetId",
                table: "Prijava");

            migrationBuilder.DropColumn(
                name: "StudentskaSluzbaId",
                table: "Dokument");

            migrationBuilder.RenameColumn(
                name: "NastavnoOsobljeId",
                table: "Predmet",
                newName: "ProfesorId");

            migrationBuilder.RenameIndex(
                name: "IX_Predmet_NastavnoOsobljeId",
                table: "Predmet",
                newName: "IX_Predmet_ProfesorId");

            migrationBuilder.RenameColumn(
                name: "NastavnoOsobljeId",
                table: "Ocjena",
                newName: "ProfesorId");

            migrationBuilder.RenameIndex(
                name: "IX_Ocjena_NastavnoOsobljeId",
                table: "Ocjena",
                newName: "IX_Ocjena_ProfesorId");

            migrationBuilder.RenameColumn(
                name: "NastavnoOsobljeId",
                table: "Obavjestenje",
                newName: "ProfesorId");

            migrationBuilder.RenameIndex(
                name: "IX_Obavjestenje_NastavnoOsobljeId",
                table: "Obavjestenje",
                newName: "IX_Obavjestenje_ProfesorId");

            migrationBuilder.RenameColumn(
                name: "NastavnoOsobljeId",
                table: "Ispit",
                newName: "ProfesorId");

            migrationBuilder.RenameIndex(
                name: "IX_Ispit_NastavnoOsobljeId",
                table: "Ispit",
                newName: "IX_Ispit_ProfesorId");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Student",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "AsistentId",
                table: "Predmet",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AsistentId",
                table: "Obavjestenje",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "AsistentId",
                table: "Ispit",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "Asistent",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Titula = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    JMBG = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asistent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asistent_Korisnik_JMBG",
                        column: x => x.JMBG,
                        principalTable: "Korisnik",
                        principalColumn: "JMBG",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Profesor",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Titula = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    JMBG = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profesor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Profesor_Korisnik_JMBG",
                        column: x => x.JMBG,
                        principalTable: "Korisnik",
                        principalColumn: "JMBG",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PredmetAsistent",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PredmetId = table.Column<long>(type: "bigint", nullable: false),
                    AsistentId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PredmetAsistent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PredmetAsistent_Asistent_AsistentId",
                        column: x => x.AsistentId,
                        principalTable: "Asistent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PredmetAsistent_Predmet_PredmetId",
                        column: x => x.PredmetId,
                        principalTable: "Predmet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PredmetProfesor",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PredmetId = table.Column<long>(type: "bigint", nullable: false),
                    ProfesorId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PredmetProfesor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PredmetProfesor_Predmet_PredmetId",
                        column: x => x.PredmetId,
                        principalTable: "Predmet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PredmetProfesor_Profesor_ProfesorId",
                        column: x => x.ProfesorId,
                        principalTable: "Profesor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Predmet_AsistentId",
                table: "Predmet",
                column: "AsistentId");

            migrationBuilder.CreateIndex(
                name: "IX_Obavjestenje_AsistentId",
                table: "Obavjestenje",
                column: "AsistentId");

            migrationBuilder.CreateIndex(
                name: "IX_Ispit_AsistentId",
                table: "Ispit",
                column: "AsistentId");

            migrationBuilder.CreateIndex(
                name: "IX_Asistent_JMBG",
                table: "Asistent",
                column: "JMBG");

            migrationBuilder.CreateIndex(
                name: "IX_PredmetAsistent_AsistentId",
                table: "PredmetAsistent",
                column: "AsistentId");

            migrationBuilder.CreateIndex(
                name: "IX_PredmetAsistent_PredmetId",
                table: "PredmetAsistent",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_PredmetProfesor_PredmetId",
                table: "PredmetProfesor",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_PredmetProfesor_ProfesorId",
                table: "PredmetProfesor",
                column: "ProfesorId");

            migrationBuilder.CreateIndex(
                name: "IX_Profesor_JMBG",
                table: "Profesor",
                column: "JMBG");

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
                name: "FK_Predmet_Asistent_AsistentId",
                table: "Predmet",
                column: "AsistentId",
                principalTable: "Asistent",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Predmet_NastavniPlan_NastavniPlanId",
                table: "Predmet",
                column: "NastavniPlanId",
                principalTable: "NastavniPlan",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Predmet_Profesor_ProfesorId",
                table: "Predmet",
                column: "ProfesorId",
                principalTable: "Profesor",
                principalColumn: "Id");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "FK_Predmet_Asistent_AsistentId",
                table: "Predmet");

            migrationBuilder.DropForeignKey(
                name: "FK_Predmet_NastavniPlan_NastavniPlanId",
                table: "Predmet");

            migrationBuilder.DropForeignKey(
                name: "FK_Predmet_Profesor_ProfesorId",
                table: "Predmet");

            migrationBuilder.DropForeignKey(
                name: "FK_Zahtjev_Student_StudentId",
                table: "Zahtjev");

            migrationBuilder.DropForeignKey(
                name: "FK_Zahtjev_StudentskaSluzba_StudentskaSluzbaId",
                table: "Zahtjev");

            migrationBuilder.DropTable(
                name: "PredmetAsistent");

            migrationBuilder.DropTable(
                name: "PredmetProfesor");

            migrationBuilder.DropTable(
                name: "Asistent");

            migrationBuilder.DropTable(
                name: "Profesor");

            migrationBuilder.DropIndex(
                name: "IX_Predmet_AsistentId",
                table: "Predmet");

            migrationBuilder.DropIndex(
                name: "IX_Obavjestenje_AsistentId",
                table: "Obavjestenje");

            migrationBuilder.DropIndex(
                name: "IX_Ispit_AsistentId",
                table: "Ispit");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Student");

            migrationBuilder.DropColumn(
                name: "AsistentId",
                table: "Predmet");

            migrationBuilder.DropColumn(
                name: "AsistentId",
                table: "Obavjestenje");

            migrationBuilder.DropColumn(
                name: "AsistentId",
                table: "Ispit");

            migrationBuilder.RenameColumn(
                name: "ProfesorId",
                table: "Predmet",
                newName: "NastavnoOsobljeId");

            migrationBuilder.RenameIndex(
                name: "IX_Predmet_ProfesorId",
                table: "Predmet",
                newName: "IX_Predmet_NastavnoOsobljeId");

            migrationBuilder.RenameColumn(
                name: "ProfesorId",
                table: "Ocjena",
                newName: "NastavnoOsobljeId");

            migrationBuilder.RenameIndex(
                name: "IX_Ocjena_ProfesorId",
                table: "Ocjena",
                newName: "IX_Ocjena_NastavnoOsobljeId");

            migrationBuilder.RenameColumn(
                name: "ProfesorId",
                table: "Obavjestenje",
                newName: "NastavnoOsobljeId");

            migrationBuilder.RenameIndex(
                name: "IX_Obavjestenje_ProfesorId",
                table: "Obavjestenje",
                newName: "IX_Obavjestenje_NastavnoOsobljeId");

            migrationBuilder.RenameColumn(
                name: "ProfesorId",
                table: "Ispit",
                newName: "NastavnoOsobljeId");

            migrationBuilder.RenameIndex(
                name: "IX_Ispit_ProfesorId",
                table: "Ispit",
                newName: "IX_Ispit_NastavnoOsobljeId");

            migrationBuilder.AddColumn<long>(
                name: "StudentId",
                table: "Uvjerenje",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "StudentskaSluzbaId",
                table: "Uvjerenje",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "PredmetId",
                table: "Prijava",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "StudentskaSluzbaId",
                table: "Dokument",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "NastavnoOsoblje",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JMBG = table.Column<long>(type: "bigint", nullable: false),
                    Titula = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NastavnoOsoblje", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NastavnoOsoblje_Korisnik_JMBG",
                        column: x => x.JMBG,
                        principalTable: "Korisnik",
                        principalColumn: "JMBG",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Uvjerenje_StudentId",
                table: "Uvjerenje",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Uvjerenje_StudentskaSluzbaId",
                table: "Uvjerenje",
                column: "StudentskaSluzbaId");

            migrationBuilder.CreateIndex(
                name: "IX_Prijava_PredmetId",
                table: "Prijava",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_Dokument_StudentskaSluzbaId",
                table: "Dokument",
                column: "StudentskaSluzbaId");

            migrationBuilder.CreateIndex(
                name: "IX_NastavnoOsoblje_JMBG",
                table: "NastavnoOsoblje",
                column: "JMBG");

            migrationBuilder.AddForeignKey(
                name: "FK_Dokument_StudentskaSluzba_StudentskaSluzbaId",
                table: "Dokument",
                column: "StudentskaSluzbaId",
                principalTable: "StudentskaSluzba",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ispit_NastavnoOsoblje_NastavnoOsobljeId",
                table: "Ispit",
                column: "NastavnoOsobljeId",
                principalTable: "NastavnoOsoblje",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Obavjestenje_NastavnoOsoblje_NastavnoOsobljeId",
                table: "Obavjestenje",
                column: "NastavnoOsobljeId",
                principalTable: "NastavnoOsoblje",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Obavjestenje_StudentskaSluzba_StudentskaSluzbaId",
                table: "Obavjestenje",
                column: "StudentskaSluzbaId",
                principalTable: "StudentskaSluzba",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ocjena_NastavnoOsoblje_NastavnoOsobljeId",
                table: "Ocjena",
                column: "NastavnoOsobljeId",
                principalTable: "NastavnoOsoblje",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
                name: "FK_Prijava_Predmet_PredmetId",
                table: "Prijava",
                column: "PredmetId",
                principalTable: "Predmet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Uvjerenje_Student_StudentId",
                table: "Uvjerenje",
                column: "StudentId",
                principalTable: "Student",
                principalColumn: "BrojIndeksa",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Uvjerenje_StudentskaSluzba_StudentskaSluzbaId",
                table: "Uvjerenje",
                column: "StudentskaSluzbaId",
                principalTable: "StudentskaSluzba",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Zahtjev_Student_StudentId",
                table: "Zahtjev",
                column: "StudentId",
                principalTable: "Student",
                principalColumn: "BrojIndeksa",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Zahtjev_StudentskaSluzba_StudentskaSluzbaId",
                table: "Zahtjev",
                column: "StudentskaSluzbaId",
                principalTable: "StudentskaSluzba",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaBrojJedan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "StudijskiProgram",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    trajanjeUGodinama = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudijskiProgram", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NastavniPlan",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    godinaStudija = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StudijskiProgramId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NastavniPlan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NastavniPlan_StudijskiProgram_StudijskiProgramId",
                        column: x => x.StudijskiProgramId,
                        principalTable: "StudijskiProgram",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dokument",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Putanja = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    StudentskaSluzbaId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dokument", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ispit",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    datumOdrzavanja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    datumObjave = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Lokacija = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    brojBodova = table.Column<int>(type: "int", nullable: false),
                    PredmetId = table.Column<long>(type: "bigint", nullable: false),
                    KorisnikId = table.Column<long>(type: "bigint", nullable: true),
                    ProfesorId = table.Column<long>(type: "bigint", nullable: true),
                    AsistentId = table.Column<long>(type: "bigint", nullable: true),
                    StudentId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ispit", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Korisnik",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JMBG = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    Ime = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Prezime = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lozinka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Uloga = table.Column<int>(type: "int", nullable: false),
                    Titula = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Profesor_Titula = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    brojIndeksa = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    predhodnoObrazovanje = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    godinaStudija = table.Column<int>(type: "int", nullable: true),
                    podaciUplata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StudijskiProgramId = table.Column<long>(type: "bigint", nullable: true),
                    ZahtjevId = table.Column<long>(type: "bigint", nullable: true),
                    PredmetId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Korisnik", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Korisnik_StudijskiProgram_StudijskiProgramId",
                        column: x => x.StudijskiProgramId,
                        principalTable: "StudijskiProgram",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Obavjestenje",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naslov = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Sadrzaj = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    datumObjave = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KorisnikId = table.Column<long>(type: "bigint", nullable: true),
                    StudentskaSluzbaId = table.Column<long>(type: "bigint", nullable: true),
                    ProfesorId = table.Column<long>(type: "bigint", nullable: true),
                    AsistentId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Obavjestenje", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Obavjestenje_Korisnik_AsistentId",
                        column: x => x.AsistentId,
                        principalTable: "Korisnik",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Obavjestenje_Korisnik_KorisnikId",
                        column: x => x.KorisnikId,
                        principalTable: "Korisnik",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Obavjestenje_Korisnik_ProfesorId",
                        column: x => x.ProfesorId,
                        principalTable: "Korisnik",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Obavjestenje_Korisnik_StudentskaSluzbaId",
                        column: x => x.StudentskaSluzbaId,
                        principalTable: "Korisnik",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Predmet",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ECTS = table.Column<int>(type: "int", nullable: false),
                    ProfesorId = table.Column<long>(type: "bigint", nullable: true),
                    AsistentId = table.Column<long>(type: "bigint", nullable: true),
                    NastavniPlanId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Predmet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Predmet_Korisnik_AsistentId",
                        column: x => x.AsistentId,
                        principalTable: "Korisnik",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Predmet_Korisnik_ProfesorId",
                        column: x => x.ProfesorId,
                        principalTable: "Korisnik",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Predmet_NastavniPlan_NastavniPlanId",
                        column: x => x.NastavniPlanId,
                        principalTable: "NastavniPlan",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Prijava",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    datumPrijave = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IspitId = table.Column<long>(type: "bigint", nullable: false),
                    StudentId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prijava", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prijava_Ispit_IspitId",
                        column: x => x.IspitId,
                        principalTable: "Ispit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Prijava_Korisnik_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Korisnik",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Uvjerenje",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Namjena = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    datumIzdavanja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    StudentskaSluzbaId = table.Column<long>(type: "bigint", nullable: false),
                    Vrsta = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Uvjerenje", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Uvjerenje_Korisnik_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Korisnik",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Uvjerenje_Korisnik_StudentskaSluzbaId",
                        column: x => x.StudentskaSluzbaId,
                        principalTable: "Korisnik",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Zahtjev",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tipZahtjeva = table.Column<int>(type: "int", nullable: false),
                    statusZahtjeva = table.Column<int>(type: "int", nullable: false),
                    datumPodnosenja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    datumRjesavanja = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StudentId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zahtjev", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Zahtjev_Korisnik_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Korisnik",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ocjena",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Vrijednost = table.Column<float>(type: "real", nullable: false),
                    PredmetId = table.Column<long>(type: "bigint", nullable: false),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    ProfesorId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ocjena", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ocjena_Korisnik_ProfesorId",
                        column: x => x.ProfesorId,
                        principalTable: "Korisnik",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ocjena_Korisnik_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Korisnik",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ocjena_Predmet_PredmetId",
                        column: x => x.PredmetId,
                        principalTable: "Predmet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                        name: "FK_PredmetAsistent_Korisnik_AsistentId",
                        column: x => x.AsistentId,
                        principalTable: "Korisnik",
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
                        name: "FK_PredmetProfesor_Korisnik_ProfesorId",
                        column: x => x.ProfesorId,
                        principalTable: "Korisnik",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PredmetProfesor_Predmet_PredmetId",
                        column: x => x.PredmetId,
                        principalTable: "Predmet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentNaPredmetu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    akademskaGodina = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    PredmetId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentNaPredmetu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentNaPredmetu_Korisnik_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Korisnik",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentNaPredmetu_Predmet_PredmetId",
                        column: x => x.PredmetId,
                        principalTable: "Predmet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dokument_StudentId",
                table: "Dokument",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Dokument_StudentskaSluzbaId",
                table: "Dokument",
                column: "StudentskaSluzbaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ispit_AsistentId",
                table: "Ispit",
                column: "AsistentId");

            migrationBuilder.CreateIndex(
                name: "IX_Ispit_KorisnikId",
                table: "Ispit",
                column: "KorisnikId");

            migrationBuilder.CreateIndex(
                name: "IX_Ispit_PredmetId",
                table: "Ispit",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_Ispit_ProfesorId",
                table: "Ispit",
                column: "ProfesorId");

            migrationBuilder.CreateIndex(
                name: "IX_Ispit_StudentId",
                table: "Ispit",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Korisnik_PredmetId",
                table: "Korisnik",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_Korisnik_StudijskiProgramId",
                table: "Korisnik",
                column: "StudijskiProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Korisnik_ZahtjevId",
                table: "Korisnik",
                column: "ZahtjevId");

            migrationBuilder.CreateIndex(
                name: "IX_NastavniPlan_StudijskiProgramId",
                table: "NastavniPlan",
                column: "StudijskiProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Obavjestenje_AsistentId",
                table: "Obavjestenje",
                column: "AsistentId");

            migrationBuilder.CreateIndex(
                name: "IX_Obavjestenje_KorisnikId",
                table: "Obavjestenje",
                column: "KorisnikId");

            migrationBuilder.CreateIndex(
                name: "IX_Obavjestenje_ProfesorId",
                table: "Obavjestenje",
                column: "ProfesorId");

            migrationBuilder.CreateIndex(
                name: "IX_Obavjestenje_StudentskaSluzbaId",
                table: "Obavjestenje",
                column: "StudentskaSluzbaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ocjena_PredmetId",
                table: "Ocjena",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_Ocjena_ProfesorId",
                table: "Ocjena",
                column: "ProfesorId");

            migrationBuilder.CreateIndex(
                name: "IX_Ocjena_StudentId",
                table: "Ocjena",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Predmet_AsistentId",
                table: "Predmet",
                column: "AsistentId");

            migrationBuilder.CreateIndex(
                name: "IX_Predmet_NastavniPlanId",
                table: "Predmet",
                column: "NastavniPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Predmet_ProfesorId",
                table: "Predmet",
                column: "ProfesorId");

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
                name: "IX_Prijava_IspitId",
                table: "Prijava",
                column: "IspitId");

            migrationBuilder.CreateIndex(
                name: "IX_Prijava_StudentId",
                table: "Prijava",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentNaPredmetu_PredmetId",
                table: "StudentNaPredmetu",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentNaPredmetu_StudentId",
                table: "StudentNaPredmetu",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Uvjerenje_StudentId",
                table: "Uvjerenje",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Uvjerenje_StudentskaSluzbaId",
                table: "Uvjerenje",
                column: "StudentskaSluzbaId");

            migrationBuilder.CreateIndex(
                name: "IX_Zahtjev_StudentId",
                table: "Zahtjev",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Dokument_Korisnik_StudentId",
                table: "Dokument",
                column: "StudentId",
                principalTable: "Korisnik",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Dokument_Korisnik_StudentskaSluzbaId",
                table: "Dokument",
                column: "StudentskaSluzbaId",
                principalTable: "Korisnik",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ispit_Korisnik_AsistentId",
                table: "Ispit",
                column: "AsistentId",
                principalTable: "Korisnik",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ispit_Korisnik_KorisnikId",
                table: "Ispit",
                column: "KorisnikId",
                principalTable: "Korisnik",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ispit_Korisnik_ProfesorId",
                table: "Ispit",
                column: "ProfesorId",
                principalTable: "Korisnik",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ispit_Korisnik_StudentId",
                table: "Ispit",
                column: "StudentId",
                principalTable: "Korisnik",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ispit_Predmet_PredmetId",
                table: "Ispit",
                column: "PredmetId",
                principalTable: "Predmet",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Korisnik_Predmet_PredmetId",
                table: "Korisnik",
                column: "PredmetId",
                principalTable: "Predmet",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Korisnik_Zahtjev_ZahtjevId",
                table: "Korisnik",
                column: "ZahtjevId",
                principalTable: "Zahtjev",
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
                name: "FK_Zahtjev_Korisnik_StudentId",
                table: "Zahtjev");

            migrationBuilder.DropTable(
                name: "Dokument");

            migrationBuilder.DropTable(
                name: "Obavjestenje");

            migrationBuilder.DropTable(
                name: "Ocjena");

            migrationBuilder.DropTable(
                name: "PredmetAsistent");

            migrationBuilder.DropTable(
                name: "PredmetProfesor");

            migrationBuilder.DropTable(
                name: "Prijava");

            migrationBuilder.DropTable(
                name: "StudentNaPredmetu");

            migrationBuilder.DropTable(
                name: "Uvjerenje");

            migrationBuilder.DropTable(
                name: "Ispit");

            migrationBuilder.DropTable(
                name: "Korisnik");

            migrationBuilder.DropTable(
                name: "Predmet");

            migrationBuilder.DropTable(
                name: "Zahtjev");

            migrationBuilder.DropTable(
                name: "NastavniPlan");

            migrationBuilder.DropTable(
                name: "StudijskiProgram");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");
        }
    }
}

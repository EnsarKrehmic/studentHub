using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaBrojJedan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudijskiProgram",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TrajanjeUGodinama = table.Column<int>(type: "int", nullable: false)
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
                    GodinaStudija = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
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
                name: "AsistentStudijskiProgram",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AsistentId = table.Column<long>(type: "bigint", nullable: false),
                    StudijskiProgramId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsistentStudijskiProgram", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AsistentStudijskiProgram_StudijskiProgram_StudijskiProgramId",
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
                    DatumOdrzavanja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DatumObjave = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Lokacija = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BrojBodova = table.Column<int>(type: "int", nullable: true),
                    StudijskiProgramId = table.Column<long>(type: "bigint", nullable: false),
                    NastavniPlanId = table.Column<long>(type: "bigint", nullable: false),
                    PredmetId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ispit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ispit_NastavniPlan_NastavniPlanId",
                        column: x => x.NastavniPlanId,
                        principalTable: "NastavniPlan",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Ispit_StudijskiProgram_StudijskiProgramId",
                        column: x => x.StudijskiProgramId,
                        principalTable: "StudijskiProgram",
                        principalColumn: "Id");
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
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Lozinka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StudentId = table.Column<long>(type: "bigint", nullable: true),
                    ProfesorId = table.Column<long>(type: "bigint", nullable: true),
                    AsistentId = table.Column<long>(type: "bigint", nullable: true),
                    StudentskaSluzbaId = table.Column<long>(type: "bigint", nullable: true),
                    Uloga = table.Column<int>(type: "int", nullable: false),
                    AsistentTitula = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ProfesorTitula = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    BrojIndeksa = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PredhodnoObrazovanje = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GodinaStudija = table.Column<int>(type: "int", nullable: true),
                    Semestar = table.Column<int>(type: "int", nullable: true),
                    StudijskiProgramId = table.Column<long>(type: "bigint", nullable: true),
                    NastavniPlanId = table.Column<long>(type: "bigint", nullable: true),
                    PredmetId = table.Column<long>(type: "bigint", nullable: true),
                    StudentskaSluzba_StudijskiProgramId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Korisnik", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Korisnik_Korisnik_AsistentId",
                        column: x => x.AsistentId,
                        principalTable: "Korisnik",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Korisnik_Korisnik_ProfesorId",
                        column: x => x.ProfesorId,
                        principalTable: "Korisnik",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Korisnik_Korisnik_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Korisnik",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Korisnik_Korisnik_StudentskaSluzbaId",
                        column: x => x.StudentskaSluzbaId,
                        principalTable: "Korisnik",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Korisnik_NastavniPlan_NastavniPlanId",
                        column: x => x.NastavniPlanId,
                        principalTable: "NastavniPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Korisnik_StudijskiProgram_StudentskaSluzba_StudijskiProgramId",
                        column: x => x.StudentskaSluzba_StudijskiProgramId,
                        principalTable: "StudijskiProgram",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Korisnik_StudijskiProgram_StudijskiProgramId",
                        column: x => x.StudijskiProgramId,
                        principalTable: "StudijskiProgram",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Obavjestenje",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naslov = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Sadrzaj = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DatumObjave = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StudijskiProgramId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.ForeignKey(
                        name: "FK_Obavjestenje_StudijskiProgram_StudijskiProgramId",
                        column: x => x.StudijskiProgramId,
                        principalTable: "StudijskiProgram",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    NastavniPlanId = table.Column<long>(type: "bigint", nullable: false),
                    Semestar = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Predmet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Predmet_Korisnik_AsistentId",
                        column: x => x.AsistentId,
                        principalTable: "Korisnik",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Predmet_Korisnik_ProfesorId",
                        column: x => x.ProfesorId,
                        principalTable: "Korisnik",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Predmet_NastavniPlan_NastavniPlanId",
                        column: x => x.NastavniPlanId,
                        principalTable: "NastavniPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prijava",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatumPrijave = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                name: "ProfesorStudijskiProgram",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProfesorId = table.Column<long>(type: "bigint", nullable: false),
                    StudijskiProgramId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfesorStudijskiProgram", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfesorStudijskiProgram_Korisnik_ProfesorId",
                        column: x => x.ProfesorId,
                        principalTable: "Korisnik",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfesorStudijskiProgram_StudijskiProgram_StudijskiProgramId",
                        column: x => x.StudijskiProgramId,
                        principalTable: "StudijskiProgram",
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
                    DatumIzdavanja = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    TipZahtjeva = table.Column<int>(type: "int", nullable: false),
                    StatusZahtjeva = table.Column<int>(type: "int", nullable: false),
                    DatumPodnosenja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DatumRjesavanja = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    AkademskaGodina = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
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
                name: "IX_AsistentStudijskiProgram_AsistentId",
                table: "AsistentStudijskiProgram",
                column: "AsistentId");

            migrationBuilder.CreateIndex(
                name: "IX_AsistentStudijskiProgram_StudijskiProgramId",
                table: "AsistentStudijskiProgram",
                column: "StudijskiProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Dokument_StudentId",
                table: "Dokument",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Dokument_StudentskaSluzbaId",
                table: "Dokument",
                column: "StudentskaSluzbaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ispit_NastavniPlanId",
                table: "Ispit",
                column: "NastavniPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Ispit_PredmetId",
                table: "Ispit",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_Ispit_StudijskiProgramId",
                table: "Ispit",
                column: "StudijskiProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Korisnik_AsistentId",
                table: "Korisnik",
                column: "AsistentId");

            migrationBuilder.CreateIndex(
                name: "IX_Korisnik_NastavniPlanId",
                table: "Korisnik",
                column: "NastavniPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Korisnik_PredmetId",
                table: "Korisnik",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_Korisnik_ProfesorId",
                table: "Korisnik",
                column: "ProfesorId");

            migrationBuilder.CreateIndex(
                name: "IX_Korisnik_StudentId",
                table: "Korisnik",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Korisnik_StudentskaSluzba_StudijskiProgramId",
                table: "Korisnik",
                column: "StudentskaSluzba_StudijskiProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Korisnik_StudentskaSluzbaId",
                table: "Korisnik",
                column: "StudentskaSluzbaId");

            migrationBuilder.CreateIndex(
                name: "IX_Korisnik_StudijskiProgramId",
                table: "Korisnik",
                column: "StudijskiProgramId");

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
                name: "IX_Obavjestenje_StudijskiProgramId",
                table: "Obavjestenje",
                column: "StudijskiProgramId");

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
                name: "IX_ProfesorStudijskiProgram_ProfesorId",
                table: "ProfesorStudijskiProgram",
                column: "ProfesorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfesorStudijskiProgram_StudijskiProgramId",
                table: "ProfesorStudijskiProgram",
                column: "StudijskiProgramId");

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
                name: "FK_AsistentStudijskiProgram_Korisnik_AsistentId",
                table: "AsistentStudijskiProgram",
                column: "AsistentId",
                principalTable: "Korisnik",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.DropTable(
                name: "AsistentStudijskiProgram");

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
                name: "ProfesorStudijskiProgram");

            migrationBuilder.DropTable(
                name: "StudentNaPredmetu");

            migrationBuilder.DropTable(
                name: "Uvjerenje");

            migrationBuilder.DropTable(
                name: "Zahtjev");

            migrationBuilder.DropTable(
                name: "Ispit");

            migrationBuilder.DropTable(
                name: "Korisnik");

            migrationBuilder.DropTable(
                name: "Predmet");

            migrationBuilder.DropTable(
                name: "NastavniPlan");

            migrationBuilder.DropTable(
                name: "StudijskiProgram");
        }
    }
}

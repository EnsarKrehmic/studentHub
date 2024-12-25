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
                name: "Korisnik",
                columns: table => new
                {
                    JMBG = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ime = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Prezime = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lozinka = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Uloga = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Korisnik", x => x.JMBG);
                });

            migrationBuilder.CreateTable(
                name: "NastavnoOsoblje",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titula = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NastavnoOsoblje", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudentskaSluzba",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prezime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lozinka = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentskaSluzba", x => x.Id);
                });

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
                name: "Student",
                columns: table => new
                {
                    BrojIndeksa = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KorisnikId = table.Column<long>(type: "bigint", nullable: false),
                    PredhodnoObrazovanje = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StudijskiProgram = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GodinaStudija = table.Column<int>(type: "int", nullable: false),
                    PodaciUplata = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Student", x => x.BrojIndeksa);
                    table.ForeignKey(
                        name: "FK_Student_Korisnik_KorisnikId",
                        column: x => x.KorisnikId,
                        principalTable: "Korisnik",
                        principalColumn: "JMBG",
                        onDelete: ReferentialAction.Cascade);
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
                    SifraProfesora = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Predmet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Predmet_NastavnoOsoblje_SifraProfesora",
                        column: x => x.SifraProfesora,
                        principalTable: "NastavnoOsoblje",
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
                    DatumObjave = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StudentskaSluzbaId = table.Column<long>(type: "bigint", nullable: false),
                    NastavnoOsobljeId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Obavjestenje", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Obavjestenje_NastavnoOsoblje_NastavnoOsobljeId",
                        column: x => x.NastavnoOsobljeId,
                        principalTable: "NastavnoOsoblje",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Obavjestenje_StudentskaSluzba_StudentskaSluzbaId",
                        column: x => x.StudentskaSluzbaId,
                        principalTable: "StudentskaSluzba",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NastavniPlan",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GodinaStudija = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                name: "Zahtjev",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipZahtjeva = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DatumPodnosenja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DatumRjesavanja = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    StudentskaSluzbaId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zahtjev", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Zahtjev_Student_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Student",
                        principalColumn: "BrojIndeksa",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Zahtjev_StudentskaSluzba_StudentskaSluzbaId",
                        column: x => x.StudentskaSluzbaId,
                        principalTable: "StudentskaSluzba",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ispit",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    datumOdrzavanja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Lokacija = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    brojBodova = table.Column<int>(type: "int", nullable: false),
                    PredmetId = table.Column<long>(type: "bigint", nullable: false),
                    NastavnoOsobljeId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ispit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ispit_NastavnoOsoblje_NastavnoOsobljeId",
                        column: x => x.NastavnoOsobljeId,
                        principalTable: "NastavnoOsoblje",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ispit_Predmet_PredmetId",
                        column: x => x.PredmetId,
                        principalTable: "Predmet",
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
                    NastavnoOsobljeId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ocjena", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ocjena_NastavnoOsoblje_NastavnoOsobljeId",
                        column: x => x.NastavnoOsobljeId,
                        principalTable: "NastavnoOsoblje",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ocjena_Predmet_PredmetId",
                        column: x => x.PredmetId,
                        principalTable: "Predmet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ocjena_Student_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Student",
                        principalColumn: "BrojIndeksa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentNaPredmetu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AkademskaGodina = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    PredmetId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentNaPredmetu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentNaPredmetu_Predmet_PredmetId",
                        column: x => x.PredmetId,
                        principalTable: "Predmet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentNaPredmetu_Student_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Student",
                        principalColumn: "BrojIndeksa",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dokument",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Putanja = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ZahtjevId = table.Column<long>(type: "bigint", nullable: true),
                    KorisnikId = table.Column<long>(type: "bigint", nullable: true),
                    StudentskaSluzbaId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dokument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dokument_Korisnik_KorisnikId",
                        column: x => x.KorisnikId,
                        principalTable: "Korisnik",
                        principalColumn: "JMBG");
                    table.ForeignKey(
                        name: "FK_Dokument_StudentskaSluzba_StudentskaSluzbaId",
                        column: x => x.StudentskaSluzbaId,
                        principalTable: "StudentskaSluzba",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dokument_Zahtjev_ZahtjevId",
                        column: x => x.ZahtjevId,
                        principalTable: "Zahtjev",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Uvjerenje",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Namjena = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DatumIzdavanja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ZahtjevId = table.Column<long>(type: "bigint", nullable: false),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    StudentskaSluzbaId = table.Column<long>(type: "bigint", nullable: false),
                    Vrsta = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Uvjerenje", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Uvjerenje_Student_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Student",
                        principalColumn: "BrojIndeksa",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Uvjerenje_StudentskaSluzba_StudentskaSluzbaId",
                        column: x => x.StudentskaSluzbaId,
                        principalTable: "StudentskaSluzba",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Uvjerenje_Zahtjev_ZahtjevId",
                        column: x => x.ZahtjevId,
                        principalTable: "Zahtjev",
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
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    PredmetId = table.Column<long>(type: "bigint", nullable: false)
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
                        name: "FK_Prijava_Predmet_PredmetId",
                        column: x => x.PredmetId,
                        principalTable: "Predmet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Prijava_Student_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Student",
                        principalColumn: "BrojIndeksa",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dokument_KorisnikId",
                table: "Dokument",
                column: "KorisnikId");

            migrationBuilder.CreateIndex(
                name: "IX_Dokument_StudentskaSluzbaId",
                table: "Dokument",
                column: "StudentskaSluzbaId");

            migrationBuilder.CreateIndex(
                name: "IX_Dokument_ZahtjevId",
                table: "Dokument",
                column: "ZahtjevId");

            migrationBuilder.CreateIndex(
                name: "IX_Ispit_NastavnoOsobljeId",
                table: "Ispit",
                column: "NastavnoOsobljeId");

            migrationBuilder.CreateIndex(
                name: "IX_Ispit_PredmetId",
                table: "Ispit",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_NastavniPlan_StudijskiProgramId",
                table: "NastavniPlan",
                column: "StudijskiProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Obavjestenje_NastavnoOsobljeId",
                table: "Obavjestenje",
                column: "NastavnoOsobljeId");

            migrationBuilder.CreateIndex(
                name: "IX_Obavjestenje_StudentskaSluzbaId",
                table: "Obavjestenje",
                column: "StudentskaSluzbaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ocjena_NastavnoOsobljeId",
                table: "Ocjena",
                column: "NastavnoOsobljeId");

            migrationBuilder.CreateIndex(
                name: "IX_Ocjena_PredmetId",
                table: "Ocjena",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_Ocjena_StudentId",
                table: "Ocjena",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Predmet_SifraProfesora",
                table: "Predmet",
                column: "SifraProfesora");

            migrationBuilder.CreateIndex(
                name: "IX_Prijava_IspitId",
                table: "Prijava",
                column: "IspitId");

            migrationBuilder.CreateIndex(
                name: "IX_Prijava_PredmetId",
                table: "Prijava",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_Prijava_StudentId",
                table: "Prijava",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Student_KorisnikId",
                table: "Student",
                column: "KorisnikId");

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
                name: "IX_Uvjerenje_ZahtjevId",
                table: "Uvjerenje",
                column: "ZahtjevId");

            migrationBuilder.CreateIndex(
                name: "IX_Zahtjev_StudentId",
                table: "Zahtjev",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Zahtjev_StudentskaSluzbaId",
                table: "Zahtjev",
                column: "StudentskaSluzbaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dokument");

            migrationBuilder.DropTable(
                name: "NastavniPlan");

            migrationBuilder.DropTable(
                name: "Obavjestenje");

            migrationBuilder.DropTable(
                name: "Ocjena");

            migrationBuilder.DropTable(
                name: "Prijava");

            migrationBuilder.DropTable(
                name: "StudentNaPredmetu");

            migrationBuilder.DropTable(
                name: "Uvjerenje");

            migrationBuilder.DropTable(
                name: "StudijskiProgram");

            migrationBuilder.DropTable(
                name: "Ispit");

            migrationBuilder.DropTable(
                name: "Zahtjev");

            migrationBuilder.DropTable(
                name: "Predmet");

            migrationBuilder.DropTable(
                name: "Student");

            migrationBuilder.DropTable(
                name: "StudentskaSluzba");

            migrationBuilder.DropTable(
                name: "NastavnoOsoblje");

            migrationBuilder.DropTable(
                name: "Korisnik");

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

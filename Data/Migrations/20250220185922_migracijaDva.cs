using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaDva : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "ProfesorId",
                table: "Ocjena",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "PredmetId",
                table: "Ocjena",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "NastavnaAktivnostId",
                table: "Ocjena",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Tip",
                table: "Ocjena",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "NastavnaAktivnost",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Tip = table.Column<int>(type: "int", nullable: false),
                    DatumVrijemeOdrzavanja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ManuelnoOtkljucano = table.Column<bool>(type: "bit", nullable: false),
                    PredmetId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NastavnaAktivnost", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NastavnaAktivnost_Predmet_PredmetId",
                        column: x => x.PredmetId,
                        principalTable: "Predmet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Komentar",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sadrzaj = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DatumVrijeme = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    NastavnaAktivnostId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Komentar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Komentar_Korisnik_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Korisnik",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Komentar_NastavnaAktivnost_NastavnaAktivnostId",
                        column: x => x.NastavnaAktivnostId,
                        principalTable: "NastavnaAktivnost",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NastavniMaterijal",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PutanjaDoFajla = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NastavnaAktivnostId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NastavniMaterijal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NastavniMaterijal_NastavnaAktivnost_NastavnaAktivnostId",
                        column: x => x.NastavnaAktivnostId,
                        principalTable: "NastavnaAktivnost",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ocjena_NastavnaAktivnostId",
                table: "Ocjena",
                column: "NastavnaAktivnostId");

            migrationBuilder.CreateIndex(
                name: "IX_Komentar_NastavnaAktivnostId",
                table: "Komentar",
                column: "NastavnaAktivnostId");

            migrationBuilder.CreateIndex(
                name: "IX_Komentar_StudentId",
                table: "Komentar",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_NastavnaAktivnost_PredmetId",
                table: "NastavnaAktivnost",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_NastavniMaterijal_NastavnaAktivnostId",
                table: "NastavniMaterijal",
                column: "NastavnaAktivnostId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ocjena_NastavnaAktivnost_NastavnaAktivnostId",
                table: "Ocjena",
                column: "NastavnaAktivnostId",
                principalTable: "NastavnaAktivnost",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ocjena_NastavnaAktivnost_NastavnaAktivnostId",
                table: "Ocjena");

            migrationBuilder.DropTable(
                name: "Komentar");

            migrationBuilder.DropTable(
                name: "NastavniMaterijal");

            migrationBuilder.DropTable(
                name: "NastavnaAktivnost");

            migrationBuilder.DropIndex(
                name: "IX_Ocjena_NastavnaAktivnostId",
                table: "Ocjena");

            migrationBuilder.DropColumn(
                name: "NastavnaAktivnostId",
                table: "Ocjena");

            migrationBuilder.DropColumn(
                name: "Tip",
                table: "Ocjena");

            migrationBuilder.AlterColumn<long>(
                name: "ProfesorId",
                table: "Ocjena",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "PredmetId",
                table: "Ocjena",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}

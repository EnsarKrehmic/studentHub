using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracija7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ZahtjevZaPrisustvo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    NastavnaAktivnostId = table.Column<long>(type: "bigint", nullable: false),
                    VrijemePodnosenja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Odbijen = table.Column<bool>(type: "bit", nullable: false),
                    KodUnesen = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZahtjevZaPrisustvo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZahtjevZaPrisustvo_Korisnik_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Korisnik",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ZahtjevZaPrisustvo_NastavnaAktivnost_NastavnaAktivnostId",
                        column: x => x.NastavnaAktivnostId,
                        principalTable: "NastavnaAktivnost",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ZahtjevZaPrisustvo_NastavnaAktivnostId",
                table: "ZahtjevZaPrisustvo",
                column: "NastavnaAktivnostId");

            migrationBuilder.CreateIndex(
                name: "IX_ZahtjevZaPrisustvo_StudentId",
                table: "ZahtjevZaPrisustvo",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ZahtjevZaPrisustvo");
        }
    }
}

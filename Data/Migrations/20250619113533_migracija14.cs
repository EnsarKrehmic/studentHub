using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracija14 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Raspored",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudijskiProgramId = table.Column<long>(type: "bigint", nullable: false),
                    GodinaStudija = table.Column<int>(type: "int", nullable: false),
                    Semestar = table.Column<int>(type: "int", nullable: false),
                    AkademskaGodina = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Raspored", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Raspored_StudijskiProgram_StudijskiProgramId",
                        column: x => x.StudijskiProgramId,
                        principalTable: "StudijskiProgram",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TerminNastave",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PredmetId = table.Column<long>(type: "bigint", nullable: false),
                    Vrsta = table.Column<int>(type: "int", nullable: false),
                    Dan = table.Column<int>(type: "int", nullable: false),
                    VrijemeOd = table.Column<TimeSpan>(type: "time", nullable: false),
                    VrijemeDo = table.Column<TimeSpan>(type: "time", nullable: false),
                    Lokacija = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RasporedId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerminNastave", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TerminNastave_Predmet_PredmetId",
                        column: x => x.PredmetId,
                        principalTable: "Predmet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TerminNastave_Raspored_RasporedId",
                        column: x => x.RasporedId,
                        principalTable: "Raspored",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Raspored_StudijskiProgramId",
                table: "Raspored",
                column: "StudijskiProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_TerminNastave_PredmetId",
                table: "TerminNastave",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_TerminNastave_RasporedId",
                table: "TerminNastave",
                column: "RasporedId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TerminNastave");

            migrationBuilder.DropTable(
                name: "Raspored");
        }
    }
}

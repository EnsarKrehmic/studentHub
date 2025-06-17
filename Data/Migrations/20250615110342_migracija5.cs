using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracija5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PrisustvoNaAktivnosti",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    NastavnaAktivnostId = table.Column<long>(type: "bigint", nullable: false),
                    VrijemeEvidentiranja = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrisustvoNaAktivnosti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrisustvoNaAktivnosti_Korisnik_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Korisnik",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PrisustvoNaAktivnosti_NastavnaAktivnost_NastavnaAktivnostId",
                        column: x => x.NastavnaAktivnostId,
                        principalTable: "NastavnaAktivnost",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrisustvoNaAktivnosti_NastavnaAktivnostId",
                table: "PrisustvoNaAktivnosti",
                column: "NastavnaAktivnostId");

            migrationBuilder.CreateIndex(
                name: "IX_PrisustvoNaAktivnosti_StudentId",
                table: "PrisustvoNaAktivnosti",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrisustvoNaAktivnosti");
        }
    }
}

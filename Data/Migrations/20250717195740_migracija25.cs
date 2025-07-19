using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracija25 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pravilnik",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naslov = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DatumKreiranja = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pravilnik", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PravilnikClanak",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PravilnikId = table.Column<int>(type: "int", nullable: false),
                    NaslovClanka = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Sadrzaj = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    RedniBroj = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PravilnikClanak", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PravilnikClanak_Pravilnik_PravilnikId",
                        column: x => x.PravilnikId,
                        principalTable: "Pravilnik",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PravilnikClanak_PravilnikId",
                table: "PravilnikClanak",
                column: "PravilnikId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PravilnikClanak");

            migrationBuilder.DropTable(
                name: "Pravilnik");
        }
    }
}

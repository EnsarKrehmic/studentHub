using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracija6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "KodAktivanDo",
                table: "NastavnaAktivnost",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KodZaPrisustvo",
                table: "NastavnaAktivnost",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VrijemeGenerisanjaKoda",
                table: "NastavnaAktivnost",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KodAktivanDo",
                table: "NastavnaAktivnost");

            migrationBuilder.DropColumn(
                name: "KodZaPrisustvo",
                table: "NastavnaAktivnost");

            migrationBuilder.DropColumn(
                name: "VrijemeGenerisanjaKoda",
                table: "NastavnaAktivnost");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracija11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DatumUnosa",
                table: "Ocjena",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Komentar",
                table: "Ocjena",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ParentOcjenaId",
                table: "Ocjena",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TezinaProcentualno",
                table: "Ocjena",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ocjena_ParentOcjenaId",
                table: "Ocjena",
                column: "ParentOcjenaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ocjena_Ocjena_ParentOcjenaId",
                table: "Ocjena",
                column: "ParentOcjenaId",
                principalTable: "Ocjena",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ocjena_Ocjena_ParentOcjenaId",
                table: "Ocjena");

            migrationBuilder.DropIndex(
                name: "IX_Ocjena_ParentOcjenaId",
                table: "Ocjena");

            migrationBuilder.DropColumn(
                name: "DatumUnosa",
                table: "Ocjena");

            migrationBuilder.DropColumn(
                name: "Komentar",
                table: "Ocjena");

            migrationBuilder.DropColumn(
                name: "ParentOcjenaId",
                table: "Ocjena");

            migrationBuilder.DropColumn(
                name: "TezinaProcentualno",
                table: "Ocjena");
        }
    }
}

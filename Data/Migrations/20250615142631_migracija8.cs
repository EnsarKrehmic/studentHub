using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracija8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PragPrisustvaPredavanja",
                table: "Predmet",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PragPrisustvaVjezbe",
                table: "Predmet",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PragPrisustvaPredavanja",
                table: "Predmet");

            migrationBuilder.DropColumn(
                name: "PragPrisustvaVjezbe",
                table: "Predmet");
        }
    }
}

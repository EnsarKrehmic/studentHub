using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaBrDevet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Bodovi",
                table: "Prijava",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UslovZaPolaganje",
                table: "Ispit",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bodovi",
                table: "Prijava");

            migrationBuilder.DropColumn(
                name: "UslovZaPolaganje",
                table: "Ispit");
        }
    }
}

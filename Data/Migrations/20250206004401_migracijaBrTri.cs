using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaBrTri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AspNetUserId",
                table: "StudentNaPredmetu",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AspNetUserId",
                table: "PredmetProfesor",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AspNetUserId",
                table: "PredmetAsistent",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AspNetUserId",
                table: "StudentNaPredmetu");

            migrationBuilder.DropColumn(
                name: "AspNetUserId",
                table: "PredmetProfesor");

            migrationBuilder.DropColumn(
                name: "AspNetUserId",
                table: "PredmetAsistent");
        }
    }
}

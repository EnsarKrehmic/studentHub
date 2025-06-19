using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracija12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ocjena_Ocjena_ParentOcjenaId",
                table: "Ocjena");

            migrationBuilder.DropIndex(
                name: "IX_Ocjena_ParentOcjenaId",
                table: "Ocjena");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}

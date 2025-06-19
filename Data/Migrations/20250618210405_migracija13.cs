using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracija13 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "IspitId",
                table: "Ocjena",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ocjena_IspitId",
                table: "Ocjena",
                column: "IspitId");

            migrationBuilder.CreateIndex(
                name: "IX_Ocjena_ParentOcjenaId",
                table: "Ocjena",
                column: "ParentOcjenaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ocjena_Ispit_IspitId",
                table: "Ocjena",
                column: "IspitId",
                principalTable: "Ispit",
                principalColumn: "Id");

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
                name: "FK_Ocjena_Ispit_IspitId",
                table: "Ocjena");

            migrationBuilder.DropForeignKey(
                name: "FK_Ocjena_Ocjena_ParentOcjenaId",
                table: "Ocjena");

            migrationBuilder.DropIndex(
                name: "IX_Ocjena_IspitId",
                table: "Ocjena");

            migrationBuilder.DropIndex(
                name: "IX_Ocjena_ParentOcjenaId",
                table: "Ocjena");

            migrationBuilder.DropColumn(
                name: "IspitId",
                table: "Ocjena");
        }
    }
}

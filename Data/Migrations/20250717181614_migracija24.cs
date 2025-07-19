using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracija24 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PodrskaUpitId",
                table: "ChatbotLogUpiti",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatbotLogUpiti_PodrskaUpitId",
                table: "ChatbotLogUpiti",
                column: "PodrskaUpitId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatbotLogUpiti_PodrskaUpit_PodrskaUpitId",
                table: "ChatbotLogUpiti",
                column: "PodrskaUpitId",
                principalTable: "PodrskaUpit",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatbotLogUpiti_PodrskaUpit_PodrskaUpitId",
                table: "ChatbotLogUpiti");

            migrationBuilder.DropIndex(
                name: "IX_ChatbotLogUpiti_PodrskaUpitId",
                table: "ChatbotLogUpiti");

            migrationBuilder.DropColumn(
                name: "PodrskaUpitId",
                table: "ChatbotLogUpiti");
        }
    }
}

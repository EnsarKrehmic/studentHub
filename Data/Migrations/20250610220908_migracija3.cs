using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracija3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MentionedUserId",
                table: "Komentar",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Komentar_MentionedUserId",
                table: "Komentar",
                column: "MentionedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Komentar_AspNetUsers_MentionedUserId",
                table: "Komentar",
                column: "MentionedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Komentar_AspNetUsers_MentionedUserId",
                table: "Komentar");

            migrationBuilder.DropIndex(
                name: "IX_Komentar_MentionedUserId",
                table: "Komentar");

            migrationBuilder.DropColumn(
                name: "MentionedUserId",
                table: "Komentar");
        }
    }
}

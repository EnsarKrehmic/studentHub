using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaBrojDevet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KorisnikStudijskiProgram");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KorisnikStudijskiProgram",
                columns: table => new
                {
                    KorisnikId = table.Column<long>(type: "bigint", nullable: false),
                    StudijskiProgramId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KorisnikStudijskiProgram", x => new { x.KorisnikId, x.StudijskiProgramId });
                    table.ForeignKey(
                        name: "FK_KorisnikStudijskiProgram_Korisnik_KorisnikId",
                        column: x => x.KorisnikId,
                        principalTable: "Korisnik",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_KorisnikStudijskiProgram_StudijskiProgram_StudijskiProgramId",
                        column: x => x.StudijskiProgramId,
                        principalTable: "StudijskiProgram",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_KorisnikStudijskiProgram_StudijskiProgramId",
                table: "KorisnikStudijskiProgram",
                column: "StudijskiProgramId");
        }
    }
}

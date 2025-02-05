using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaBrojSedam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ObavjestenjeStudijskiProgram",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ObavjestenjeId = table.Column<long>(type: "bigint", nullable: false),
                    StudijskiProgramId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObavjestenjeStudijskiProgram", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ObavjestenjeStudijskiProgram_Obavjestenje_ObavjestenjeId",
                        column: x => x.ObavjestenjeId,
                        principalTable: "Obavjestenje",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ObavjestenjeStudijskiProgram_StudijskiProgram_StudijskiProgramId",
                        column: x => x.StudijskiProgramId,
                        principalTable: "StudijskiProgram",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ObavjestenjeStudijskiProgram_ObavjestenjeId",
                table: "ObavjestenjeStudijskiProgram",
                column: "ObavjestenjeId");

            migrationBuilder.CreateIndex(
                name: "IX_ObavjestenjeStudijskiProgram_StudijskiProgramId",
                table: "ObavjestenjeStudijskiProgram",
                column: "StudijskiProgramId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ObavjestenjeStudijskiProgram");
        }
    }
}

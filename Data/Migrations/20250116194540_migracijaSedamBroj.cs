using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaSedamBroj : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AsistentStudijskiProgram",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AsistentId = table.Column<long>(type: "bigint", nullable: false),
                    StudijskiProgramId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsistentStudijskiProgram", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AsistentStudijskiProgram_Korisnik_AsistentId",
                        column: x => x.AsistentId,
                        principalTable: "Korisnik",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AsistentStudijskiProgram_StudijskiProgram_StudijskiProgramId",
                        column: x => x.StudijskiProgramId,
                        principalTable: "StudijskiProgram",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfesorStudijskiProgram",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProfesorId = table.Column<long>(type: "bigint", nullable: false),
                    StudijskiProgramId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfesorStudijskiProgram", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfesorStudijskiProgram_Korisnik_ProfesorId",
                        column: x => x.ProfesorId,
                        principalTable: "Korisnik",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfesorStudijskiProgram_StudijskiProgram_StudijskiProgramId",
                        column: x => x.StudijskiProgramId,
                        principalTable: "StudijskiProgram",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AsistentStudijskiProgram_AsistentId",
                table: "AsistentStudijskiProgram",
                column: "AsistentId");

            migrationBuilder.CreateIndex(
                name: "IX_AsistentStudijskiProgram_StudijskiProgramId",
                table: "AsistentStudijskiProgram",
                column: "StudijskiProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfesorStudijskiProgram_ProfesorId",
                table: "ProfesorStudijskiProgram",
                column: "ProfesorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfesorStudijskiProgram_StudijskiProgramId",
                table: "ProfesorStudijskiProgram",
                column: "StudijskiProgramId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AsistentStudijskiProgram");

            migrationBuilder.DropTable(
                name: "ProfesorStudijskiProgram");
        }
    }
}

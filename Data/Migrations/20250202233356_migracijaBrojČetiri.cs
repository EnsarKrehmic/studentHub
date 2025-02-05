using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaBrojČetiri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Korisnik_Korisnik_AsistentId",
                table: "Korisnik");

            migrationBuilder.DropForeignKey(
                name: "FK_Korisnik_Korisnik_ProfesorId",
                table: "Korisnik");

            migrationBuilder.DropForeignKey(
                name: "FK_Korisnik_Korisnik_StudentId",
                table: "Korisnik");

            migrationBuilder.DropForeignKey(
                name: "FK_Korisnik_Korisnik_StudentskaSluzbaId",
                table: "Korisnik");

            migrationBuilder.DropIndex(
                name: "IX_Korisnik_AsistentId",
                table: "Korisnik");

            migrationBuilder.DropIndex(
                name: "IX_Korisnik_ProfesorId",
                table: "Korisnik");

            migrationBuilder.DropIndex(
                name: "IX_Korisnik_StudentId",
                table: "Korisnik");

            migrationBuilder.DropIndex(
                name: "IX_Korisnik_StudentskaSluzbaId",
                table: "Korisnik");

            migrationBuilder.DropColumn(
                name: "AsistentId",
                table: "Korisnik");

            migrationBuilder.DropColumn(
                name: "ProfesorId",
                table: "Korisnik");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "Korisnik");

            migrationBuilder.DropColumn(
                name: "StudentskaSluzbaId",
                table: "Korisnik");

            migrationBuilder.CreateTable(
                name: "StudentskaSluzbaStudijskiProgram",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentskaSluzbaId = table.Column<long>(type: "bigint", nullable: false),
                    StudijskiProgramId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentskaSluzbaStudijskiProgram", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentskaSluzbaStudijskiProgram_Korisnik_StudentskaSluzbaId",
                        column: x => x.StudentskaSluzbaId,
                        principalTable: "Korisnik",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentskaSluzbaStudijskiProgram_StudijskiProgram_StudijskiProgramId",
                        column: x => x.StudijskiProgramId,
                        principalTable: "StudijskiProgram",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentStudijskiProgram",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    StudijskiProgramId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentStudijskiProgram", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentStudijskiProgram_Korisnik_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Korisnik",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentStudijskiProgram_StudijskiProgram_StudijskiProgramId",
                        column: x => x.StudijskiProgramId,
                        principalTable: "StudijskiProgram",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentskaSluzbaStudijskiProgram_StudentskaSluzbaId",
                table: "StudentskaSluzbaStudijskiProgram",
                column: "StudentskaSluzbaId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentskaSluzbaStudijskiProgram_StudijskiProgramId",
                table: "StudentskaSluzbaStudijskiProgram",
                column: "StudijskiProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentStudijskiProgram_StudentId",
                table: "StudentStudijskiProgram",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentStudijskiProgram_StudijskiProgramId",
                table: "StudentStudijskiProgram",
                column: "StudijskiProgramId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentskaSluzbaStudijskiProgram");

            migrationBuilder.DropTable(
                name: "StudentStudijskiProgram");

            migrationBuilder.AddColumn<long>(
                name: "AsistentId",
                table: "Korisnik",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProfesorId",
                table: "Korisnik",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "StudentId",
                table: "Korisnik",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "StudentskaSluzbaId",
                table: "Korisnik",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Korisnik_AsistentId",
                table: "Korisnik",
                column: "AsistentId");

            migrationBuilder.CreateIndex(
                name: "IX_Korisnik_ProfesorId",
                table: "Korisnik",
                column: "ProfesorId");

            migrationBuilder.CreateIndex(
                name: "IX_Korisnik_StudentId",
                table: "Korisnik",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Korisnik_StudentskaSluzbaId",
                table: "Korisnik",
                column: "StudentskaSluzbaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Korisnik_Korisnik_AsistentId",
                table: "Korisnik",
                column: "AsistentId",
                principalTable: "Korisnik",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Korisnik_Korisnik_ProfesorId",
                table: "Korisnik",
                column: "ProfesorId",
                principalTable: "Korisnik",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Korisnik_Korisnik_StudentId",
                table: "Korisnik",
                column: "StudentId",
                principalTable: "Korisnik",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Korisnik_Korisnik_StudentskaSluzbaId",
                table: "Korisnik",
                column: "StudentskaSluzbaId",
                principalTable: "Korisnik",
                principalColumn: "Id");
        }
    }
}

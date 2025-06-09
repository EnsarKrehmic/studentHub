using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaOsam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TipPredmeta",
                table: "Predmet",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "StudijskiProgramIzborniLimit",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudijskiProgramId = table.Column<long>(type: "bigint", nullable: false),
                    GodinaStudija = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MinIzborniPredmeti = table.Column<int>(type: "int", nullable: false),
                    MaxIzborniPredmeti = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudijskiProgramIzborniLimit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudijskiProgramIzborniLimit_StudijskiProgram_StudijskiProgramId",
                        column: x => x.StudijskiProgramId,
                        principalTable: "StudijskiProgram",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudijskiProgramIzborniLimit_StudijskiProgramId",
                table: "StudijskiProgramIzborniLimit",
                column: "StudijskiProgramId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudijskiProgramIzborniLimit");

            migrationBuilder.DropColumn(
                name: "TipPredmeta",
                table: "Predmet");
        }
    }
}

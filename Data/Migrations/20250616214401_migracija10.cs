using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracija10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PutanjaDoFajla",
                table: "NastavniMaterijal");

            migrationBuilder.DropColumn(
                name: "TipFajla",
                table: "NastavniMaterijal");

            migrationBuilder.CreateTable(
                name: "NastavniMaterijalFajl",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PutanjaDoFajla = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipFajla = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NastavniMaterijalId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NastavniMaterijalFajl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NastavniMaterijalFajl_NastavniMaterijal_NastavniMaterijalId",
                        column: x => x.NastavniMaterijalId,
                        principalTable: "NastavniMaterijal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NastavniMaterijalFajl_NastavniMaterijalId",
                table: "NastavniMaterijalFajl",
                column: "NastavniMaterijalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NastavniMaterijalFajl");

            migrationBuilder.AddColumn<string>(
                name: "PutanjaDoFajla",
                table: "NastavniMaterijal",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TipFajla",
                table: "NastavniMaterijal",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}

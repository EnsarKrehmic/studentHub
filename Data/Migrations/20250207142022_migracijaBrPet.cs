using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaBrPet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "BrojBodova",
                table: "Ispit",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "DokumentSlike",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DokumentId = table.Column<long>(type: "bigint", nullable: false),
                    Putanja = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DokumentSlike", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DokumentSlike_Dokument_DokumentId",
                        column: x => x.DokumentId,
                        principalTable: "Dokument",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DokumentSlike_DokumentId",
                table: "DokumentSlike",
                column: "DokumentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DokumentSlike");

            migrationBuilder.AlterColumn<int>(
                name: "BrojBodova",
                table: "Ispit",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}

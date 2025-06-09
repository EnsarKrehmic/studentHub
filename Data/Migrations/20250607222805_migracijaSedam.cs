using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class migracijaSedam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "IspitId",
                table: "Komentar",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "KorisnikId",
                table: "Komentar",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "PrilogPath",
                table: "Komentar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Vidljivost",
                table: "Komentar",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "KomentarVidljivosti",
                columns: table => new
                {
                    KomentarId = table.Column<long>(type: "bigint", nullable: false),
                    KorisnikId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KomentarVidljivosti", x => new { x.KomentarId, x.KorisnikId });
                    table.ForeignKey(
                        name: "FK_KomentarVidljivosti_Komentar_KomentarId",
                        column: x => x.KomentarId,
                        principalTable: "Komentar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KomentarVidljivosti_Korisnik_KorisnikId",
                        column: x => x.KorisnikId,
                        principalTable: "Korisnik",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Komentar_IspitId",
                table: "Komentar",
                column: "IspitId");

            migrationBuilder.CreateIndex(
                name: "IX_Komentar_KorisnikId",
                table: "Komentar",
                column: "KorisnikId");

            migrationBuilder.CreateIndex(
                name: "IX_KomentarVidljivosti_KorisnikId",
                table: "KomentarVidljivosti",
                column: "KorisnikId");

            migrationBuilder.AddForeignKey(
                name: "FK_Komentar_Ispit_IspitId",
                table: "Komentar",
                column: "IspitId",
                principalTable: "Ispit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Komentar_Korisnik_KorisnikId",
                table: "Komentar",
                column: "KorisnikId",
                principalTable: "Korisnik",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Komentar_Ispit_IspitId",
                table: "Komentar");

            migrationBuilder.DropForeignKey(
                name: "FK_Komentar_Korisnik_KorisnikId",
                table: "Komentar");

            migrationBuilder.DropTable(
                name: "KomentarVidljivosti");

            migrationBuilder.DropIndex(
                name: "IX_Komentar_IspitId",
                table: "Komentar");

            migrationBuilder.DropIndex(
                name: "IX_Komentar_KorisnikId",
                table: "Komentar");

            migrationBuilder.DropColumn(
                name: "IspitId",
                table: "Komentar");

            migrationBuilder.DropColumn(
                name: "KorisnikId",
                table: "Komentar");

            migrationBuilder.DropColumn(
                name: "PrilogPath",
                table: "Komentar");

            migrationBuilder.DropColumn(
                name: "Vidljivost",
                table: "Komentar");

            migrationBuilder.AddColumn<string>(
                name: "Lozinka",
                table: "Korisnik",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

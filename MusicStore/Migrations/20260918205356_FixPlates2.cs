using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exam.Migrations
{
    /// <inheritdoc />
    public partial class FixPlates2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Plates_Genres_GenresId",
                table: "Plates");

            migrationBuilder.DropIndex(
                name: "IX_Plates_GenresId",
                table: "Plates");

            migrationBuilder.DropColumn(
                name: "GenresId",
                table: "Plates");

            migrationBuilder.CreateIndex(
                name: "IX_Plates_GenreId",
                table: "Plates",
                column: "GenreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Plates_Genres_GenreId",
                table: "Plates",
                column: "GenreId",
                principalTable: "Genres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Plates_Genres_GenreId",
                table: "Plates");

            migrationBuilder.DropIndex(
                name: "IX_Plates_GenreId",
                table: "Plates");

            migrationBuilder.AddColumn<int>(
                name: "GenresId",
                table: "Plates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Plates_GenresId",
                table: "Plates",
                column: "GenresId");

            migrationBuilder.AddForeignKey(
                name: "FK_Plates_Genres_GenresId",
                table: "Plates",
                column: "GenresId",
                principalTable: "Genres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

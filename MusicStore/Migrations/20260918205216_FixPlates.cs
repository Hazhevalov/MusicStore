using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exam.Migrations
{
    /// <inheritdoc />
    public partial class FixPlates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Plates_Artists_ArtistsId",
                table: "Plates");

            migrationBuilder.DropIndex(
                name: "IX_Plates_ArtistsId",
                table: "Plates");

            migrationBuilder.DropColumn(
                name: "ArtistsId",
                table: "Plates");

            migrationBuilder.CreateIndex(
                name: "IX_Plates_ArtistId",
                table: "Plates",
                column: "ArtistId");

            migrationBuilder.AddForeignKey(
                name: "FK_Plates_Artists_ArtistId",
                table: "Plates",
                column: "ArtistId",
                principalTable: "Artists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Plates_Artists_ArtistId",
                table: "Plates");

            migrationBuilder.DropIndex(
                name: "IX_Plates_ArtistId",
                table: "Plates");

            migrationBuilder.AddColumn<int>(
                name: "ArtistsId",
                table: "Plates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Plates_ArtistsId",
                table: "Plates",
                column: "ArtistsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Plates_Artists_ArtistsId",
                table: "Plates",
                column: "ArtistsId",
                principalTable: "Artists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

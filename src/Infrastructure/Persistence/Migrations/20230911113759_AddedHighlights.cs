using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    public partial class AddedHighlights : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Highlights",
                columns: table => new
                {
                    HighlightId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PageNumber = table.Column<int>(type: "int", nullable: false),
                    BookId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BookId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Highlights", x => x.HighlightId);
                    table.ForeignKey(
                        name: "FK_Highlights_Books_BookId1",
                        column: x => x.BookId1,
                        principalTable: "Books",
                        principalColumn: "BookId");
                });

            migrationBuilder.CreateTable(
                name: "RectF",
                columns: table => new
                {
                    RectFId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    X = table.Column<float>(type: "real", nullable: false),
                    Y = table.Column<float>(type: "real", nullable: false),
                    Width = table.Column<float>(type: "real", nullable: false),
                    Height = table.Column<float>(type: "real", nullable: false),
                    HighlightId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RectF", x => x.RectFId);
                    table.ForeignKey(
                        name: "FK_RectF_Highlights_HighlightId",
                        column: x => x.HighlightId,
                        principalTable: "Highlights",
                        principalColumn: "HighlightId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Highlights_BookId1",
                table: "Highlights",
                column: "BookId1");

            migrationBuilder.CreateIndex(
                name: "IX_RectF_HighlightId",
                table: "RectF",
                column: "HighlightId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RectF");

            migrationBuilder.DropTable(
                name: "Highlights");
        }
    }
}

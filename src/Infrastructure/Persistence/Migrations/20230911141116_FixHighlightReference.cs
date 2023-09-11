using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    public partial class FixHighlightReference : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Highlights_Books_BookId1",
                table: "Highlights");

            migrationBuilder.DropIndex(
                name: "IX_Highlights_BookId1",
                table: "Highlights");

            migrationBuilder.DropColumn(
                name: "BookId1",
                table: "Highlights");

            migrationBuilder.AlterColumn<Guid>(
                name: "BookId",
                table: "Highlights",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Highlights_BookId",
                table: "Highlights",
                column: "BookId");

            migrationBuilder.AddForeignKey(
                name: "FK_Highlights_Books_BookId",
                table: "Highlights",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "BookId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Highlights_Books_BookId",
                table: "Highlights");

            migrationBuilder.DropIndex(
                name: "IX_Highlights_BookId",
                table: "Highlights");

            migrationBuilder.AlterColumn<string>(
                name: "BookId",
                table: "Highlights",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "BookId1",
                table: "Highlights",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Highlights_BookId1",
                table: "Highlights",
                column: "BookId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Highlights_Books_BookId1",
                table: "Highlights",
                column: "BookId1",
                principalTable: "Books",
                principalColumn: "BookId");
        }
    }
}

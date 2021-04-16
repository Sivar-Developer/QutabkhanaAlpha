using Microsoft.EntityFrameworkCore.Migrations;

namespace WebAPI.Migrations
{
    public partial class StageSectiononetomanyrelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "StageId",
                table: "Section",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Section_StageId",
                table: "Section",
                column: "StageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Section_Stages_StageId",
                table: "Section",
                column: "StageId",
                principalTable: "Stages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Section_Stages_StageId",
                table: "Section");

            migrationBuilder.DropIndex(
                name: "IX_Section_StageId",
                table: "Section");

            migrationBuilder.DropColumn(
                name: "StageId",
                table: "Section");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}

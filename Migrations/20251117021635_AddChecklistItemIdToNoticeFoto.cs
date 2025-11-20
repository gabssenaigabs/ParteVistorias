using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CondoHub.Migrations
{
    /// <inheritdoc />
    public partial class AddChecklistItemIdToNoticeFoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FotoFile",
                table: "NoticeFotos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "ChecklistItemId",
                table: "NoticeFotos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notices_PropertyId",
                table: "Notices",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_NoticeFotos_ChecklistItemId",
                table: "NoticeFotos",
                column: "ChecklistItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_NoticeFotos_ChecklistItems_ChecklistItemId",
                table: "NoticeFotos",
                column: "ChecklistItemId",
                principalTable: "ChecklistItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notices_Properties_PropertyId",
                table: "Notices",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NoticeFotos_ChecklistItems_ChecklistItemId",
                table: "NoticeFotos");

            migrationBuilder.DropForeignKey(
                name: "FK_Notices_Properties_PropertyId",
                table: "Notices");

            migrationBuilder.DropIndex(
                name: "IX_Notices_PropertyId",
                table: "Notices");

            migrationBuilder.DropIndex(
                name: "IX_NoticeFotos_ChecklistItemId",
                table: "NoticeFotos");

            migrationBuilder.DropColumn(
                name: "ChecklistItemId",
                table: "NoticeFotos");

            migrationBuilder.AlterColumn<string>(
                name: "FotoFile",
                table: "NoticeFotos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}

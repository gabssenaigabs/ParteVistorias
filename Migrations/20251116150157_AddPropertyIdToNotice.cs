using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CondoHub.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyIdToNotice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PropertyId",
                table: "Notices",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PropertyId",
                table: "Notices");
        }
    }
}

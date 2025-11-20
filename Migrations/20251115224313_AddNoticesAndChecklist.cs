using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CondoHub.Migrations
{
    /// <inheritdoc />
    public partial class AddNoticesAndChecklist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitanteId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GestorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataConclusao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notices_AspNetUsers_GestorId",
                        column: x => x.GestorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notices_AspNetUsers_SolicitanteId",
                        column: x => x.SolicitanteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Categoria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    NoticeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistItems_Notices_NoticeId",
                        column: x => x.NoticeId,
                        principalTable: "Notices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NoticeFotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FotoFile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataUpload = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NoticeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoticeFotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NoticeFotos_Notices_NoticeId",
                        column: x => x.NoticeId,
                        principalTable: "Notices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItems_NoticeId",
                table: "ChecklistItems",
                column: "NoticeId");

            migrationBuilder.CreateIndex(
                name: "IX_NoticeFotos_NoticeId",
                table: "NoticeFotos",
                column: "NoticeId");

            migrationBuilder.CreateIndex(
                name: "IX_Notices_GestorId",
                table: "Notices",
                column: "GestorId");

            migrationBuilder.CreateIndex(
                name: "IX_Notices_SolicitanteId",
                table: "Notices",
                column: "SolicitanteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChecklistItems");

            migrationBuilder.DropTable(
                name: "NoticeFotos");

            migrationBuilder.DropTable(
                name: "Notices");
        }
    }
}

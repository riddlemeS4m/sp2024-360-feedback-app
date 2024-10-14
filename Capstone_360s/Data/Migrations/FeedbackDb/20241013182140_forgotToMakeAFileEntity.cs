using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone_360s.Data.Migrations.FeedbackDb
{
    /// <inheritdoc />
    public partial class forgotToMakeAFileEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FeedbackPdfId",
                table: "feedback",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "pdffiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FileName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentGDFolderId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GDFileId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProjectId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RoundId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pdffiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pdffiles_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_pdffiles_rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_pdffiles_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_feedback_FeedbackPdfId",
                table: "feedback",
                column: "FeedbackPdfId");

            migrationBuilder.CreateIndex(
                name: "IX_pdffiles_ProjectId",
                table: "pdffiles",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_pdffiles_RoundId",
                table: "pdffiles",
                column: "RoundId");

            migrationBuilder.CreateIndex(
                name: "IX_pdffiles_UserId",
                table: "pdffiles",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_feedback_pdffiles_FeedbackPdfId",
                table: "feedback",
                column: "FeedbackPdfId",
                principalTable: "pdffiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_feedback_pdffiles_FeedbackPdfId",
                table: "feedback");

            migrationBuilder.DropTable(
                name: "pdffiles");

            migrationBuilder.DropIndex(
                name: "IX_feedback_FeedbackPdfId",
                table: "feedback");

            migrationBuilder.DropColumn(
                name: "FeedbackPdfId",
                table: "feedback");
        }
    }
}

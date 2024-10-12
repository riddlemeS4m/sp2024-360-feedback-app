using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone_360s.Data.Migrations.FeedbackDb
{
    /// <inheritdoc />
    public partial class tweakedProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_projects_teams_TeamId",
                table: "projects");

            migrationBuilder.DropForeignKey(
                name: "FK_projects_users_POCId",
                table: "projects");

            migrationBuilder.AlterColumn<Guid>(
                name: "TeamId",
                table: "projects",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<Guid>(
                name: "POCId",
                table: "projects",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<string>(
                name: "GDFolderId",
                table: "projects",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_projects_teams_TeamId",
                table: "projects",
                column: "TeamId",
                principalTable: "teams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_projects_users_POCId",
                table: "projects",
                column: "POCId",
                principalTable: "users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_projects_teams_TeamId",
                table: "projects");

            migrationBuilder.DropForeignKey(
                name: "FK_projects_users_POCId",
                table: "projects");

            migrationBuilder.AlterColumn<Guid>(
                name: "TeamId",
                table: "projects",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<Guid>(
                name: "POCId",
                table: "projects",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.UpdateData(
                table: "projects",
                keyColumn: "GDFolderId",
                keyValue: null,
                column: "GDFolderId",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "GDFolderId",
                table: "projects",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_projects_teams_TeamId",
                table: "projects",
                column: "TeamId",
                principalTable: "teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_projects_users_POCId",
                table: "projects",
                column: "POCId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

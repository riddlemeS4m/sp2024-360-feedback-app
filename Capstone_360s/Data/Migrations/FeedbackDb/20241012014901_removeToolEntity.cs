using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone_360s.Data.Migrations.FeedbackDb
{
    /// <inheritdoc />
    public partial class removeToolEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_feedback_teams_TeamId",
                table: "feedback");

            migrationBuilder.DropForeignKey(
                name: "FK_projects_teams_TeamId",
                table: "projects");

            migrationBuilder.DropForeignKey(
                name: "FK_teams_users_ManagerId",
                table: "teams");

            migrationBuilder.DropIndex(
                name: "IX_projects_OrganizationId",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "IX_feedback_TeamId",
                table: "feedback");

            migrationBuilder.DropPrimaryKey(
                name: "PK_teams",
                table: "teams");

            migrationBuilder.DropIndex(
                name: "IX_teams_ManagerId",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "feedback");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "NoOfMembers",
                table: "teams");

            migrationBuilder.RenameTable(
                name: "teams",
                newName: "teammembers");

            migrationBuilder.RenameColumn(
                name: "TeamId",
                table: "projects",
                newName: "ManagerId");

            migrationBuilder.RenameIndex(
                name: "IX_projects_TeamId",
                table: "projects",
                newName: "IX_projects_ManagerId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "teammembers",
                newName: "UserId");

            migrationBuilder.AddColumn<string>(
                name: "Example",
                table: "questions",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "NoOfMembers",
                table: "projects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "teammembers",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddPrimaryKey(
                name: "PK_teammembers",
                table: "teammembers",
                columns: new[] { "ProjectId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_projects_Name",
                table: "projects",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_projects_OrganizationId_TimeframeId",
                table: "projects",
                columns: new[] { "OrganizationId", "TimeframeId" });

            migrationBuilder.CreateIndex(
                name: "IX_teammembers_UserId",
                table: "teammembers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_projects_users_ManagerId",
                table: "projects",
                column: "ManagerId",
                principalTable: "users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_teammembers_projects_ProjectId",
                table: "teammembers",
                column: "ProjectId",
                principalTable: "projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_teammembers_users_UserId",
                table: "teammembers",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_projects_users_ManagerId",
                table: "projects");

            migrationBuilder.DropForeignKey(
                name: "FK_teammembers_projects_ProjectId",
                table: "teammembers");

            migrationBuilder.DropForeignKey(
                name: "FK_teammembers_users_UserId",
                table: "teammembers");

            migrationBuilder.DropIndex(
                name: "IX_users_Email",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_projects_Name",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "IX_projects_OrganizationId_TimeframeId",
                table: "projects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_teammembers",
                table: "teammembers");

            migrationBuilder.DropIndex(
                name: "IX_teammembers_UserId",
                table: "teammembers");

            migrationBuilder.DropColumn(
                name: "Example",
                table: "questions");

            migrationBuilder.DropColumn(
                name: "NoOfMembers",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "teammembers");

            migrationBuilder.RenameTable(
                name: "teammembers",
                newName: "teams");

            migrationBuilder.RenameColumn(
                name: "ManagerId",
                table: "projects",
                newName: "TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_projects_ManagerId",
                table: "projects",
                newName: "IX_projects_TeamId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "teams",
                newName: "Id");

            migrationBuilder.AddColumn<Guid>(
                name: "TeamId",
                table: "feedback",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "ManagerId",
                table: "teams",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "teams",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "NoOfMembers",
                table: "teams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_teams",
                table: "teams",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_projects_OrganizationId",
                table: "projects",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_feedback_TeamId",
                table: "feedback",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_teams_ManagerId",
                table: "teams",
                column: "ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_feedback_teams_TeamId",
                table: "feedback",
                column: "TeamId",
                principalTable: "teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_projects_teams_TeamId",
                table: "projects",
                column: "TeamId",
                principalTable: "teams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_teams_users_ManagerId",
                table: "teams",
                column: "ManagerId",
                principalTable: "users",
                principalColumn: "Id");
        }
    }
}

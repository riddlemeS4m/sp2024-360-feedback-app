using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone_360s.Data.Migrations.FeedbackDb
{
    /// <inheritdoc />
    public partial class addedForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "projects",
                table: "projectrounds",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_users_OrganizationId",
                table: "users",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_timeframes_OrganizationId",
                table: "timeframes",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_teams_ManagerId",
                table: "teams",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_questionresponses_FeedbackId",
                table: "questionresponses",
                column: "FeedbackId");

            migrationBuilder.CreateIndex(
                name: "IX_projects_OrganizationId",
                table: "projects",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_projects_POCId",
                table: "projects",
                column: "POCId");

            migrationBuilder.CreateIndex(
                name: "IX_projects_TeamId",
                table: "projects",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_projects_TimeframeId",
                table: "projects",
                column: "TimeframeId");

            migrationBuilder.CreateIndex(
                name: "IX_projectrounds_projects",
                table: "projectrounds",
                column: "projects");

            migrationBuilder.CreateIndex(
                name: "IX_projectrounds_RoundId",
                table: "projectrounds",
                column: "RoundId");

            migrationBuilder.CreateIndex(
                name: "IX_metricresponses_FeedbackId",
                table: "metricresponses",
                column: "FeedbackId");

            migrationBuilder.CreateIndex(
                name: "IX_feedback_ProjectId",
                table: "feedback",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_feedback_RevieweeId",
                table: "feedback",
                column: "RevieweeId");

            migrationBuilder.CreateIndex(
                name: "IX_feedback_ReviewerId",
                table: "feedback",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_feedback_RoundId",
                table: "feedback",
                column: "RoundId");

            migrationBuilder.CreateIndex(
                name: "IX_feedback_TeamId",
                table: "feedback",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_feedback_TimeframeId",
                table: "feedback",
                column: "TimeframeId");

            migrationBuilder.AddForeignKey(
                name: "FK_feedback_projects_ProjectId",
                table: "feedback",
                column: "ProjectId",
                principalTable: "projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_feedback_rounds_RoundId",
                table: "feedback",
                column: "RoundId",
                principalTable: "rounds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_feedback_teams_TeamId",
                table: "feedback",
                column: "TeamId",
                principalTable: "teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_feedback_timeframes_TimeframeId",
                table: "feedback",
                column: "TimeframeId",
                principalTable: "timeframes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_feedback_users_RevieweeId",
                table: "feedback",
                column: "RevieweeId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_feedback_users_ReviewerId",
                table: "feedback",
                column: "ReviewerId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_metricresponses_feedback_FeedbackId",
                table: "metricresponses",
                column: "FeedbackId",
                principalTable: "feedback",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_metricresponses_metrics_MetricId",
                table: "metricresponses",
                column: "MetricId",
                principalTable: "metrics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_projectrounds_projects_projects",
                table: "projectrounds",
                column: "projects",
                principalTable: "projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_projectrounds_rounds_RoundId",
                table: "projectrounds",
                column: "RoundId",
                principalTable: "rounds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_projects_organizations_OrganizationId",
                table: "projects",
                column: "OrganizationId",
                principalTable: "organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_projects_teams_TeamId",
                table: "projects",
                column: "TeamId",
                principalTable: "teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_projects_timeframes_TimeframeId",
                table: "projects",
                column: "TimeframeId",
                principalTable: "timeframes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_projects_users_POCId",
                table: "projects",
                column: "POCId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_questionresponses_feedback_FeedbackId",
                table: "questionresponses",
                column: "FeedbackId",
                principalTable: "feedback",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_questionresponses_questions_QuestionId",
                table: "questionresponses",
                column: "QuestionId",
                principalTable: "questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_teams_users_ManagerId",
                table: "teams",
                column: "ManagerId",
                principalTable: "users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_timeframes_organizations_OrganizationId",
                table: "timeframes",
                column: "OrganizationId",
                principalTable: "organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_users_organizations_OrganizationId",
                table: "users",
                column: "OrganizationId",
                principalTable: "organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_feedback_projects_ProjectId",
                table: "feedback");

            migrationBuilder.DropForeignKey(
                name: "FK_feedback_rounds_RoundId",
                table: "feedback");

            migrationBuilder.DropForeignKey(
                name: "FK_feedback_teams_TeamId",
                table: "feedback");

            migrationBuilder.DropForeignKey(
                name: "FK_feedback_timeframes_TimeframeId",
                table: "feedback");

            migrationBuilder.DropForeignKey(
                name: "FK_feedback_users_RevieweeId",
                table: "feedback");

            migrationBuilder.DropForeignKey(
                name: "FK_feedback_users_ReviewerId",
                table: "feedback");

            migrationBuilder.DropForeignKey(
                name: "FK_metricresponses_feedback_FeedbackId",
                table: "metricresponses");

            migrationBuilder.DropForeignKey(
                name: "FK_metricresponses_metrics_MetricId",
                table: "metricresponses");

            migrationBuilder.DropForeignKey(
                name: "FK_projectrounds_projects_projects",
                table: "projectrounds");

            migrationBuilder.DropForeignKey(
                name: "FK_projectrounds_rounds_RoundId",
                table: "projectrounds");

            migrationBuilder.DropForeignKey(
                name: "FK_projects_organizations_OrganizationId",
                table: "projects");

            migrationBuilder.DropForeignKey(
                name: "FK_projects_teams_TeamId",
                table: "projects");

            migrationBuilder.DropForeignKey(
                name: "FK_projects_timeframes_TimeframeId",
                table: "projects");

            migrationBuilder.DropForeignKey(
                name: "FK_projects_users_POCId",
                table: "projects");

            migrationBuilder.DropForeignKey(
                name: "FK_questionresponses_feedback_FeedbackId",
                table: "questionresponses");

            migrationBuilder.DropForeignKey(
                name: "FK_questionresponses_questions_QuestionId",
                table: "questionresponses");

            migrationBuilder.DropForeignKey(
                name: "FK_teams_users_ManagerId",
                table: "teams");

            migrationBuilder.DropForeignKey(
                name: "FK_timeframes_organizations_OrganizationId",
                table: "timeframes");

            migrationBuilder.DropForeignKey(
                name: "FK_users_organizations_OrganizationId",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_OrganizationId",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_timeframes_OrganizationId",
                table: "timeframes");

            migrationBuilder.DropIndex(
                name: "IX_teams_ManagerId",
                table: "teams");

            migrationBuilder.DropIndex(
                name: "IX_questionresponses_FeedbackId",
                table: "questionresponses");

            migrationBuilder.DropIndex(
                name: "IX_projects_OrganizationId",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "IX_projects_POCId",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "IX_projects_TeamId",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "IX_projects_TimeframeId",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "IX_projectrounds_projects",
                table: "projectrounds");

            migrationBuilder.DropIndex(
                name: "IX_projectrounds_RoundId",
                table: "projectrounds");

            migrationBuilder.DropIndex(
                name: "IX_metricresponses_FeedbackId",
                table: "metricresponses");

            migrationBuilder.DropIndex(
                name: "IX_feedback_ProjectId",
                table: "feedback");

            migrationBuilder.DropIndex(
                name: "IX_feedback_RevieweeId",
                table: "feedback");

            migrationBuilder.DropIndex(
                name: "IX_feedback_ReviewerId",
                table: "feedback");

            migrationBuilder.DropIndex(
                name: "IX_feedback_RoundId",
                table: "feedback");

            migrationBuilder.DropIndex(
                name: "IX_feedback_TeamId",
                table: "feedback");

            migrationBuilder.DropIndex(
                name: "IX_feedback_TimeframeId",
                table: "feedback");

            migrationBuilder.DropColumn(
                name: "projects",
                table: "projectrounds");
        }
    }
}

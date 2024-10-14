using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone_360s.Data.Migrations.FeedbackDb
{
    /// <inheritdoc />
    public partial class addedProjectRoundReleaseDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "projectrounds",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReleaseDate",
                table: "projectrounds",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "projectrounds");

            migrationBuilder.DropColumn(
                name: "ReleaseDate",
                table: "projectrounds");
        }
    }
}

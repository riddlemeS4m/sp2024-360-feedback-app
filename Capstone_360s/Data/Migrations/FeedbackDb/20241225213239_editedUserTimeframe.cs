using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone_360s.Data.Migrations.FeedbackDb
{
    /// <inheritdoc />
    public partial class editedUserTimeframe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_usertimeframes_organizations_TimeframeId",
                table: "usertimeframes");

            migrationBuilder.AlterColumn<int>(
                name: "TimeframeId",
                table: "usertimeframes",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddForeignKey(
                name: "FK_usertimeframes_timeframes_TimeframeId",
                table: "usertimeframes",
                column: "TimeframeId",
                principalTable: "timeframes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_usertimeframes_timeframes_TimeframeId",
                table: "usertimeframes");

            migrationBuilder.AlterColumn<Guid>(
                name: "TimeframeId",
                table: "usertimeframes",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_usertimeframes_organizations_TimeframeId",
                table: "usertimeframes",
                column: "TimeframeId",
                principalTable: "organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

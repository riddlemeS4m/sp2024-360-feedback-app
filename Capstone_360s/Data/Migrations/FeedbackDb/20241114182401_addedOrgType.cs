using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone_360s.Data.Migrations.FeedbackDb
{
    /// <inheritdoc />
    public partial class addedOrgType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "organizations",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "organizations");
        }
    }
}

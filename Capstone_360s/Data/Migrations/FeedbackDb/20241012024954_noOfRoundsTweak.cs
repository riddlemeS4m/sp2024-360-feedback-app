using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone_360s.Data.Migrations.FeedbackDb
{
    /// <inheritdoc />
    public partial class noOfRoundsTweak : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NoOfRounds",
                table: "timeframes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoOfRounds",
                table: "timeframes");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assignment_1_Sample_Solution.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendeeUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Attendees",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Attendees");
        }
    }
}
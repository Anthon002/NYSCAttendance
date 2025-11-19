using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NYSCAttendance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class otp_status : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "OTPs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "OTPs");
        }
    }
}

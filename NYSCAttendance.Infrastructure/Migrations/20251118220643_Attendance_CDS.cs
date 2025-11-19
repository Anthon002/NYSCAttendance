using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NYSCAttendance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Attendance_CDS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CDS",
                table: "Attendances",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CDS",
                table: "Attendances");
        }
    }
}

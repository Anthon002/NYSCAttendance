using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NYSCAttendance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LGA_Open_Closing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CloseTime",
                table: "LGAs",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OpenTime",
                table: "LGAs",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CloseTime",
                table: "LGAs");

            migrationBuilder.DropColumn(
                name: "OpenTime",
                table: "LGAs");
        }
    }
}

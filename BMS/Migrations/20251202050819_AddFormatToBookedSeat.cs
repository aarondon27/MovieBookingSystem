using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BMS.Migrations
{
    /// <inheritdoc />
    public partial class AddFormatToBookedSeat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Format",
                table: "BookedSeats",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Format",
                table: "BookedSeats");
        }
    }
}

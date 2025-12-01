using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoT_Sensor_Monitoring_Web_App.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceReadingInterval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReadingIntervalSeconds",
                table: "Devices",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReadingIntervalSeconds",
                table: "Devices");
        }
    }
}

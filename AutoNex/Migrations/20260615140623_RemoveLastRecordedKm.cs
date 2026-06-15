using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoNex.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLastRecordedKm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_recorded_km",
                table: "mileage_alerts");

            migrationBuilder.AddColumn<int>(
                name: "DaysPerWeek",
                table: "service_orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedDailyKm",
                table: "service_orders",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaysPerWeek",
                table: "service_orders");

            migrationBuilder.DropColumn(
                name: "EstimatedDailyKm",
                table: "service_orders");

            migrationBuilder.AddColumn<int>(
                name: "last_recorded_km",
                table: "mileage_alerts",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}

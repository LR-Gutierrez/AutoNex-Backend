using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoNex.Migrations
{
    /// <inheritdoc />
    public partial class FixEstimatedKmColumnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EstimatedDailyKm",
                table: "service_orders",
                newName: "estimated_daily_km");

            migrationBuilder.RenameColumn(
                name: "DaysPerWeek",
                table: "service_orders",
                newName: "days_per_week");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "estimated_daily_km",
                table: "service_orders",
                newName: "EstimatedDailyKm");

            migrationBuilder.RenameColumn(
                name: "days_per_week",
                table: "service_orders",
                newName: "DaysPerWeek");
        }
    }
}

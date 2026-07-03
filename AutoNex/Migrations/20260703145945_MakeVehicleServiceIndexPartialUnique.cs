using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoNex.Migrations
{
    /// <inheritdoc />
    public partial class MakeVehicleServiceIndexPartialUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_mileage_alerts_vehicle_id_service_id",
                table: "mileage_alerts");

            migrationBuilder.CreateIndex(
                name: "IX_mileage_alerts_vehicle_id_service_id",
                table: "mileage_alerts",
                columns: new[] { "vehicle_id", "service_id" },
                unique: true,
                filter: "is_active = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_mileage_alerts_vehicle_id_service_id",
                table: "mileage_alerts");

            migrationBuilder.CreateIndex(
                name: "IX_mileage_alerts_vehicle_id_service_id",
                table: "mileage_alerts",
                columns: new[] { "vehicle_id", "service_id" },
                unique: true);
        }
    }
}

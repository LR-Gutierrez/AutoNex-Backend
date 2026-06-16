using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoNex.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceIdToMileageAlert : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_mileage_alerts_vehicle_id",
                table: "mileage_alerts");

            migrationBuilder.AddColumn<int>(
                name: "service_id",
                table: "mileage_alerts",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_mileage_alerts_service_id",
                table: "mileage_alerts",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "IX_mileage_alerts_vehicle_id_service_id",
                table: "mileage_alerts",
                columns: new[] { "vehicle_id", "service_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_mileage_alerts_services_service_id",
                table: "mileage_alerts",
                column: "service_id",
                principalTable: "services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mileage_alerts_services_service_id",
                table: "mileage_alerts");

            migrationBuilder.DropIndex(
                name: "IX_mileage_alerts_service_id",
                table: "mileage_alerts");

            migrationBuilder.DropIndex(
                name: "IX_mileage_alerts_vehicle_id_service_id",
                table: "mileage_alerts");

            migrationBuilder.DropColumn(
                name: "service_id",
                table: "mileage_alerts");

            migrationBuilder.CreateIndex(
                name: "IX_mileage_alerts_vehicle_id",
                table: "mileage_alerts",
                column: "vehicle_id");
        }
    }
}

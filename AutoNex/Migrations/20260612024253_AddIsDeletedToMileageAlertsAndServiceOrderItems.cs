using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoNex.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedToMileageAlertsAndServiceOrderItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "service_order_items",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "mileage_alerts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "service_order_items");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "mileage_alerts");
        }
    }
}

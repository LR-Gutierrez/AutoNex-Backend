using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutoNex.Migrations
{
    /// <inheritdoc />
    public partial class AddMileageAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RecommendedKmInterval",
                table: "services",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "mileage_alerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vehicle_id = table.Column<int>(type: "integer", nullable: false),
                    last_recorded_km = table.Column<int>(type: "integer", nullable: false),
                    estimated_weekly_km = table.Column<int>(type: "integer", nullable: false),
                    next_alert_km = table.Column<int>(type: "integer", nullable: false),
                    last_alert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mileage_alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_mileage_alerts_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_mileage_alerts_vehicle_id",
                table: "mileage_alerts",
                column: "vehicle_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mileage_alerts");

            migrationBuilder.DropColumn(
                name: "RecommendedKmInterval",
                table: "services");
        }
    }
}

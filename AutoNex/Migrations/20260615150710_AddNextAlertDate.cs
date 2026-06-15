using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoNex.Migrations
{
    /// <inheritdoc />
    public partial class AddNextAlertDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "next_alert_date",
                table: "mileage_alerts",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "next_alert_date",
                table: "mileage_alerts");
        }
    }
}

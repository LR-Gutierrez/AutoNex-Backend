using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoNex.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentFieldsToServiceOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "operation_date",
                table: "service_orders",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "operation_number",
                table: "service_orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_method",
                table: "service_orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "operation_date",
                table: "service_orders");

            migrationBuilder.DropColumn(
                name: "operation_number",
                table: "service_orders");

            migrationBuilder.DropColumn(
                name: "payment_method",
                table: "service_orders");
        }
    }
}

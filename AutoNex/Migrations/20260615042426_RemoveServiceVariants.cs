using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutoNex.Migrations
{
    /// <inheritdoc />
    public partial class RemoveServiceVariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_service_order_items_service_variants_service_variant_id",
                table: "service_order_items");

            migrationBuilder.DropTable(
                name: "service_variants");

            migrationBuilder.DropIndex(
                name: "IX_service_order_items_service_variant_id",
                table: "service_order_items");

            migrationBuilder.DropColumn(
                name: "service_variant_id",
                table: "service_order_items");

            migrationBuilder.AddColumn<int>(
                name: "recommended_months",
                table: "services",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "recommended_months",
                table: "services");

            migrationBuilder.AddColumn<int>(
                name: "service_variant_id",
                table: "service_order_items",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "service_variants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    service_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    max_km_interval = table.Column<int>(type: "integer", nullable: false),
                    min_km_interval = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    recommended_months = table.Column<int>(type: "integer", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_variants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_service_variants_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_service_order_items_service_variant_id",
                table: "service_order_items",
                column: "service_variant_id");

            migrationBuilder.CreateIndex(
                name: "IX_service_variants_service_id",
                table: "service_variants",
                column: "service_id");

            migrationBuilder.AddForeignKey(
                name: "FK_service_order_items_service_variants_service_variant_id",
                table: "service_order_items",
                column: "service_variant_id",
                principalTable: "service_variants",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}

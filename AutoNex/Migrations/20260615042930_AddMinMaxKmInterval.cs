using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoNex.Migrations
{
    /// <inheritdoc />
    public partial class AddMinMaxKmInterval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RecommendedKmInterval",
                table: "services",
                newName: "min_km_interval");

            migrationBuilder.AddColumn<int>(
                name: "max_km_interval",
                table: "services",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("UPDATE services SET max_km_interval = min_km_interval WHERE min_km_interval IS NOT NULL");
            migrationBuilder.Sql("UPDATE services SET min_km_interval = 0 WHERE min_km_interval IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE services SET min_km_interval = max_km_interval WHERE max_km_interval IS NOT NULL");

            migrationBuilder.DropColumn(
                name: "max_km_interval",
                table: "services");

            migrationBuilder.RenameColumn(
                name: "min_km_interval",
                table: "services",
                newName: "RecommendedKmInterval");
        }
    }
}

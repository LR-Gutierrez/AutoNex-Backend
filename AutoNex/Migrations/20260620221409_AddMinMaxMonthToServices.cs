using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoNex.Migrations
{
    /// <inheritdoc />
    public partial class AddMinMaxMonthToServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "recommended_months",
                table: "services",
                newName: "min_month");

            migrationBuilder.AddColumn<int>(
                name: "max_month",
                table: "services",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "max_month",
                table: "services");

            migrationBuilder.RenameColumn(
                name: "min_month",
                table: "services",
                newName: "recommended_months");
        }
    }
}

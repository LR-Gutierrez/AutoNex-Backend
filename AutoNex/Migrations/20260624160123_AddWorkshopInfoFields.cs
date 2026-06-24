using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoNex.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkshopInfoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "city",
                table: "workshop_info",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "maps_url",
                table: "workshop_info",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "opening_hours",
                table: "workshop_info",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rif",
                table: "workshop_info",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "website",
                table: "workshop_info",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "city",
                table: "workshop_info");

            migrationBuilder.DropColumn(
                name: "maps_url",
                table: "workshop_info");

            migrationBuilder.DropColumn(
                name: "opening_hours",
                table: "workshop_info");

            migrationBuilder.DropColumn(
                name: "rif",
                table: "workshop_info");

            migrationBuilder.DropColumn(
                name: "website",
                table: "workshop_info");
        }
    }
}

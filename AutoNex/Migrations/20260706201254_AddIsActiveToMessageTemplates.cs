using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoNex.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToMessageTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "message_templates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_message_templates_is_active",
                table: "message_templates",
                column: "is_active",
                unique: true,
                filter: "\"is_active\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_message_templates_is_active",
                table: "message_templates");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "message_templates");
        }
    }
}

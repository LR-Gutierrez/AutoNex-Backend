using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoNex.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeToRecurringExpense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "recurring_expenses",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type",
                table: "recurring_expenses");
        }
    }
}

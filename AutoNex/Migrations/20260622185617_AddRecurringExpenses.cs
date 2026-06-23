using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutoNex.Migrations
{
    /// <inheritdoc />
    public partial class AddRecurringExpenses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recurring_expenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    frequency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    day_of_month = table.Column<int>(type: "integer", nullable: false),
                    account_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recurring_expenses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recurring_expense_occurrences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    recurring_expense_id = table.Column<int>(type: "integer", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    is_paid = table.Column<bool>(type: "boolean", nullable: false),
                    paid_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    paid_account_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    financial_record_id = table.Column<int>(type: "integer", nullable: true),
                    dismissed_date = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recurring_expense_occurrences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recurring_expense_occurrences_financial_records_financial_r~",
                        column: x => x.financial_record_id,
                        principalTable: "financial_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_recurring_expense_occurrences_recurring_expenses_recurring_~",
                        column: x => x.recurring_expense_id,
                        principalTable: "recurring_expenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recurring_expense_occurrences_financial_record_id",
                table: "recurring_expense_occurrences",
                column: "financial_record_id");

            migrationBuilder.CreateIndex(
                name: "IX_recurring_expense_occurrences_recurring_expense_id",
                table: "recurring_expense_occurrences",
                column: "recurring_expense_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recurring_expense_occurrences");

            migrationBuilder.DropTable(
                name: "recurring_expenses");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoNex.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceSuccessWithStatusInMessageLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "whatsapp_message_logs",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.Sql("UPDATE whatsapp_message_logs SET status = CASE WHEN success THEN 'Sent' ELSE 'Failed' END");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "whatsapp_message_logs",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Sending");

            migrationBuilder.DropColumn(
                name: "success",
                table: "whatsapp_message_logs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "success",
                table: "whatsapp_message_logs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE whatsapp_message_logs SET success = CASE WHEN status = 'Sent' THEN TRUE ELSE FALSE END");

            migrationBuilder.DropColumn(
                name: "status",
                table: "whatsapp_message_logs");
        }
    }
}

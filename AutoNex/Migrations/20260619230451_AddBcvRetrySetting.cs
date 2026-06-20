using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoNex.Migrations
{
    /// <inheritdoc />
    public partial class AddBcvRetrySetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO settings ("Key", "Value", "Type", "Description", "IsActive", "CreatedAt", "UpdatedAt")
                VALUES ('bcv_retry_enabled', 'true', 'boolean', 'Activa los reintentos automáticos cada 10 min si el BCV no ha publicado', true, NOW(), NOW())
                ON CONFLICT ("Key") DO NOTHING;
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM settings WHERE "Key" = 'bcv_retry_enabled';
            """);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shortener.Migrator.Migrations;
/// <inheritdoc />
public partial class AddApiKeyWebhookTables : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "api_keys",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                key_prefix = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                key_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                scopes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                is_revoked = table.Column<bool>(type: "boolean", nullable: false),
                last_used_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_api_keys", x => x.id);
                table.ForeignKey(
                    name: "FK_api_keys_tenants_tenant_id",
                    column: x => x.tenant_id,
                    principalTable: "tenants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "webhooks",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                secret = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                events = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_webhooks", x => x.id);
                table.ForeignKey(
                    name: "FK_webhooks_tenants_tenant_id",
                    column: x => x.tenant_id,
                    principalTable: "tenants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "webhook_deliveries",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                webhook_id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                event_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                payload = table.Column<string>(type: "text", nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                attempt_count = table.Column<int>(type: "integer", nullable: false),
                next_attempt_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                delivered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                last_response_body = table.Column<string>(type: "text", nullable: true),
                last_http_status = table.Column<int>(type: "integer", nullable: true),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_webhook_deliveries", x => x.id);
                table.ForeignKey(
                    name: "FK_webhook_deliveries_webhooks_webhook_id",
                    column: x => x.webhook_id,
                    principalTable: "webhooks",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_api_keys_prefix",
            table: "api_keys",
            column: "key_prefix");

        migrationBuilder.CreateIndex(
            name: "ix_api_keys_tenant_id",
            table: "api_keys",
            column: "tenant_id");

        migrationBuilder.CreateIndex(
            name: "ix_webhook_deliveries_pending",
            table: "webhook_deliveries",
            columns: new[] { "status", "next_attempt_at" },
            filter: "status = 'Pending'");

        migrationBuilder.CreateIndex(
            name: "ix_webhook_deliveries_webhook_id",
            table: "webhook_deliveries",
            column: "webhook_id");

        migrationBuilder.CreateIndex(
            name: "ix_webhooks_tenant_id",
            table: "webhooks",
            column: "tenant_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "api_keys");

        migrationBuilder.DropTable(
            name: "webhook_deliveries");

        migrationBuilder.DropTable(
            name: "webhooks");
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shortener.Migrator.Migrations;
/// <inheritdoc />
public partial class InitialSchema : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "tenants",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_tenants", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "domains",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                host = table.Column<string>(type: "character varying(253)", maxLength: 253, nullable: false),
                normalized_host = table.Column<string>(type: "character varying(253)", maxLength: 253, nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                is_default = table.Column<bool>(type: "boolean", nullable: false),
                verification_token = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_domains", x => x.id);
                table.ForeignKey(
                    name: "FK_domains_tenants_tenant_id",
                    column: x => x.tenant_id,
                    principalTable: "tenants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "short_links",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                domain_id = table.Column<Guid>(type: "uuid", nullable: false),
                short_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                destination_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: true),
                updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                last_accessed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                redirect_type = table.Column<int>(type: "integer", nullable: false),
                is_anonymous = table.Column<bool>(type: "boolean", nullable: false),
                click_count_snapshot = table.Column<long>(type: "bigint", nullable: false),
                version = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_short_links", x => x.id);
                table.ForeignKey(
                    name: "FK_short_links_domains_domain_id",
                    column: x => x.domain_id,
                    principalTable: "domains",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_domains_normalized_host",
            table: "domains",
            column: "normalized_host",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_domains_tenant_id",
            table: "domains",
            column: "tenant_id");

        migrationBuilder.CreateIndex(
            name: "ix_short_links_domain_code",
            table: "short_links",
            columns: new[] { "domain_id", "short_code" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_short_links_expires_at_active",
            table: "short_links",
            column: "expires_at",
            filter: "expires_at IS NOT NULL AND status = 'Active'");

        migrationBuilder.CreateIndex(
            name: "ix_short_links_tenant_status_created",
            table: "short_links",
            columns: new[] { "tenant_id", "status", "created_at_utc" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "short_links");

        migrationBuilder.DropTable(
            name: "domains");

        migrationBuilder.DropTable(
            name: "tenants");
    }
}

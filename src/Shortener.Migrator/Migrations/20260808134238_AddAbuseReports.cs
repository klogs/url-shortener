using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shortener.Migrator.Migrations;
/// <inheritdoc />
public partial class AddAbuseReports : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "report_count",
            table: "short_links",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateTable(
            name: "abuse_reports",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                link_id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                short_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                normalized_host = table.Column<string>(type: "character varying(253)", maxLength: 253, nullable: false),
                reporter_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_abuse_reports", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_abuse_reports_created_at",
            table: "abuse_reports",
            column: "created_at_utc");

        migrationBuilder.CreateIndex(
            name: "ix_abuse_reports_link_id",
            table: "abuse_reports",
            column: "link_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "abuse_reports");

        migrationBuilder.DropColumn(
            name: "report_count",
            table: "short_links");
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shortener.Migrator.Migrations;
/// <inheritdoc />
public partial class AddLinkVariants : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "is_ab_test",
            table: "short_links",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateTable(
            name: "link_variants",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                link_id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                destination_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                weight = table.Column<int>(type: "integer", nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_link_variants", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_link_variants_link_id",
            table: "link_variants",
            column: "link_id");

        migrationBuilder.CreateIndex(
            name: "ix_link_variants_tenant_link",
            table: "link_variants",
            columns: new[] { "tenant_id", "link_id" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "link_variants");

        migrationBuilder.DropColumn(
            name: "is_ab_test",
            table: "short_links");
    }
}

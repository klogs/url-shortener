using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shortener.Migrator.Migrations;

/// <inheritdoc />
public partial class UseXminForShortLinkConcurrency : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // The "version" column was a spurious xid column that EF Core excluded from
        // INSERTs (ValueGeneratedOnAddOrUpdate), causing NOT NULL violations.
        // Optimistic concurrency is now handled by PostgreSQL's built-in xmin system column,
        // which requires no physical column in the table.
        migrationBuilder.DropColumn(
            name: "version",
            table: "short_links");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<uint>(
            name: "version",
            table: "short_links",
            type: "xid",
            rowVersion: true,
            nullable: false,
            defaultValueSql: "0::xid");
    }
}

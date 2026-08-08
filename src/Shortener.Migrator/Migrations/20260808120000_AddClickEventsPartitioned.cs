using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shortener.Migrator.Migrations;

public partial class AddClickEventsPartitioned : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Partitioned base table — EF Core does not model this, managed as raw SQL
        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS click_events (
                id            BIGINT GENERATED ALWAYS AS IDENTITY,
                link_id       UUID            NOT NULL,
                tenant_id     UUID            NOT NULL,
                short_code    VARCHAR(100)    NOT NULL,
                normalized_host VARCHAR(253)  NOT NULL,
                occurred_at_utc TIMESTAMPTZ   NOT NULL,
                user_agent    TEXT,
                remote_ip     VARCHAR(45),
                referer       TEXT,
                country       CHAR(2),
                PRIMARY KEY (id, occurred_at_utc)
            ) PARTITION BY RANGE (occurred_at_utc);
            """);

        // Index on link_id within each partition (ONLY — child partitions inherit)
        migrationBuilder.Sql("""
            CREATE INDEX ix_click_events_link_id ON click_events (link_id);
            """);

        migrationBuilder.Sql("""
            CREATE INDEX ix_click_events_tenant_occurred ON click_events (tenant_id, occurred_at_utc DESC);
            """);

        // Initial partition for 2026 — add a monthly partition management procedure in a later migration
        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS click_events_2026
                PARTITION OF click_events
                FOR VALUES FROM ('2026-01-01') TO ('2027-01-01');
            """);

        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS click_events_2027
                PARTITION OF click_events
                FOR VALUES FROM ('2027-01-01') TO ('2028-01-01');
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP TABLE IF EXISTS click_events CASCADE;");
    }
}

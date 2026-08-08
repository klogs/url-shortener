using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shortener.Migrator.Migrations;
    /// <inheritdoc />
    public partial class AddGeoRoutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "has_geo_routes",
                table: "short_links",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "geo_routes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    link_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    destination_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_geo_routes", x => x.id);
                    table.ForeignKey(
                        name: "FK_geo_routes_short_links_link_id",
                        column: x => x.link_id,
                        principalTable: "short_links",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_geo_routes_link_country_unique",
                table: "geo_routes",
                columns: new[] { "link_id", "country_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_geo_routes_link_id",
                table: "geo_routes",
                column: "link_id");

            migrationBuilder.CreateIndex(
                name: "ix_geo_routes_tenant_link",
                table: "geo_routes",
                columns: new[] { "tenant_id", "link_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "geo_routes");

            migrationBuilder.DropColumn(
                name: "has_geo_routes",
                table: "short_links");
        }
    }


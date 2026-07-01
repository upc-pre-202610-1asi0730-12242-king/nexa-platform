using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace King.Nexa.Platform.Migrations
{
    /// <inheritdoc />
    public partial class NormalizePromotionsPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "adjustment_type",
                table: "promotions",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "catalog_scope",
                table: "promotions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "commercial_rule",
                table: "promotions",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "promotions",
                type: "character varying(800)",
                maxLength: 800,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "promotions",
                type: "character varying(800)",
                maxLength: 800,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "target_segment",
                table: "promotions",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "visibility",
                table: "promotions",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "promotion_catalog_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    promotion_id = table.Column<int>(type: "integer", nullable: false),
                    catalog_item_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_promotion_catalog_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_promotion_catalog_items_catalog_items_catalog_item_id",
                        column: x => x.catalog_item_id,
                        principalTable: "catalog_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_promotion_catalog_items_promotions_promotion_id",
                        column: x => x.promotion_id,
                        principalTable: "promotions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_promotion_catalog_items_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_promotion_catalog_items_catalog_item_id",
                table: "promotion_catalog_items",
                column: "catalog_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_promotion_catalog_items_promotion_id",
                table: "promotion_catalog_items",
                column: "promotion_id");

            migrationBuilder.CreateIndex(
                name: "ix_promotion_catalog_items_tenant_id_promotion_id_catalog_item~",
                table: "promotion_catalog_items",
                columns: new[] { "tenant_id", "promotion_id", "catalog_item_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "promotion_catalog_items");

            migrationBuilder.DropColumn(
                name: "adjustment_type",
                table: "promotions");

            migrationBuilder.DropColumn(
                name: "catalog_scope",
                table: "promotions");

            migrationBuilder.DropColumn(
                name: "commercial_rule",
                table: "promotions");

            migrationBuilder.DropColumn(
                name: "description",
                table: "promotions");

            migrationBuilder.DropColumn(
                name: "notes",
                table: "promotions");

            migrationBuilder.DropColumn(
                name: "target_segment",
                table: "promotions");

            migrationBuilder.DropColumn(
                name: "visibility",
                table: "promotions");
        }
    }
}

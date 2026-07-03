using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace King.Nexa.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantAndClientAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "client_accounts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    business_name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    commercial_name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    ruc = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    segment = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    contact = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    contact_email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    payment_condition = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    monthly_credit_limit = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    monthly_credit_used = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    monthly_credit_status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    delivery_preference = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    portal_access = table.Column<bool>(type: "boolean", nullable: false),
                    seller_workspace_email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_client_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    legal_name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    slug = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ruc = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    workspace_url = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    plan = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    country = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenants", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_client_accounts_code",
                table: "client_accounts",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_client_accounts_ruc",
                table: "client_accounts",
                column: "ruc");

            migrationBuilder.CreateIndex(
                name: "ix_tenants_ruc",
                table: "tenants",
                column: "ruc");

            migrationBuilder.CreateIndex(
                name: "ix_tenants_slug",
                table: "tenants",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "client_accounts");

            migrationBuilder.DropTable(
                name: "tenants");
        }
    }
}

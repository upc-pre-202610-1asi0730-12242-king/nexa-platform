using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace King.Nexa.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoicingAuditAndTenantScopedInvoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_invoices_invoice_number",
                table: "invoices");

            migrationBuilder.AddColumn<int>(
                name: "tenant_id",
                table: "invoices",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    workspace_id = table.Column<int>(type: "integer", nullable: true),
                    actor_user_id = table.Column<int>(type: "integer", nullable: false),
                    actor_membership_id = table.Column<int>(type: "integer", nullable: true),
                    action = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    resource_type = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    resource_id = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: true),
                    ip_address = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(360)", maxLength: 360, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_audit_logs_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_invoices_tenant_id_invoice_number",
                table: "invoices",
                columns: new[] { "tenant_id", "invoice_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_invoices_tenant_id_payment_status",
                table: "invoices",
                columns: new[] { "tenant_id", "payment_status" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_tenant_id_action_created_at",
                table: "audit_logs",
                columns: new[] { "tenant_id", "action", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_tenant_id_resource_type_resource_id",
                table: "audit_logs",
                columns: new[] { "tenant_id", "resource_type", "resource_id" });

            migrationBuilder.AddForeignKey(
                name: "fk_invoices_tenants_tenant_id",
                table: "invoices",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_invoices_tenants_tenant_id",
                table: "invoices");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropIndex(
                name: "ix_invoices_tenant_id_invoice_number",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "ix_invoices_tenant_id_payment_status",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "invoices");

            migrationBuilder.CreateIndex(
                name: "ix_invoices_invoice_number",
                table: "invoices",
                column: "invoice_number",
                unique: true);
        }
    }
}

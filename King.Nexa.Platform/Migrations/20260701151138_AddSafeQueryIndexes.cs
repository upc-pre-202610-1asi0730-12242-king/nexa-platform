using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace King.Nexa.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddSafeQueryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_purchase_requests_tenant_id_status_created_at",
                table: "purchase_requests",
                columns: new[] { "tenant_id", "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_payments_tenant_id_status_created_at",
                table: "payments",
                columns: new[] { "tenant_id", "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_orders_tenant_id_status_created_at",
                table: "orders",
                columns: new[] { "tenant_id", "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_invoices_tenant_id_payment_status_created_at",
                table: "invoices",
                columns: new[] { "tenant_id", "payment_status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_dispatch_orders_tenant_id_status_created_at",
                table: "dispatch_orders",
                columns: new[] { "tenant_id", "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_catalog_items_tenant_id_brand_name",
                table: "catalog_items",
                columns: new[] { "tenant_id", "brand_name" });

            migrationBuilder.CreateIndex(
                name: "ix_catalog_items_tenant_id_category_name",
                table: "catalog_items",
                columns: new[] { "tenant_id", "category_name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_purchase_requests_tenant_id_status_created_at",
                table: "purchase_requests");

            migrationBuilder.DropIndex(
                name: "ix_payments_tenant_id_status_created_at",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_orders_tenant_id_status_created_at",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "ix_invoices_tenant_id_payment_status_created_at",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "ix_dispatch_orders_tenant_id_status_created_at",
                table: "dispatch_orders");

            migrationBuilder.DropIndex(
                name: "ix_catalog_items_tenant_id_brand_name",
                table: "catalog_items");

            migrationBuilder.DropIndex(
                name: "ix_catalog_items_tenant_id_category_name",
                table: "catalog_items");
        }
    }
}

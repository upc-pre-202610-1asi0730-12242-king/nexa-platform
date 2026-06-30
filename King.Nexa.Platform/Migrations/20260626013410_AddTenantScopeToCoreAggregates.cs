using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace King.Nexa.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantScopeToCoreAggregates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_warehouses_location",
                table: "warehouses");

            migrationBuilder.DropIndex(
                name: "ix_shipments_shipment_code",
                table: "shipments");

            migrationBuilder.DropIndex(
                name: "ix_purchase_requests_code",
                table: "purchase_requests");

            migrationBuilder.DropIndex(
                name: "ix_orders_order_number",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "ix_inventory_items_catalog_item_id",
                table: "inventory_items");

            migrationBuilder.DropIndex(
                name: "ix_dispatch_orders_code",
                table: "dispatch_orders");

            migrationBuilder.DropIndex(
                name: "ix_catalog_items_catalog_item_id",
                table: "catalog_items");

            migrationBuilder.AddColumn<int>(
                name: "tenant_id",
                table: "warehouses",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "tenant_id",
                table: "shipments",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "tenant_id",
                table: "orders",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "tenant_id",
                table: "order_items",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "tenant_id",
                table: "inventory_items",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "tenant_id",
                table: "catalog_items",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "ix_warehouses_tenant_id_location",
                table: "warehouses",
                columns: new[] { "tenant_id", "location" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shipments_tenant_id_shipment_code",
                table: "shipments",
                columns: new[] { "tenant_id", "shipment_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_purchase_requests_tenant_id_code",
                table: "purchase_requests",
                columns: new[] { "tenant_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_orders_tenant_id_order_number",
                table: "orders",
                columns: new[] { "tenant_id", "order_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_order_items_tenant_id_order_id",
                table: "order_items",
                columns: new[] { "tenant_id", "order_id" });

            migrationBuilder.CreateIndex(
                name: "ix_inventory_items_tenant_id_catalog_item_id",
                table: "inventory_items",
                columns: new[] { "tenant_id", "catalog_item_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_inventory_items_tenant_id_product_id",
                table: "inventory_items",
                columns: new[] { "tenant_id", "product_id" });

            migrationBuilder.CreateIndex(
                name: "ix_dispatch_orders_tenant_id_code",
                table: "dispatch_orders",
                columns: new[] { "tenant_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_catalog_items_tenant_id_catalog_item_id",
                table: "catalog_items",
                columns: new[] { "tenant_id", "catalog_item_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_catalog_items_tenant_id_product_id",
                table: "catalog_items",
                columns: new[] { "tenant_id", "product_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_catalog_items_tenants_tenant_id",
                table: "catalog_items",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_inventory_items_tenants_tenant_id",
                table: "inventory_items",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_order_items_tenants_tenant_id",
                table: "order_items",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_orders_tenants_tenant_id",
                table: "orders",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_shipments_tenants_tenant_id",
                table: "shipments",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_warehouses_tenants_tenant_id",
                table: "warehouses",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_catalog_items_tenants_tenant_id",
                table: "catalog_items");

            migrationBuilder.DropForeignKey(
                name: "fk_inventory_items_tenants_tenant_id",
                table: "inventory_items");

            migrationBuilder.DropForeignKey(
                name: "fk_order_items_tenants_tenant_id",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "fk_orders_tenants_tenant_id",
                table: "orders");

            migrationBuilder.DropForeignKey(
                name: "fk_shipments_tenants_tenant_id",
                table: "shipments");

            migrationBuilder.DropForeignKey(
                name: "fk_warehouses_tenants_tenant_id",
                table: "warehouses");

            migrationBuilder.DropIndex(
                name: "ix_warehouses_tenant_id_location",
                table: "warehouses");

            migrationBuilder.DropIndex(
                name: "ix_shipments_tenant_id_shipment_code",
                table: "shipments");

            migrationBuilder.DropIndex(
                name: "ix_purchase_requests_tenant_id_code",
                table: "purchase_requests");

            migrationBuilder.DropIndex(
                name: "ix_orders_tenant_id_order_number",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "ix_order_items_tenant_id_order_id",
                table: "order_items");

            migrationBuilder.DropIndex(
                name: "ix_inventory_items_tenant_id_catalog_item_id",
                table: "inventory_items");

            migrationBuilder.DropIndex(
                name: "ix_inventory_items_tenant_id_product_id",
                table: "inventory_items");

            migrationBuilder.DropIndex(
                name: "ix_dispatch_orders_tenant_id_code",
                table: "dispatch_orders");

            migrationBuilder.DropIndex(
                name: "ix_catalog_items_tenant_id_catalog_item_id",
                table: "catalog_items");

            migrationBuilder.DropIndex(
                name: "ix_catalog_items_tenant_id_product_id",
                table: "catalog_items");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "warehouses");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "shipments");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "inventory_items");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "catalog_items");

            migrationBuilder.CreateIndex(
                name: "ix_warehouses_location",
                table: "warehouses",
                column: "location",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shipments_shipment_code",
                table: "shipments",
                column: "shipment_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_purchase_requests_code",
                table: "purchase_requests",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_orders_order_number",
                table: "orders",
                column: "order_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_inventory_items_catalog_item_id",
                table: "inventory_items",
                column: "catalog_item_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_dispatch_orders_code",
                table: "dispatch_orders",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_catalog_items_catalog_item_id",
                table: "catalog_items",
                column: "catalog_item_id",
                unique: true);
        }
    }
}

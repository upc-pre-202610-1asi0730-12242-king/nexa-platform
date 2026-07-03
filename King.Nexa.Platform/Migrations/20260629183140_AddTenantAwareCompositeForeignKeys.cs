using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace King.Nexa.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantAwareCompositeForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_business_documents_client_accounts_client_account_id",
                table: "business_documents");

            migrationBuilder.DropForeignKey(
                name: "fk_business_documents_orders_order_id",
                table: "business_documents");

            migrationBuilder.DropForeignKey(
                name: "fk_conversation_messages_client_accounts_client_account_id",
                table: "conversation_messages");

            migrationBuilder.DropForeignKey(
                name: "fk_conversation_messages_orders_order_id",
                table: "conversation_messages");

            migrationBuilder.DropForeignKey(
                name: "fk_conversation_messages_purchase_requests_purchase_request_id",
                table: "conversation_messages");

            migrationBuilder.DropForeignKey(
                name: "fk_credit_requests_client_accounts_client_account_id",
                table: "credit_requests");

            migrationBuilder.DropForeignKey(
                name: "fk_customer_portal_tasks_client_accounts_client_account_id",
                table: "customer_portal_tasks");

            migrationBuilder.DropForeignKey(
                name: "fk_dispatch_events_dispatch_orders_dispatch_order_id",
                table: "dispatch_events");

            migrationBuilder.DropForeignKey(
                name: "fk_dispatch_orders_client_accounts_client_account_id",
                table: "dispatch_orders");

            migrationBuilder.DropForeignKey(
                name: "fk_dispatch_orders_orders_order_id",
                table: "dispatch_orders");

            migrationBuilder.DropForeignKey(
                name: "fk_inventory_lots_inventory_items_inventory_item_id",
                table: "inventory_lots");

            migrationBuilder.DropForeignKey(
                name: "fk_inventory_lots_warehouses_warehouse_id",
                table: "inventory_lots");

            migrationBuilder.DropForeignKey(
                name: "fk_inventory_movements_inventory_items_inventory_item_id",
                table: "inventory_movements");

            migrationBuilder.DropForeignKey(
                name: "fk_inventory_movements_inventory_lots_inventory_lot_id",
                table: "inventory_movements");

            migrationBuilder.DropForeignKey(
                name: "fk_inventory_movements_orders_order_id",
                table: "inventory_movements");

            migrationBuilder.DropForeignKey(
                name: "fk_inventory_movements_warehouses_warehouse_id",
                table: "inventory_movements");

            migrationBuilder.DropForeignKey(
                name: "fk_inventory_reservation_records_inventory_items_inventory_ite~",
                table: "inventory_reservation_records");

            migrationBuilder.DropForeignKey(
                name: "fk_inventory_reservation_records_inventory_lots_inventory_lot_~",
                table: "inventory_reservation_records");

            migrationBuilder.DropForeignKey(
                name: "fk_inventory_reservation_records_orders_order_id",
                table: "inventory_reservation_records");

            migrationBuilder.DropForeignKey(
                name: "fk_inventory_reservation_records_purchase_requests_purchase_re~",
                table: "inventory_reservation_records");

            migrationBuilder.DropForeignKey(
                name: "fk_notification_records_client_accounts_client_account_id",
                table: "notification_records");

            migrationBuilder.DropForeignKey(
                name: "fk_order_items_orders_order_id",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "fk_orders_client_accounts_client_account_id",
                table: "orders");

            migrationBuilder.DropForeignKey(
                name: "fk_payment_method_records_client_accounts_client_account_id",
                table: "payment_method_records");

            migrationBuilder.DropForeignKey(
                name: "fk_payment_process_records_client_accounts_client_account_id",
                table: "payment_process_records");

            migrationBuilder.DropForeignKey(
                name: "fk_payment_process_records_orders_order_id",
                table: "payment_process_records");

            migrationBuilder.DropForeignKey(
                name: "fk_payment_process_records_payment_method_records_payment_meth~",
                table: "payment_process_records");

            migrationBuilder.DropForeignKey(
                name: "fk_payment_process_records_payments_payment_id",
                table: "payment_process_records");

            migrationBuilder.DropForeignKey(
                name: "fk_payments_client_accounts_client_account_id",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "fk_payments_invoices_invoice_id",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "fk_payments_orders_order_id",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "fk_payments_payment_method_records_payment_method_record_id",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "fk_promotion_catalog_items_catalog_items_catalog_item_id",
                table: "promotion_catalog_items");

            migrationBuilder.DropForeignKey(
                name: "fk_promotion_catalog_items_promotions_promotion_id",
                table: "promotion_catalog_items");

            migrationBuilder.DropForeignKey(
                name: "fk_proof_of_delivery_records_dispatch_orders_dispatch_order_id",
                table: "proof_of_delivery_records");

            migrationBuilder.DropForeignKey(
                name: "fk_purchase_request_lines_catalog_items_catalog_item_id",
                table: "purchase_request_lines");

            migrationBuilder.DropForeignKey(
                name: "fk_purchase_request_lines_purchase_requests_purchase_request_id",
                table: "purchase_request_lines");

            migrationBuilder.DropForeignKey(
                name: "fk_purchase_requests_client_accounts_client_account_id",
                table: "purchase_requests");

            migrationBuilder.DropForeignKey(
                name: "fk_temperature_logs_dispatch_orders_dispatch_order_id",
                table: "temperature_logs");

            migrationBuilder.DropForeignKey(
                name: "fk_temperature_logs_orders_order_id",
                table: "temperature_logs");

            migrationBuilder.DropForeignKey(
                name: "fk_user_workspace_memberships_workspaces_workspace_id",
                table: "user_workspace_memberships");

            migrationBuilder.DropForeignKey(
                name: "fk_workspace_preferences_workspaces_workspace_id",
                table: "workspace_preferences");

            migrationBuilder.DropIndex(
                name: "ix_workspace_preferences_workspace_id",
                table: "workspace_preferences");

            migrationBuilder.DropIndex(
                name: "ix_temperature_logs_dispatch_order_id",
                table: "temperature_logs");

            migrationBuilder.DropIndex(
                name: "ix_temperature_logs_order_id",
                table: "temperature_logs");

            migrationBuilder.DropIndex(
                name: "ix_purchase_requests_client_account_id",
                table: "purchase_requests");

            migrationBuilder.DropIndex(
                name: "ix_purchase_request_lines_catalog_item_id",
                table: "purchase_request_lines");

            migrationBuilder.DropIndex(
                name: "ix_purchase_request_lines_tenant_id",
                table: "purchase_request_lines");

            migrationBuilder.DropIndex(
                name: "ix_proof_of_delivery_records_tenant_id",
                table: "proof_of_delivery_records");

            migrationBuilder.DropIndex(
                name: "ix_promotion_catalog_items_catalog_item_id",
                table: "promotion_catalog_items");

            migrationBuilder.DropIndex(
                name: "ix_promotion_catalog_items_promotion_id",
                table: "promotion_catalog_items");

            migrationBuilder.DropIndex(
                name: "ix_payments_client_account_id",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payments_invoice_id",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payments_order_id",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payments_payment_method_record_id",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payment_process_records_client_account_id",
                table: "payment_process_records");

            migrationBuilder.DropIndex(
                name: "ix_payment_process_records_order_id",
                table: "payment_process_records");

            migrationBuilder.DropIndex(
                name: "ix_payment_process_records_payment_id",
                table: "payment_process_records");

            migrationBuilder.DropIndex(
                name: "ix_payment_process_records_payment_method_record_id",
                table: "payment_process_records");

            migrationBuilder.DropIndex(
                name: "ix_payment_method_records_client_account_id",
                table: "payment_method_records");

            migrationBuilder.DropIndex(
                name: "ix_orders_client_account_id",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "ix_order_items_order_id",
                table: "order_items");

            migrationBuilder.DropIndex(
                name: "ix_notification_records_client_account_id",
                table: "notification_records");

            migrationBuilder.DropIndex(
                name: "ix_inventory_reservation_records_inventory_item_id",
                table: "inventory_reservation_records");

            migrationBuilder.DropIndex(
                name: "ix_inventory_reservation_records_inventory_lot_id",
                table: "inventory_reservation_records");

            migrationBuilder.DropIndex(
                name: "ix_inventory_reservation_records_order_id",
                table: "inventory_reservation_records");

            migrationBuilder.DropIndex(
                name: "ix_inventory_reservation_records_purchase_request_id",
                table: "inventory_reservation_records");

            migrationBuilder.DropIndex(
                name: "ix_inventory_movements_inventory_item_id",
                table: "inventory_movements");

            migrationBuilder.DropIndex(
                name: "ix_inventory_movements_inventory_lot_id",
                table: "inventory_movements");

            migrationBuilder.DropIndex(
                name: "ix_inventory_movements_order_id",
                table: "inventory_movements");

            migrationBuilder.DropIndex(
                name: "ix_inventory_movements_warehouse_id",
                table: "inventory_movements");

            migrationBuilder.DropIndex(
                name: "ix_inventory_lots_inventory_item_id",
                table: "inventory_lots");

            migrationBuilder.DropIndex(
                name: "ix_inventory_lots_warehouse_id",
                table: "inventory_lots");

            migrationBuilder.DropIndex(
                name: "ix_dispatch_orders_client_account_id",
                table: "dispatch_orders");

            migrationBuilder.DropIndex(
                name: "ix_dispatch_orders_order_id",
                table: "dispatch_orders");

            migrationBuilder.DropIndex(
                name: "ix_dispatch_events_tenant_id",
                table: "dispatch_events");

            migrationBuilder.DropIndex(
                name: "ix_customer_portal_tasks_client_account_id",
                table: "customer_portal_tasks");

            migrationBuilder.DropIndex(
                name: "ix_credit_requests_client_account_id",
                table: "credit_requests");

            migrationBuilder.DropIndex(
                name: "ix_conversation_messages_client_account_id",
                table: "conversation_messages");

            migrationBuilder.DropIndex(
                name: "ix_conversation_messages_order_id",
                table: "conversation_messages");

            migrationBuilder.DropIndex(
                name: "ix_conversation_messages_purchase_request_id",
                table: "conversation_messages");

            migrationBuilder.DropIndex(
                name: "ix_business_documents_client_account_id",
                table: "business_documents");

            migrationBuilder.DropIndex(
                name: "ix_business_documents_order_id",
                table: "business_documents");

            migrationBuilder.AddUniqueConstraint(
                name: "ak_workspaces_tenant_id_id",
                table: "workspaces",
                columns: new[] { "tenant_id", "id" });

            migrationBuilder.AddUniqueConstraint(
                name: "ak_warehouses_tenant_id_id",
                table: "warehouses",
                columns: new[] { "tenant_id", "id" });

            migrationBuilder.AddUniqueConstraint(
                name: "ak_purchase_requests_tenant_id_id",
                table: "purchase_requests",
                columns: new[] { "tenant_id", "id" });

            migrationBuilder.AddUniqueConstraint(
                name: "ak_promotions_tenant_id_id",
                table: "promotions",
                columns: new[] { "tenant_id", "id" });

            migrationBuilder.AddUniqueConstraint(
                name: "ak_payments_tenant_id_id",
                table: "payments",
                columns: new[] { "tenant_id", "id" });

            migrationBuilder.AddUniqueConstraint(
                name: "ak_payment_method_records_tenant_id_id",
                table: "payment_method_records",
                columns: new[] { "tenant_id", "id" });

            migrationBuilder.AddUniqueConstraint(
                name: "ak_orders_tenant_id_id",
                table: "orders",
                columns: new[] { "tenant_id", "id" });

            migrationBuilder.AddUniqueConstraint(
                name: "ak_invoices_tenant_id_id",
                table: "invoices",
                columns: new[] { "tenant_id", "id" });

            migrationBuilder.AddUniqueConstraint(
                name: "ak_inventory_lots_tenant_id_id",
                table: "inventory_lots",
                columns: new[] { "tenant_id", "id" });

            migrationBuilder.AddUniqueConstraint(
                name: "ak_inventory_items_tenant_id_id",
                table: "inventory_items",
                columns: new[] { "tenant_id", "id" });

            migrationBuilder.AddUniqueConstraint(
                name: "ak_dispatch_orders_tenant_id_id",
                table: "dispatch_orders",
                columns: new[] { "tenant_id", "id" });

            migrationBuilder.AddUniqueConstraint(
                name: "ak_client_accounts_tenant_id_id",
                table: "client_accounts",
                columns: new[] { "tenant_id", "id" });

            migrationBuilder.AddUniqueConstraint(
                name: "ak_catalog_items_tenant_id_id",
                table: "catalog_items",
                columns: new[] { "tenant_id", "id" });

            migrationBuilder.CreateIndex(
                name: "ix_user_workspace_memberships_tenant_id_workspace_id",
                table: "user_workspace_memberships",
                columns: new[] { "tenant_id", "workspace_id" });

            migrationBuilder.CreateIndex(
                name: "ix_temperature_logs_tenant_id_dispatch_order_id",
                table: "temperature_logs",
                columns: new[] { "tenant_id", "dispatch_order_id" });

            migrationBuilder.CreateIndex(
                name: "ix_temperature_logs_tenant_id_order_id",
                table: "temperature_logs",
                columns: new[] { "tenant_id", "order_id" });

            migrationBuilder.CreateIndex(
                name: "ix_purchase_requests_tenant_id_client_account_id",
                table: "purchase_requests",
                columns: new[] { "tenant_id", "client_account_id" });

            migrationBuilder.CreateIndex(
                name: "ix_purchase_request_lines_tenant_id_catalog_item_id",
                table: "purchase_request_lines",
                columns: new[] { "tenant_id", "catalog_item_id" });

            migrationBuilder.CreateIndex(
                name: "ix_purchase_request_lines_tenant_id_purchase_request_id",
                table: "purchase_request_lines",
                columns: new[] { "tenant_id", "purchase_request_id" });

            migrationBuilder.CreateIndex(
                name: "ix_proof_of_delivery_records_tenant_id_dispatch_order_id",
                table: "proof_of_delivery_records",
                columns: new[] { "tenant_id", "dispatch_order_id" });

            migrationBuilder.CreateIndex(
                name: "ix_promotion_catalog_items_tenant_id_catalog_item_id",
                table: "promotion_catalog_items",
                columns: new[] { "tenant_id", "catalog_item_id" });

            migrationBuilder.CreateIndex(
                name: "ix_payments_tenant_id_client_account_id",
                table: "payments",
                columns: new[] { "tenant_id", "client_account_id" });

            migrationBuilder.CreateIndex(
                name: "ix_payments_tenant_id_invoice_id",
                table: "payments",
                columns: new[] { "tenant_id", "invoice_id" });

            migrationBuilder.CreateIndex(
                name: "ix_payments_tenant_id_order_id",
                table: "payments",
                columns: new[] { "tenant_id", "order_id" });

            migrationBuilder.CreateIndex(
                name: "ix_payments_tenant_id_payment_method_record_id",
                table: "payments",
                columns: new[] { "tenant_id", "payment_method_record_id" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_process_records_tenant_id_client_account_id",
                table: "payment_process_records",
                columns: new[] { "tenant_id", "client_account_id" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_process_records_tenant_id_order_id",
                table: "payment_process_records",
                columns: new[] { "tenant_id", "order_id" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_process_records_tenant_id_payment_id",
                table: "payment_process_records",
                columns: new[] { "tenant_id", "payment_id" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_process_records_tenant_id_payment_method_record_id",
                table: "payment_process_records",
                columns: new[] { "tenant_id", "payment_method_record_id" });

            migrationBuilder.CreateIndex(
                name: "ix_inventory_reservation_records_tenant_id_inventory_item_id",
                table: "inventory_reservation_records",
                columns: new[] { "tenant_id", "inventory_item_id" });

            migrationBuilder.CreateIndex(
                name: "ix_inventory_reservation_records_tenant_id_inventory_lot_id",
                table: "inventory_reservation_records",
                columns: new[] { "tenant_id", "inventory_lot_id" });

            migrationBuilder.CreateIndex(
                name: "ix_inventory_reservation_records_tenant_id_order_id",
                table: "inventory_reservation_records",
                columns: new[] { "tenant_id", "order_id" });

            migrationBuilder.CreateIndex(
                name: "ix_inventory_reservation_records_tenant_id_purchase_request_id",
                table: "inventory_reservation_records",
                columns: new[] { "tenant_id", "purchase_request_id" });

            migrationBuilder.CreateIndex(
                name: "ix_inventory_movements_tenant_id_inventory_item_id",
                table: "inventory_movements",
                columns: new[] { "tenant_id", "inventory_item_id" });

            migrationBuilder.CreateIndex(
                name: "ix_inventory_movements_tenant_id_inventory_lot_id",
                table: "inventory_movements",
                columns: new[] { "tenant_id", "inventory_lot_id" });

            migrationBuilder.CreateIndex(
                name: "ix_inventory_movements_tenant_id_order_id",
                table: "inventory_movements",
                columns: new[] { "tenant_id", "order_id" });

            migrationBuilder.CreateIndex(
                name: "ix_inventory_movements_tenant_id_warehouse_id",
                table: "inventory_movements",
                columns: new[] { "tenant_id", "warehouse_id" });

            migrationBuilder.CreateIndex(
                name: "ix_inventory_lots_tenant_id_inventory_item_id",
                table: "inventory_lots",
                columns: new[] { "tenant_id", "inventory_item_id" });

            migrationBuilder.CreateIndex(
                name: "ix_inventory_lots_tenant_id_warehouse_id",
                table: "inventory_lots",
                columns: new[] { "tenant_id", "warehouse_id" });

            migrationBuilder.CreateIndex(
                name: "ix_dispatch_orders_tenant_id_client_account_id",
                table: "dispatch_orders",
                columns: new[] { "tenant_id", "client_account_id" });

            migrationBuilder.CreateIndex(
                name: "ix_dispatch_orders_tenant_id_order_id",
                table: "dispatch_orders",
                columns: new[] { "tenant_id", "order_id" });

            migrationBuilder.CreateIndex(
                name: "ix_dispatch_events_tenant_id_dispatch_order_id",
                table: "dispatch_events",
                columns: new[] { "tenant_id", "dispatch_order_id" });

            migrationBuilder.CreateIndex(
                name: "ix_credit_requests_tenant_id_client_account_id",
                table: "credit_requests",
                columns: new[] { "tenant_id", "client_account_id" });

            migrationBuilder.CreateIndex(
                name: "ix_conversation_messages_tenant_id_order_id",
                table: "conversation_messages",
                columns: new[] { "tenant_id", "order_id" });

            migrationBuilder.CreateIndex(
                name: "ix_conversation_messages_tenant_id_purchase_request_id",
                table: "conversation_messages",
                columns: new[] { "tenant_id", "purchase_request_id" });

            migrationBuilder.CreateIndex(
                name: "ix_business_documents_tenant_id_order_id",
                table: "business_documents",
                columns: new[] { "tenant_id", "order_id" });

            migrationBuilder.AddForeignKey(
                name: "fk_business_documents_client_accounts_tenant_id_client_account~",
                table: "business_documents",
                columns: new[] { "tenant_id", "client_account_id" },
                principalTable: "client_accounts",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_business_documents_orders_tenant_id_order_id",
                table: "business_documents",
                columns: new[] { "tenant_id", "order_id" },
                principalTable: "orders",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_conversation_messages_client_accounts_tenant_id_client_acco~",
                table: "conversation_messages",
                columns: new[] { "tenant_id", "client_account_id" },
                principalTable: "client_accounts",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_conversation_messages_orders_tenant_id_order_id",
                table: "conversation_messages",
                columns: new[] { "tenant_id", "order_id" },
                principalTable: "orders",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_conversation_messages_purchase_requests_tenant_id_purchase_~",
                table: "conversation_messages",
                columns: new[] { "tenant_id", "purchase_request_id" },
                principalTable: "purchase_requests",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_credit_requests_client_accounts_tenant_id_client_account_id",
                table: "credit_requests",
                columns: new[] { "tenant_id", "client_account_id" },
                principalTable: "client_accounts",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_customer_portal_tasks_client_accounts_tenant_id_client_acco~",
                table: "customer_portal_tasks",
                columns: new[] { "tenant_id", "client_account_id" },
                principalTable: "client_accounts",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_dispatch_events_dispatch_orders_tenant_id_dispatch_order_id",
                table: "dispatch_events",
                columns: new[] { "tenant_id", "dispatch_order_id" },
                principalTable: "dispatch_orders",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_dispatch_orders_client_accounts_tenant_id_client_account_id",
                table: "dispatch_orders",
                columns: new[] { "tenant_id", "client_account_id" },
                principalTable: "client_accounts",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_dispatch_orders_orders_tenant_id_order_id",
                table: "dispatch_orders",
                columns: new[] { "tenant_id", "order_id" },
                principalTable: "orders",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_inventory_lots_inventory_items_tenant_id_inventory_item_id",
                table: "inventory_lots",
                columns: new[] { "tenant_id", "inventory_item_id" },
                principalTable: "inventory_items",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_inventory_lots_warehouses_tenant_id_warehouse_id",
                table: "inventory_lots",
                columns: new[] { "tenant_id", "warehouse_id" },
                principalTable: "warehouses",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_inventory_movements_inventory_items_tenant_id_inventory_ite~",
                table: "inventory_movements",
                columns: new[] { "tenant_id", "inventory_item_id" },
                principalTable: "inventory_items",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_inventory_movements_inventory_lots_tenant_id_inventory_lot_~",
                table: "inventory_movements",
                columns: new[] { "tenant_id", "inventory_lot_id" },
                principalTable: "inventory_lots",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_inventory_movements_orders_tenant_id_order_id",
                table: "inventory_movements",
                columns: new[] { "tenant_id", "order_id" },
                principalTable: "orders",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_inventory_movements_warehouses_tenant_id_warehouse_id",
                table: "inventory_movements",
                columns: new[] { "tenant_id", "warehouse_id" },
                principalTable: "warehouses",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_inventory_reservation_records_inventory_items_tenant_id_inv~",
                table: "inventory_reservation_records",
                columns: new[] { "tenant_id", "inventory_item_id" },
                principalTable: "inventory_items",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_inventory_reservation_records_inventory_lots_tenant_id_inve~",
                table: "inventory_reservation_records",
                columns: new[] { "tenant_id", "inventory_lot_id" },
                principalTable: "inventory_lots",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_inventory_reservation_records_orders_tenant_id_order_id",
                table: "inventory_reservation_records",
                columns: new[] { "tenant_id", "order_id" },
                principalTable: "orders",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_inventory_reservation_records_purchase_requests_tenant_id_p~",
                table: "inventory_reservation_records",
                columns: new[] { "tenant_id", "purchase_request_id" },
                principalTable: "purchase_requests",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_notification_records_client_accounts_tenant_id_client_accou~",
                table: "notification_records",
                columns: new[] { "tenant_id", "client_account_id" },
                principalTable: "client_accounts",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_order_items_orders_tenant_id_order_id",
                table: "order_items",
                columns: new[] { "tenant_id", "order_id" },
                principalTable: "orders",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_orders_client_accounts_tenant_id_client_account_id",
                table: "orders",
                columns: new[] { "tenant_id", "client_account_id" },
                principalTable: "client_accounts",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_payment_method_records_client_accounts_tenant_id_client_acc~",
                table: "payment_method_records",
                columns: new[] { "tenant_id", "client_account_id" },
                principalTable: "client_accounts",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_payment_process_records_client_accounts_tenant_id_client_ac~",
                table: "payment_process_records",
                columns: new[] { "tenant_id", "client_account_id" },
                principalTable: "client_accounts",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_payment_process_records_orders_tenant_id_order_id",
                table: "payment_process_records",
                columns: new[] { "tenant_id", "order_id" },
                principalTable: "orders",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_payment_process_records_payment_method_records_tenant_id_pa~",
                table: "payment_process_records",
                columns: new[] { "tenant_id", "payment_method_record_id" },
                principalTable: "payment_method_records",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_payment_process_records_payments_tenant_id_payment_id",
                table: "payment_process_records",
                columns: new[] { "tenant_id", "payment_id" },
                principalTable: "payments",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_payments_client_accounts_tenant_id_client_account_id",
                table: "payments",
                columns: new[] { "tenant_id", "client_account_id" },
                principalTable: "client_accounts",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_payments_invoices_tenant_id_invoice_id",
                table: "payments",
                columns: new[] { "tenant_id", "invoice_id" },
                principalTable: "invoices",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_payments_orders_tenant_id_order_id",
                table: "payments",
                columns: new[] { "tenant_id", "order_id" },
                principalTable: "orders",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_payments_payment_method_records_tenant_id_payment_method_re~",
                table: "payments",
                columns: new[] { "tenant_id", "payment_method_record_id" },
                principalTable: "payment_method_records",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_promotion_catalog_items_catalog_items_tenant_id_catalog_ite~",
                table: "promotion_catalog_items",
                columns: new[] { "tenant_id", "catalog_item_id" },
                principalTable: "catalog_items",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_promotion_catalog_items_promotions_tenant_id_promotion_id",
                table: "promotion_catalog_items",
                columns: new[] { "tenant_id", "promotion_id" },
                principalTable: "promotions",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_proof_of_delivery_records_dispatch_orders_tenant_id_dispatc~",
                table: "proof_of_delivery_records",
                columns: new[] { "tenant_id", "dispatch_order_id" },
                principalTable: "dispatch_orders",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_purchase_request_lines_catalog_items_tenant_id_catalog_item~",
                table: "purchase_request_lines",
                columns: new[] { "tenant_id", "catalog_item_id" },
                principalTable: "catalog_items",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_purchase_request_lines_purchase_requests_tenant_id_purchase~",
                table: "purchase_request_lines",
                columns: new[] { "tenant_id", "purchase_request_id" },
                principalTable: "purchase_requests",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_purchase_requests_client_accounts_tenant_id_client_account_~",
                table: "purchase_requests",
                columns: new[] { "tenant_id", "client_account_id" },
                principalTable: "client_accounts",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_temperature_logs_dispatch_orders_tenant_id_dispatch_order_id",
                table: "temperature_logs",
                columns: new[] { "tenant_id", "dispatch_order_id" },
                principalTable: "dispatch_orders",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_temperature_logs_orders_tenant_id_order_id",
                table: "temperature_logs",
                columns: new[] { "tenant_id", "order_id" },
                principalTable: "orders",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_user_workspace_memberships_workspaces_tenant_id_workspace_id",
                table: "user_workspace_memberships",
                columns: new[] { "tenant_id", "workspace_id" },
                principalTable: "workspaces",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_workspace_preferences_workspaces_tenant_id_workspace_id",
                table: "workspace_preferences",
                columns: new[] { "tenant_id", "workspace_id" },
                principalTable: "workspaces",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_business_documents_client_accounts_tenant_id_client_account~",
                table: "business_documents");

            migrationBuilder.DropForeignKey(
                name: "fk_business_documents_orders_tenant_id_order_id",
                table: "business_documents");

            migrationBuilder.DropForeignKey(
                name: "fk_conversation_messages_client_accounts_tenant_id_client_acco~",
                table: "conversation_messages");

            migrationBuilder.DropForeignKey(
                name: "fk_conversation_messages_orders_tenant_id_order_id",
                table: "conversation_messages");

            migrationBuilder.DropForeignKey(
                name: "fk_conversation_messages_purchase_requests_tenant_id_purchase_~",
                table: "conversation_messages");

            migrationBuilder.DropForeignKey(
                name: "fk_credit_requests_client_accounts_tenant_id_client_account_id",
                table: "credit_requests");

            migrationBuilder.DropForeignKey(
                name: "fk_customer_portal_tasks_client_accounts_tenant_id_client_acco~",
                table: "customer_portal_tasks");

            migrationBuilder.DropForeignKey(
                name: "fk_dispatch_events_dispatch_orders_tenant_id_dispatch_order_id",
                table: "dispatch_events");

            migrationBuilder.DropForeignKey(
                name: "fk_dispatch_orders_client_accounts_tenant_id_client_account_id",
                table: "dispatch_orders");

            migrationBuilder.DropForeignKey(
                name: "fk_dispatch_orders_orders_tenant_id_order_id",
                table: "dispatch_orders");

            migrationBuilder.DropForeignKey(
                name: "fk_inventory_lots_inventory_items_tenant_id_inventory_item_id",
                table: "inventory_lots");

            migrationBuilder.DropForeignKey(
                name: "fk_inventory_lots_warehouses_tenant_id_warehouse_id",
                table: "inventory_lots");

            migrationBuilder.DropForeignKey(
                name: "fk_inventory_movements_inventory_items_tenant_id_inventory_ite~",
                table: "inventory_movements");

            migrationBuilder.DropForeignKey(
                name: "fk_inventory_movements_inventory_lots_tenant_id_inventory_lot_~",
                table: "inventory_movements");

            migrationBuilder.DropForeignKey(
                name: "fk_inventory_movements_orders_tenant_id_order_id",
                table: "inventory_movements");

            migrationBuilder.DropForeignKey(
                name: "fk_inventory_movements_warehouses_tenant_id_warehouse_id",
                table: "inventory_movements");

            migrationBuilder.DropForeignKey(
                name: "fk_inventory_reservation_records_inventory_items_tenant_id_inv~",
                table: "inventory_reservation_records");

            migrationBuilder.DropForeignKey(
                name: "fk_inventory_reservation_records_inventory_lots_tenant_id_inve~",
                table: "inventory_reservation_records");

            migrationBuilder.DropForeignKey(
                name: "fk_inventory_reservation_records_orders_tenant_id_order_id",
                table: "inventory_reservation_records");

            migrationBuilder.DropForeignKey(
                name: "fk_inventory_reservation_records_purchase_requests_tenant_id_p~",
                table: "inventory_reservation_records");

            migrationBuilder.DropForeignKey(
                name: "fk_notification_records_client_accounts_tenant_id_client_accou~",
                table: "notification_records");

            migrationBuilder.DropForeignKey(
                name: "fk_order_items_orders_tenant_id_order_id",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "fk_orders_client_accounts_tenant_id_client_account_id",
                table: "orders");

            migrationBuilder.DropForeignKey(
                name: "fk_payment_method_records_client_accounts_tenant_id_client_acc~",
                table: "payment_method_records");

            migrationBuilder.DropForeignKey(
                name: "fk_payment_process_records_client_accounts_tenant_id_client_ac~",
                table: "payment_process_records");

            migrationBuilder.DropForeignKey(
                name: "fk_payment_process_records_orders_tenant_id_order_id",
                table: "payment_process_records");

            migrationBuilder.DropForeignKey(
                name: "fk_payment_process_records_payment_method_records_tenant_id_pa~",
                table: "payment_process_records");

            migrationBuilder.DropForeignKey(
                name: "fk_payment_process_records_payments_tenant_id_payment_id",
                table: "payment_process_records");

            migrationBuilder.DropForeignKey(
                name: "fk_payments_client_accounts_tenant_id_client_account_id",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "fk_payments_invoices_tenant_id_invoice_id",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "fk_payments_orders_tenant_id_order_id",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "fk_payments_payment_method_records_tenant_id_payment_method_re~",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "fk_promotion_catalog_items_catalog_items_tenant_id_catalog_ite~",
                table: "promotion_catalog_items");

            migrationBuilder.DropForeignKey(
                name: "fk_promotion_catalog_items_promotions_tenant_id_promotion_id",
                table: "promotion_catalog_items");

            migrationBuilder.DropForeignKey(
                name: "fk_proof_of_delivery_records_dispatch_orders_tenant_id_dispatc~",
                table: "proof_of_delivery_records");

            migrationBuilder.DropForeignKey(
                name: "fk_purchase_request_lines_catalog_items_tenant_id_catalog_item~",
                table: "purchase_request_lines");

            migrationBuilder.DropForeignKey(
                name: "fk_purchase_request_lines_purchase_requests_tenant_id_purchase~",
                table: "purchase_request_lines");

            migrationBuilder.DropForeignKey(
                name: "fk_purchase_requests_client_accounts_tenant_id_client_account_~",
                table: "purchase_requests");

            migrationBuilder.DropForeignKey(
                name: "fk_temperature_logs_dispatch_orders_tenant_id_dispatch_order_id",
                table: "temperature_logs");

            migrationBuilder.DropForeignKey(
                name: "fk_temperature_logs_orders_tenant_id_order_id",
                table: "temperature_logs");

            migrationBuilder.DropForeignKey(
                name: "fk_user_workspace_memberships_workspaces_tenant_id_workspace_id",
                table: "user_workspace_memberships");

            migrationBuilder.DropForeignKey(
                name: "fk_workspace_preferences_workspaces_tenant_id_workspace_id",
                table: "workspace_preferences");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_workspaces_tenant_id_id",
                table: "workspaces");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_warehouses_tenant_id_id",
                table: "warehouses");

            migrationBuilder.DropIndex(
                name: "ix_user_workspace_memberships_tenant_id_workspace_id",
                table: "user_workspace_memberships");

            migrationBuilder.DropIndex(
                name: "ix_temperature_logs_tenant_id_dispatch_order_id",
                table: "temperature_logs");

            migrationBuilder.DropIndex(
                name: "ix_temperature_logs_tenant_id_order_id",
                table: "temperature_logs");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_purchase_requests_tenant_id_id",
                table: "purchase_requests");

            migrationBuilder.DropIndex(
                name: "ix_purchase_requests_tenant_id_client_account_id",
                table: "purchase_requests");

            migrationBuilder.DropIndex(
                name: "ix_purchase_request_lines_tenant_id_catalog_item_id",
                table: "purchase_request_lines");

            migrationBuilder.DropIndex(
                name: "ix_purchase_request_lines_tenant_id_purchase_request_id",
                table: "purchase_request_lines");

            migrationBuilder.DropIndex(
                name: "ix_proof_of_delivery_records_tenant_id_dispatch_order_id",
                table: "proof_of_delivery_records");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_promotions_tenant_id_id",
                table: "promotions");

            migrationBuilder.DropIndex(
                name: "ix_promotion_catalog_items_tenant_id_catalog_item_id",
                table: "promotion_catalog_items");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_payments_tenant_id_id",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payments_tenant_id_client_account_id",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payments_tenant_id_invoice_id",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payments_tenant_id_order_id",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payments_tenant_id_payment_method_record_id",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payment_process_records_tenant_id_client_account_id",
                table: "payment_process_records");

            migrationBuilder.DropIndex(
                name: "ix_payment_process_records_tenant_id_order_id",
                table: "payment_process_records");

            migrationBuilder.DropIndex(
                name: "ix_payment_process_records_tenant_id_payment_id",
                table: "payment_process_records");

            migrationBuilder.DropIndex(
                name: "ix_payment_process_records_tenant_id_payment_method_record_id",
                table: "payment_process_records");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_payment_method_records_tenant_id_id",
                table: "payment_method_records");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_orders_tenant_id_id",
                table: "orders");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_invoices_tenant_id_id",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "ix_inventory_reservation_records_tenant_id_inventory_item_id",
                table: "inventory_reservation_records");

            migrationBuilder.DropIndex(
                name: "ix_inventory_reservation_records_tenant_id_inventory_lot_id",
                table: "inventory_reservation_records");

            migrationBuilder.DropIndex(
                name: "ix_inventory_reservation_records_tenant_id_order_id",
                table: "inventory_reservation_records");

            migrationBuilder.DropIndex(
                name: "ix_inventory_reservation_records_tenant_id_purchase_request_id",
                table: "inventory_reservation_records");

            migrationBuilder.DropIndex(
                name: "ix_inventory_movements_tenant_id_inventory_item_id",
                table: "inventory_movements");

            migrationBuilder.DropIndex(
                name: "ix_inventory_movements_tenant_id_inventory_lot_id",
                table: "inventory_movements");

            migrationBuilder.DropIndex(
                name: "ix_inventory_movements_tenant_id_order_id",
                table: "inventory_movements");

            migrationBuilder.DropIndex(
                name: "ix_inventory_movements_tenant_id_warehouse_id",
                table: "inventory_movements");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_inventory_lots_tenant_id_id",
                table: "inventory_lots");

            migrationBuilder.DropIndex(
                name: "ix_inventory_lots_tenant_id_inventory_item_id",
                table: "inventory_lots");

            migrationBuilder.DropIndex(
                name: "ix_inventory_lots_tenant_id_warehouse_id",
                table: "inventory_lots");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_inventory_items_tenant_id_id",
                table: "inventory_items");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_dispatch_orders_tenant_id_id",
                table: "dispatch_orders");

            migrationBuilder.DropIndex(
                name: "ix_dispatch_orders_tenant_id_client_account_id",
                table: "dispatch_orders");

            migrationBuilder.DropIndex(
                name: "ix_dispatch_orders_tenant_id_order_id",
                table: "dispatch_orders");

            migrationBuilder.DropIndex(
                name: "ix_dispatch_events_tenant_id_dispatch_order_id",
                table: "dispatch_events");

            migrationBuilder.DropIndex(
                name: "ix_credit_requests_tenant_id_client_account_id",
                table: "credit_requests");

            migrationBuilder.DropIndex(
                name: "ix_conversation_messages_tenant_id_order_id",
                table: "conversation_messages");

            migrationBuilder.DropIndex(
                name: "ix_conversation_messages_tenant_id_purchase_request_id",
                table: "conversation_messages");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_client_accounts_tenant_id_id",
                table: "client_accounts");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_catalog_items_tenant_id_id",
                table: "catalog_items");

            migrationBuilder.DropIndex(
                name: "ix_business_documents_tenant_id_order_id",
                table: "business_documents");

            migrationBuilder.CreateIndex(
                name: "ix_workspace_preferences_workspace_id",
                table: "workspace_preferences",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "ix_temperature_logs_dispatch_order_id",
                table: "temperature_logs",
                column: "dispatch_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_temperature_logs_order_id",
                table: "temperature_logs",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_requests_client_account_id",
                table: "purchase_requests",
                column: "client_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_request_lines_catalog_item_id",
                table: "purchase_request_lines",
                column: "catalog_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_request_lines_tenant_id",
                table: "purchase_request_lines",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_proof_of_delivery_records_tenant_id",
                table: "proof_of_delivery_records",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_promotion_catalog_items_catalog_item_id",
                table: "promotion_catalog_items",
                column: "catalog_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_promotion_catalog_items_promotion_id",
                table: "promotion_catalog_items",
                column: "promotion_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_client_account_id",
                table: "payments",
                column: "client_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_invoice_id",
                table: "payments",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_order_id",
                table: "payments",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_payment_method_record_id",
                table: "payments",
                column: "payment_method_record_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_process_records_client_account_id",
                table: "payment_process_records",
                column: "client_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_process_records_order_id",
                table: "payment_process_records",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_process_records_payment_id",
                table: "payment_process_records",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_process_records_payment_method_record_id",
                table: "payment_process_records",
                column: "payment_method_record_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_method_records_client_account_id",
                table: "payment_method_records",
                column: "client_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_client_account_id",
                table: "orders",
                column: "client_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_order_id",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_notification_records_client_account_id",
                table: "notification_records",
                column: "client_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_reservation_records_inventory_item_id",
                table: "inventory_reservation_records",
                column: "inventory_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_reservation_records_inventory_lot_id",
                table: "inventory_reservation_records",
                column: "inventory_lot_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_reservation_records_order_id",
                table: "inventory_reservation_records",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_reservation_records_purchase_request_id",
                table: "inventory_reservation_records",
                column: "purchase_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_movements_inventory_item_id",
                table: "inventory_movements",
                column: "inventory_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_movements_inventory_lot_id",
                table: "inventory_movements",
                column: "inventory_lot_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_movements_order_id",
                table: "inventory_movements",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_movements_warehouse_id",
                table: "inventory_movements",
                column: "warehouse_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_lots_inventory_item_id",
                table: "inventory_lots",
                column: "inventory_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_lots_warehouse_id",
                table: "inventory_lots",
                column: "warehouse_id");

            migrationBuilder.CreateIndex(
                name: "ix_dispatch_orders_client_account_id",
                table: "dispatch_orders",
                column: "client_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_dispatch_orders_order_id",
                table: "dispatch_orders",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_dispatch_events_tenant_id",
                table: "dispatch_events",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_portal_tasks_client_account_id",
                table: "customer_portal_tasks",
                column: "client_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_credit_requests_client_account_id",
                table: "credit_requests",
                column: "client_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_conversation_messages_client_account_id",
                table: "conversation_messages",
                column: "client_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_conversation_messages_order_id",
                table: "conversation_messages",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_conversation_messages_purchase_request_id",
                table: "conversation_messages",
                column: "purchase_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_business_documents_client_account_id",
                table: "business_documents",
                column: "client_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_business_documents_order_id",
                table: "business_documents",
                column: "order_id");

            migrationBuilder.AddForeignKey(
                name: "fk_business_documents_client_accounts_client_account_id",
                table: "business_documents",
                column: "client_account_id",
                principalTable: "client_accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_business_documents_orders_order_id",
                table: "business_documents",
                column: "order_id",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_conversation_messages_client_accounts_client_account_id",
                table: "conversation_messages",
                column: "client_account_id",
                principalTable: "client_accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_conversation_messages_orders_order_id",
                table: "conversation_messages",
                column: "order_id",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_conversation_messages_purchase_requests_purchase_request_id",
                table: "conversation_messages",
                column: "purchase_request_id",
                principalTable: "purchase_requests",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_credit_requests_client_accounts_client_account_id",
                table: "credit_requests",
                column: "client_account_id",
                principalTable: "client_accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_customer_portal_tasks_client_accounts_client_account_id",
                table: "customer_portal_tasks",
                column: "client_account_id",
                principalTable: "client_accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_dispatch_events_dispatch_orders_dispatch_order_id",
                table: "dispatch_events",
                column: "dispatch_order_id",
                principalTable: "dispatch_orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_dispatch_orders_client_accounts_client_account_id",
                table: "dispatch_orders",
                column: "client_account_id",
                principalTable: "client_accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_dispatch_orders_orders_order_id",
                table: "dispatch_orders",
                column: "order_id",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_inventory_lots_inventory_items_inventory_item_id",
                table: "inventory_lots",
                column: "inventory_item_id",
                principalTable: "inventory_items",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_inventory_lots_warehouses_warehouse_id",
                table: "inventory_lots",
                column: "warehouse_id",
                principalTable: "warehouses",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_inventory_movements_inventory_items_inventory_item_id",
                table: "inventory_movements",
                column: "inventory_item_id",
                principalTable: "inventory_items",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_inventory_movements_inventory_lots_inventory_lot_id",
                table: "inventory_movements",
                column: "inventory_lot_id",
                principalTable: "inventory_lots",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_inventory_movements_orders_order_id",
                table: "inventory_movements",
                column: "order_id",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_inventory_movements_warehouses_warehouse_id",
                table: "inventory_movements",
                column: "warehouse_id",
                principalTable: "warehouses",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_inventory_reservation_records_inventory_items_inventory_ite~",
                table: "inventory_reservation_records",
                column: "inventory_item_id",
                principalTable: "inventory_items",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_inventory_reservation_records_inventory_lots_inventory_lot_~",
                table: "inventory_reservation_records",
                column: "inventory_lot_id",
                principalTable: "inventory_lots",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_inventory_reservation_records_orders_order_id",
                table: "inventory_reservation_records",
                column: "order_id",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_inventory_reservation_records_purchase_requests_purchase_re~",
                table: "inventory_reservation_records",
                column: "purchase_request_id",
                principalTable: "purchase_requests",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_notification_records_client_accounts_client_account_id",
                table: "notification_records",
                column: "client_account_id",
                principalTable: "client_accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_order_items_orders_order_id",
                table: "order_items",
                column: "order_id",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_orders_client_accounts_client_account_id",
                table: "orders",
                column: "client_account_id",
                principalTable: "client_accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_payment_method_records_client_accounts_client_account_id",
                table: "payment_method_records",
                column: "client_account_id",
                principalTable: "client_accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_payment_process_records_client_accounts_client_account_id",
                table: "payment_process_records",
                column: "client_account_id",
                principalTable: "client_accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_payment_process_records_orders_order_id",
                table: "payment_process_records",
                column: "order_id",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_payment_process_records_payment_method_records_payment_meth~",
                table: "payment_process_records",
                column: "payment_method_record_id",
                principalTable: "payment_method_records",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_payment_process_records_payments_payment_id",
                table: "payment_process_records",
                column: "payment_id",
                principalTable: "payments",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_payments_client_accounts_client_account_id",
                table: "payments",
                column: "client_account_id",
                principalTable: "client_accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_payments_invoices_invoice_id",
                table: "payments",
                column: "invoice_id",
                principalTable: "invoices",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_payments_orders_order_id",
                table: "payments",
                column: "order_id",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_payments_payment_method_records_payment_method_record_id",
                table: "payments",
                column: "payment_method_record_id",
                principalTable: "payment_method_records",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_promotion_catalog_items_catalog_items_catalog_item_id",
                table: "promotion_catalog_items",
                column: "catalog_item_id",
                principalTable: "catalog_items",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_promotion_catalog_items_promotions_promotion_id",
                table: "promotion_catalog_items",
                column: "promotion_id",
                principalTable: "promotions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_proof_of_delivery_records_dispatch_orders_dispatch_order_id",
                table: "proof_of_delivery_records",
                column: "dispatch_order_id",
                principalTable: "dispatch_orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_purchase_request_lines_catalog_items_catalog_item_id",
                table: "purchase_request_lines",
                column: "catalog_item_id",
                principalTable: "catalog_items",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_purchase_request_lines_purchase_requests_purchase_request_id",
                table: "purchase_request_lines",
                column: "purchase_request_id",
                principalTable: "purchase_requests",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_purchase_requests_client_accounts_client_account_id",
                table: "purchase_requests",
                column: "client_account_id",
                principalTable: "client_accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_temperature_logs_dispatch_orders_dispatch_order_id",
                table: "temperature_logs",
                column: "dispatch_order_id",
                principalTable: "dispatch_orders",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_temperature_logs_orders_order_id",
                table: "temperature_logs",
                column: "order_id",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_user_workspace_memberships_workspaces_workspace_id",
                table: "user_workspace_memberships",
                column: "workspace_id",
                principalTable: "workspaces",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_workspace_preferences_workspaces_workspace_id",
                table: "workspace_preferences",
                column: "workspace_id",
                principalTable: "workspaces",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

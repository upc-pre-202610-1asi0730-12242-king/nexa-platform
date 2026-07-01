# Nexa Database Model

Nexa uses a shared database/shared schema model with tenant-scoped operational rows.

## Core Tables

| Bounded context | Tables | Tenant scope |
|---|---|---|
| IAM | `users`, `user_workspace_memberships` | Memberships scoped by tenant/workspace. |
| Tenant Management | `tenants`, `workspaces`, `tenant_members`, `tenant_rules`, `tenant_subscriptions` | Tenant-owned. |
| Catalog | `catalog_items`, `categories`, `brands` | `catalog_items.tenant_id`; reference metadata shared where applicable. |
| Sales | `client_accounts`, `purchase_requests`, `purchase_request_lines`, `orders`, `order_items`, `promotions`, `conversation_messages` | Tenant-scoped operational tables. |
| Warehouse | `warehouses`, `inventory_items`, `inventory_lots`, `inventory_movements`, `inventory_reservation_records` | Tenant-scoped. |
| Logistics | `dispatch_orders`, `dispatch_events`, `proof_of_delivery_records`, `temperature_logs`, `shipments` | Tenant-scoped. |
| Invoicing/Billing | `invoices`, `payments`, `business_documents`, `payment_process_records` | Tenant-scoped core rows; Payment is public core. |
| Audit | `audit_logs` | Tenant-scoped. |
| Reference | `countries`, `departments`, `provinces`, `districts`, `payment_options`, `document_types`, `unit_of_measures`, `categories`, `brands` | Shared reference data. |

## Current Reference Data Decision

- `payments.payment_option_id` uses `payment_options` where available.
- `business_documents.document_type_id` uses `document_types` where available.
- Purchase request delivery geography keeps snapshot text for display continuity. Nullable FK columns can be added later without deleting snapshots.
- Units of measure are exposed through reference endpoints and can be promoted to FK columns in catalog/inventory when migration risk is low.
- Delivery methods are currently an API allow-list, not a persisted `delivery_methods` table.
- `orders.client_account_id` is nullable for backward compatibility, indexed with `tenant_id`, and references `client_accounts` with `SET NULL`. The migration backfills it from tenant-local `orders.customer_id = client_accounts.code` matches.
- The current migration path preserves the former `workspace_resource_records` table as `legacy_workspace_resource_records`; it is not an active EF model or runtime persistence surface. Databases that applied an earlier draft may already lack that legacy table.

## Safety Rules

- No tenant fallback default values.
- No destructive migration for core tenant data.
- Add nullable columns first when introducing new FKs to existing operational rows.
- Keep indexes tenant-aware for business identifiers and status filters.

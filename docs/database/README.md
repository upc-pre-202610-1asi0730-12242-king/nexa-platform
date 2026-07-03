# Nexa AV2 Local Database Seed

Purpose: local reference data for AV2 frontend integration against real backend endpoints.

This seed is backend-owned local data. WebApp must call backend endpoints, not read this SQL file.

## Files

- `docs/database/seed-local.sql`: idempotent local PostgreSQL seed script.

## Local Warning

Use only with local `nexa_platform_db`.

Do not run this against shared, staging, or production databases unless explicitly intended.

The script does not use `DROP TABLE`, `TRUNCATE TABLE`, or broad `DELETE FROM`.

## How To Run With psql CLI

```bash
psql -U postgres -d nexa_platform_db -f docs/database/seed-local.sql
```

## How To Run With pgAdmin / DBeaver

1. Open pgAdmin or DBeaver.
2. Select local `nexa_platform_db` database.
3. Open and run `docs/database/seed-local.sql`.
4. Run verification queries at bottom.

## Tables Seeded

- `categories`
- `brands`
- `warehouses`
- `catalog_items`
- `inventory_items`

## Final Local Seed Status

Applied locally on `nexa_platform_db`.

| Table | Count After Seed | Status |
|---|---:|---|
| `users` | 4 | Existing local app seed; no AV2 user insert. |
| `categories` | 8 | Ready |
| `brands` | 10 | Ready |
| `warehouses` | 10 | Ready |
| `catalog_items` | 58 | Ready |
| `inventory_items` | 58 | Ready |

Quality checks passed:

- no duplicate natural keys
- no invalid status values
- no negative inventory quantities
- no invalid reference rows

## Tables Skipped

| Table | Reason |
|---|---|
| `users` | Existing local app seed already has controlled development users. Additional user inserts were skipped to avoid bypassing controlled password hashing/sign-up flow. |

## Verification Queries

```sql
SELECT COUNT(*) AS categories_count FROM categories;
SELECT COUNT(*) AS brands_count FROM brands;
SELECT COUNT(*) AS warehouses_count FROM warehouses;
SELECT COUNT(*) AS catalog_items_count FROM catalog_items;
SELECT COUNT(*) AS inventory_items_count FROM inventory_items;
```

## Frontend Integration

WebApp integration should keep using real backend endpoints:

- `/api/v1/categories`
- `/api/v1/brands`
- `/api/v1/warehouses`
- `/api/v1/catalog-items`
- `/api/v1/inventory-items`
- `/api/v1/orders`
- `/api/v1/dispatch-orders`
- `/api/v1/business-documents`
- `/api/v1/payment-process-records`

## Current Multi-Tenant Operational Trace

Nexa now uses a shared database model. Tenants are separated by tenant/workspace columns and memberships, not by one database per tenant.

### Workspace and IAM

| Table | Purpose | Required relationship |
|---|---|---|
| `tenants` | Company-level SaaS tenant, e.g. ICISA. | Parent for workspace and operational records. |
| `workspaces` | Operational workspace for a tenant, e.g. `icisa`. | `tenant_id` references `tenants.id`; primary workspace is unique per tenant. |
| `users` | IAM users with hashed passwords. | Users are not enough by themselves for workspace access. |
| `user_workspace_memberships` | User access to a concrete workspace. | Links `tenant_id`, `workspace_id`, `user_id`, email, role and status. |

This prevents orphan users from accessing a workspace without an explicit membership row.

### Buyer Request to Dispatch Flow

| Step | Table(s) | Why it exists |
|---|---|---|
| Buyer creates request | `purchase_requests` | Stores buyer demand, priority, requested date, comments and tenant/client scope. |
| Buyer selects products | `purchase_request_lines` | Stores each requested catalog item with quantity, unit and estimated weight. |
| Buyer declares delivery point | `purchase_requests` delivery fields and comments narrative | Stores selected delivery mode, address, district, city, province, reference, payment option and shipping estimate until the request is converted into order/dispatch records. |
| Buyer/sales communicate | `conversation_messages` | Keeps availability and commercial validation messages traceable by request/order. |
| Sales validates | `purchase_requests.status` | Status moves through submitted, commercially validated, adjustment/rejected or converted. |
| Sales converts to order | `orders`, `order_items` | Creates the commercial order aggregate and priced lines from backend catalog items. |
| Operations receives dispatch | `dispatch_orders` | Links the order to S2 logistics with route/responsible/delivery window. |
| Tracking starts | `dispatch_events` | Every dispatch command appends buyer-visible or internal trace events. |
| Documents required | `business_documents` | Creates invoice, dispatch guide, POD and temperature report placeholders. |
| Payment process | `payments`, optional `payment_process_records` history | Stores subtotal, IGV, total, payment option and payment status tied to the tenant/order/client. |
| Buyer notification | `notification_records` | Stores buyer-facing updates when an order is created or changes. |
| Delivery proof | `proof_of_delivery_records` | Stores delivery completion evidence references and status. |

### Database Integrity Expectations

- `purchase_request_lines.catalog_item_id` must reference a real `catalog_items.id`.
- `purchase_requests.client_account_id` must reference a real `client_accounts.id`.
- `dispatch_orders.order_id` must reference a real `orders.id`.
- `business_documents.order_id`, `payments.order_id`, optional `payment_process_records.order_id`, and `conversation_messages.order_id` must point to the converted order when present.
- `conversation_messages.purchase_request_id` should remain set for buyer/sales availability questions before and after conversion.
- `user_workspace_memberships` must exist for every user that should enter a workspace.
- Delivery address data must stay attached to the request before conversion and then be visible in the order/dispatch trace. Current frontend payload stores the route-readable delivery block in `purchase_requests.comments`; future schema hardening can split it into dedicated location columns without losing the existing trace.

### Useful Verification Queries

```sql
-- One request converted into a fully traceable order/dispatch/document/payment flow.
SELECT
  pr.id AS request_id,
  pr.code AS request_code,
  pr.status AS request_status,
  o.id AS order_id,
  o.order_number,
  d.id AS dispatch_id,
  d.status AS dispatch_status,
  COUNT(DISTINCT bd.id) AS documents,
  COUNT(DISTINCT cm.id) AS messages,
  COUNT(DISTINCT p.id) AS payments,
  COUNT(DISTINCT pp.id) AS payment_process_history
FROM purchase_requests pr
LEFT JOIN orders o ON o.order_number = REPLACE(pr.code, 'REQ-', 'ORD-')
LEFT JOIN dispatch_orders d ON d.order_id = o.id
LEFT JOIN business_documents bd ON bd.order_id = o.id
LEFT JOIN conversation_messages cm ON cm.purchase_request_id = pr.id
LEFT JOIN payments p ON p.order_id = o.id
LEFT JOIN payment_process_records pp ON pp.order_id = o.id
GROUP BY pr.id, pr.code, pr.status, o.id, o.order_number, d.id, d.status
ORDER BY pr.id DESC;

-- Workspace access must include both user and membership.
SELECT
  u.id AS user_id,
  u.email,
  w.slug AS workspace,
  m.role,
  m.status
FROM users u
JOIN user_workspace_memberships m ON m.user_id = u.id
JOIN workspaces w ON w.id = m.workspace_id
ORDER BY u.id;

-- Product catalog rows must expose numeric IDs used by purchase request lines.
SELECT id, catalog_item_id, product_id, item_name, available_stock
FROM catalog_items
ORDER BY id;
```

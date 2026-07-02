# Database Hardening Report

## Phase 7 Scope

This phase keeps database work non-destructive. No tables, columns, or production-like data are removed.

## Implemented

- Added EF model indexes for common tenant-scoped collection queries:
  - `orders(tenant_id, status, created_at)`
  - `purchase_requests(tenant_id, status, created_at)`
  - `dispatch_orders(tenant_id, status, created_at)`
  - `invoices(tenant_id, payment_status, created_at)`
  - `payments(tenant_id, status, created_at)`
  - `catalog_items(tenant_id, brand_name)`
  - `catalog_items(tenant_id, category_name)`
- Generated migration `20260701151138_AddSafeQueryIndexes`.
- Reviewed migration `Up`: it only creates indexes.

## Already Present

- `inventory_items(tenant_id, product_id)` already exists.
- `inventory_lots(tenant_id, expiration_date)` already exists.
- Several tenant-aware alternate keys and composite foreign keys already exist from earlier hardening.

## Deferred

CHECK constraints are deferred until live data is inspected with `docs/database-validation-queries.md`.

Catalog normalization is also deferred. Adding `brand_id` and `category_id` requires a controlled backfill from `brand_name` and `category_name`; existing text columns must remain for compatibility until frontend and API resources are migrated.

Membership duplication is documented as current architecture debt:

- `user_workspace_memberships.full_name`
- `user_workspace_memberships.email`
- `tenant_members`

For now, these fields are treated as profile snapshots and compatibility/admin surfaces, not removed columns.

## Validation Required Before Constraints

Run the queries in `docs/database-validation-queries.md` against the target database before adding any CHECK constraints for quantity, money, dates, temperatures, or statuses.

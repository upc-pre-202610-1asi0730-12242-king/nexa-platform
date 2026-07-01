# Nexa Multi-Tenancy

Nexa uses shared database/shared schema multi-tenancy for MVP.

## Tenant And Workspace Resolution

Authenticated sessions now emit signed token claims:

- `sub`
- `email`
- `role`
- `tenant_id`
- `workspace_id`
- `workspace_slug`
- `membership_id`
- `client_account_id` for buyer memberships when applicable

The frontend may send `X-Nexa-Tenant-Id` and `X-Nexa-Workspace` as selectors, but the backend treats those headers as untrusted. Middleware compares headers with token claims and verifies the membership row is still active.

The frontend must not send `X-Nexa-User-Email` or `X-Nexa-Role` as authorization inputs. Role decisions are server-side.

## Backend Enforcement

Protected controllers use authorization policies:

- `WorkspaceMember`
- `CanManageWorkspace`
- `CanCreateOrder`
- `CanAcceptPurchaseRequest`
- `CanStartDispatch`
- `CanManageDocuments`
- `CanManageCatalog`
- `CanManageInventory`
- `CanManageSharedReferenceData`

Role policies now verify the active `UserWorkspaceMembership` row in the database. Claims identify the session, but membership data authorizes sensitive workspace actions.

Tenant-owned repositories fail closed when the authenticated tenant is absent. Command services assign tenant from `ICurrentWorkspaceContext`; request-body tenant ids are not authorization inputs. Membership middleware also verifies that the membership belongs to the signed-in user, not only to the tenant/workspace.

Buyer sessions are additionally scoped by the signed `client_account_id` claim. Clients, purchase requests, orders, payments, invoices, business documents, dispatch tracking, portal tasks, notifications, and related records are filtered to that client account where applicable.

Direct controllers for catalog, warehouse, logistics shipments, invoices, and payments require `WorkspaceMember`. Public catalog reads are intentionally not supported in the platform API; buyer-facing access must happen through an authenticated workspace session.

## Database Scope

Tenant-scoped core aggregates now include `tenant_id`:

- `orders`
- `order_items`
- `catalog_items`
- `inventory_items`
- `warehouses`
- `shipments`
- `invoices`
- `payments`
- `business_documents`
- `audit_logs`
- `client_accounts`
- `purchase_requests`
- `dispatch_orders`
- `notification_records`
- `customer_portal_tasks`

Unique constraints are scoped by tenant, for example `(tenant_id, order_number)` and `(tenant_id, catalog_item_id)`.

## Remaining Work

Workspace-level scoping can become stricter after MVP by adding `workspace_id` to operational rows where a tenant owns multiple operational workspaces. Row-level security is deferred to a later production hardening phase.

Row-level security is not enabled in PostgreSQL. Application middleware, policies, repositories, and command services are therefore mandatory enforcement layers and must remain covered by integration tests.

New frontend work should call collection endpoints scoped by the authenticated tenant/workspace instead of passing tenant ids in route paths.

Reference data endpoints are protected with `WorkspaceMember`. They are read-only and do not grant authorization by header values.

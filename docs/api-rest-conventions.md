# API REST Conventions

## Baseline

Nexa exposes versioned HTTP resources under `/api/v1`. Controllers are HTTP adapters only: they authorize, bind resources, call application services, and return stable response resources. Domain entities and EF entities must not be serialized directly.

## Resource Names

- Use plural resource nouns: `/orders`, `/purchase-requests`, `/client-accounts`.
- Prefer canonical business vocabulary over legacy aliases.
- Keep compatibility aliases while the Vue app or external clients still use them.
- Document aliases as deprecated before removal; do not remove existing endpoints during architecture hardening.

## Domain Transitions

Use `POST` to create transition subresources when a business lifecycle changes:

- `POST /api/v1/orders/{id}/confirmations`
- `POST /api/v1/orders/{id}/rejections`
- `POST /api/v1/orders/{id}/cancellations`
- `POST /api/v1/reservations/{id}/releases`
- `POST /api/v1/business-documents/{id}/status-changes`
- `POST /api/v1/notifications/{id}/reads`

Legacy imperative routes such as `/confirm`, `/reject`, `/cancel`, `/reserve`, `/release-reservation`, `/paid`, `/mark-ready`, and `/read` are compatibility routes. They must delegate to the same application services used by canonical routes.

## Collection Reads

- Keep existing unpaged array responses when no pagination query is provided.
- Add `page` and `pageSize` only as optional behavior to avoid breaking current frontend consumers.
- Use query parameters for filtering and sorting.
- Keep `/by-*` route filters as temporary aliases when already published.
- When `page`, `pageSize`, or supported filters are provided, return a paged envelope:
  - `items`: resource array using the same item resource shape as the legacy endpoint.
  - `page`: normalized current page, minimum `1`.
  - `pageSize`: normalized page size, default `25`, maximum `100`.
  - `totalItems`: total matching rows before pagination.
  - `totalPages`: total page count for the filtered result.

Examples:

- Keep `GET /api/v1/catalog-items/by-brand/{brand}`.
- Add `GET /api/v1/catalog-items?brand={brand}`.
- Keep `GET /api/v1/orders/by-status/{status}`.
- Add `GET /api/v1/orders?status={status}`.

Paged collection endpoints currently implemented:

- `GET /api/v1/orders?page=1&pageSize=25&status=Pending&clientAccountId=1&search=ORD-001&createdFrom=2026-01-01&createdTo=2026-12-31&sort=created_desc`
- `GET /api/v1/purchase-requests?page=1&pageSize=25&status=submitted&clientAccountId=1&priority=urgent&search=PR-001`
- `GET /api/v1/catalog-items?page=1&pageSize=25&brand=FrostKing&category=Frozen&coldChain=Frozen&active=true`
- `GET /api/v1/invoices?page=1&pageSize=25&paymentStatus=Pending&clientAccountId=1&orderId=10`
- `GET /api/v1/payments?page=1&pageSize=25&status=Pending&clientAccountId=1&orderId=10&invoiceId=20`
- `GET /api/v1/dispatch-orders?page=1&pageSize=25&status=scheduled&clientAccountId=1&orderId=10`
- `GET /api/v1/inventory-items?page=1&pageSize=25&productId=SKU-001&warehouseId=1`

If the domain model does not support a requested filter directly, prefer a truthful scoped join over a fake field. For example, inventory item `warehouseId` is resolved through inventory lots because the inventory item aggregate has no direct warehouse id column.

## Errors

- Return safe ProblemDetails-compatible errors from centralized middleware where possible.
- Do not expose stack traces, raw localization keys, secrets, or persistence details.
- Business-rule failures should be clear enough for the frontend to show a truthful state.

## Tenant Scope

- Tenant-owned reads and writes must be scoped by the authenticated workspace context.
- Header tenant/workspace values are hints; JWT claims and active membership validation remain authoritative.
- Generic repository reads must not become a bypass for tenant-owned resources.

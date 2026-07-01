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

Examples:

- Keep `GET /api/v1/catalog-items/by-brand/{brand}`.
- Add `GET /api/v1/catalog-items?brand={brand}`.
- Keep `GET /api/v1/orders/by-status/{status}`.
- Add `GET /api/v1/orders?status={status}`.

## Errors

- Return safe ProblemDetails-compatible errors from centralized middleware where possible.
- Do not expose stack traces, raw localization keys, secrets, or persistence details.
- Business-rule failures should be clear enough for the frontend to show a truthful state.

## Tenant Scope

- Tenant-owned reads and writes must be scoped by the authenticated workspace context.
- Header tenant/workspace values are hints; JWT claims and active membership validation remain authoritative.
- Generic repository reads must not become a bypass for tenant-owned resources.

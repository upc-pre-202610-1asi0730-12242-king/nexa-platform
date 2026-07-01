# Nexa REST API Guidelines

Nexa exposes a RESTful API for a modular monolith. Resource endpoints are preferred over RPC-style verbs.

## Canonical Rules

- Use `/api/v1/{resources}` for collections.
- Use `/api/v1/{resources}/{id}` for specific resources.
- Use subresources for state transitions: `/payments/{id}/confirmations`, `/dispatch-orders/{id}/route-starts`, `/purchase-requests/{id}/acceptances`.
- Do not expose EF entities from core controllers. Use request DTOs/resources and assemblers.
- Do not use `by-tenant` routes for new frontend work. The authenticated tenant/workspace scopes collection endpoints.
- All protected endpoints require JWT Bearer and active workspace membership.
- Tenant mismatch returns `403`; missing token returns `401`.
- Anonymous workspace discovery uses `GET /api/v1/tenants/by-slug/{slug}` and returns only name, slug, workspace URL, plan, and status.
- Full tenant/workspace resources require authentication. Tenant creation is handled by `/api/v1/organization-registrations`; workspace managers may update only their current tenant.

## Current Canonical Core Endpoints

| Context | Endpoint family | Notes |
|---|---|---|
| IAM | `/api/v1/authentication`, `/api/v1/users` | Sign-in returns signed workspace claims. |
| Tenant | `/api/v1/tenants`, `/api/v1/workspaces`, `/api/v1/organization-registrations` | Public slug preview is minimal; full resources are scoped by workspace membership. |
| Catalog | `/api/v1/catalog-items`, `/api/v1/categories`, `/api/v1/brands` | Cold-chain catalog. |
| Sales | `/api/v1/purchase-requests`, `/api/v1/orders`, `/api/v1/clients` | Purchase request to order flow. |
| Warehouse | `/api/v1/inventory-items`, `/api/v1/warehouses` | Stock and reservations. |
| Logistics | `/api/v1/dispatch-orders`, `/api/v1/temperature-logs`, `/api/v1/proof-of-delivery-records` | Dispatch and evidence. |
| Invoicing | `/api/v1/invoices`, `/api/v1/payments`, `/api/v1/business-documents` | Payment is core; PaymentProcessRecord is internal history. |
| Reference | `/api/v1/reference/*` | Read-only reference data. |

## Compatibility Routes

Some verb-style routes remain temporarily for existing clients. New webapp code uses canonical subresources such as `/purchase-requests/{id}/commercial-validations`, `/dispatch-orders/{id}/incidents`, and `/dispatch-orders/{id}/reschedules`. Compatibility routes must not bypass the same command service, policy, or tenant scope.

Swagger defines Bearer JWT by reference to the generated OpenAPI document. The production API validates that the JWT signing key is non-placeholder and at least 32 characters.

Database startup applies the checked-in EF Core migration chain. It never fabricates `__EFMigrationsHistory`; an unmanaged or drifted schema must be baselined explicitly outside application startup.

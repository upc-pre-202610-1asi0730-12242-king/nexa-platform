# Nexa Architecture Overview

Nexa is implemented as a pragmatic modular monolith. Each bounded context keeps its own Domain, Application, Infrastructure, and Interfaces folders where the current codebase already supports that separation.

## Backend Shape

- `Iam`: global users, authentication, workspace session claims.
- `TenantManagement`: tenants, workspaces, memberships, invitations, workspace-owned setup.
- `CatalogManagement`: tenant-scoped cold-chain catalog items.
- `Sales`: clients, purchase requests, purchase orders, commercial validation.
- `Warehouse`: tenant-scoped warehouses and inventory reservations.
- `Logistics`: dispatch orders, shipment tracking, route transitions.
- `Invoicing`: tenant-scoped invoices, payments, business documents, payment options, document status, and payment audit.
- `Shared`: cross-cutting infrastructure, persistence, security contexts, unit of work.

## Layer Rules

- Domain owns entities, aggregates, value objects, and business state transitions.
- Application coordinates use cases, policies, repositories, transactions, and command/query services.
- Infrastructure implements persistence, authentication handlers, adapters, and framework-specific services.
- Interfaces exposes REST controllers, resources, and assemblers.

Controllers must stay thin. Critical workflows go through Application command services. Entities are not the intended long-term API contract for critical resources.

## Application Surfaces

Core workflows use bounded-context command/query services, typed REST resources, assemblers, and dedicated repositories. The former generic `CrudController<TEntity>` and JSON-backed `WorkspaceResourceRecord` runtime surfaces are no longer part of the active model. The current migration source renames the old table to `legacy_workspace_resource_records` instead of deleting it, and EF Core does not map it. A database that applied an earlier draft of that migration may already lack the legacy table.

Canonical transition routes:

- `POST /api/v1/purchase-requests/{id}/acceptances`
- `POST /api/v1/dispatch-orders/{id}/route-starts`
- `POST /api/v1/dispatch-orders/{id}/deliveries`
- `POST /api/v1/dispatch-orders/{id}/incidents`
- `POST /api/v1/dispatch-orders/{id}/reschedules`
- `POST /api/v1/business-documents/{id}/status-changes`
- `POST /api/v1/payment-process-records/{id}/status-changes`
- `GET /api/v1/payments`
- `POST /api/v1/payments`
- `POST /api/v1/payments/{id}/confirmations`
- `POST /api/v1/payments/{id}/rejections`
- `POST /api/v1/payments/{id}/cancellations`
- `GET /api/v1/invoices/{invoiceId}/payments`

`Invoice` and `Payment` are now core tenant-scoped concepts in the Invoicing/Billing bounded context. The public payment API is `/api/v1/payments`. `PaymentProcessRecord` remains only as an internal operational trace/history surface and is not the primary frontend or public domain name. New work must not present `payment_process_records` as the main payment API.

## Current Priority

The first architectural boundary is workspace/tenant isolation. Nexa uses shared database/shared schema for MVP, so every operational table that can leak business data must carry a tenant/workspace scope and every protected request must validate membership server-side.

Mutation endpoints use capability policies for workspace administration, catalog, inventory, commercial, logistics, and document workflows. Categories and brands remain shared reference metadata; tenant users can read them, while only platform-level roles may mutate them.

Swagger/OpenAPI declares the Bearer JWT scheme so protected endpoints are not documented as anonymous by omission.

## Local Docker Runtime

The reproducible local stack is defined in `nexa-platform/docker-compose.yml` and must be run from the `nexa-platform` directory:

```bash
docker compose up --build
```

Expected services:

- `postgres` on port `5432`
- `api` on port `5068`
- `webapp` on port `5173`

The webapp Dockerfile lives in `../nexa-webapp/Dockerfile`. Vite runs with `--host 0.0.0.0` and `VITE_SERVER_OPEN=false` in Docker so the container does not attempt to open a browser.

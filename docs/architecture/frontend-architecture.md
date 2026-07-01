# Nexa Frontend Architecture

Nexa WebApp follows Vue 3 + Vite + Pinia + Vue Router with feature folders aligned to bounded contexts.

## Context Folders

| Context | Folder |
|---|---|
| IAM | `src/iam` |
| Tenant Management | `src/tenant-management` |
| Catalog | `src/catalog-management` |
| Sales / Buyer Portal | `src/sales` |
| Warehouse | `src/warehouse` |
| Logistics | `src/logistics` |
| Invoicing / Billing | `src/invoicing` |
| Shared | `src/shared` |

## Layer Rules

- `domain`: entities and value objects only.
- `application`: Pinia stores and use-case orchestration.
- `infrastructure`: API clients, endpoints, resources, assemblers.
- `presentation`: routes, views, components, UI state.

Core UI flows must not call Axios/fetch directly from views when an API client exists. Core Billing/Payments UI must use `/api/v1/payments`, not `/api/v1/payment-process-records`.

## Reusable Components

Reusable components currently used or expected:

- `ReferenceSelect`
- `StatusBadge`
- `PaymentOptionCard`
- `InvoiceSummaryCard`
- `DocumentStatusCard`
- state components for loading, empty, and error screens where present

New UI work should preserve the current visual design and extract reuse only where duplication creates maintenance risk.

# Role Focused Read Model Endpoints

## Purpose

These endpoints reduce frontend joins across full collections. They are read-side DTOs built from tenant-scoped PostgreSQL data through `IWorkspaceReadModelQueryService`; controllers remain HTTP adapters and do not expose EF/domain entities directly.

## Buyer

- `GET /api/v1/buyer/dashboard-summary`
  - Counts active purchase requests, active orders, pending buyer-visible documents, pending invoices, recent requests, recent orders, notifications, and credit summary when the authenticated membership has a client account.
- `GET /api/v1/buyer/orders/{id}/lifecycle`
  - Returns order summary, order items, dispatch summaries, dispatch events, temperature logs, buyer-visible documents, invoices, and payments.
- `GET /api/v1/buyer/financial-profile`
  - Returns the authenticated buyer client account, credit status, pending invoices, recent payments, payment method count, and document count.

## Sales

- `GET /api/v1/sales/order-summaries?page=1&pageSize=25`
  - Returns paged order summaries with client, total, requested delivery date, dispatch status, payment status, and item count.
- `GET /api/v1/sales/purchase-request-inbox?page=1&pageSize=25`
  - Returns paged commercial inbox rows with request, buyer/client, line count, last message preview, and commercial owner.
- `GET /api/v1/client-accounts/{id}/financial-profile`
  - Returns client identity, credit status, open orders, pending invoices, recent payments, payment method count, and document count.

## Logistics

- `GET /api/v1/dispatch-orders/{id}/summary`
  - Returns dispatch, linked order, client, last event, events timeline, proof-of-delivery status, and latest temperature reading.
- `GET /api/v1/orders/{id}/timeline`
  - Returns a chronological timeline from order, dispatch events, invoice records, and payments.

## Catalog

- `GET /api/v1/catalog-items/{id}/availability`
  - Returns catalog stock, linked inventory item, available/reserved stock, lot summary, cold-chain requirement, and latest movement type.
- `GET /api/v1/catalog/promotional-catalog?page=1&pageSize=25`
  - Returns paged active catalog rows with brand/category labels, current promotion code/label when available, price, and stock.

## Guardrails

- Missing calculable data returns `null`, empty arrays, or omitted optional summaries rather than fake values.
- Buyer-scoped requests are additionally constrained by `ClientAccountId` when the authenticated membership has one.
- These endpoints are additive; existing collection endpoints remain available for compatibility.

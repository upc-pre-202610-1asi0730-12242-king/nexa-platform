# Nexa DDD Decisions

## Aggregates

- `Tenant`, `Workspace`, `UserWorkspaceMembership`: access boundary and workspace activation.
- `CatalogItem`: published tenant catalog item.
- `ClientAccount`: tenant-scoped B2B buyer account.
- `PurchaseRequest`: buyer or commercial request before order conversion.
- `Order`: commercial order and order item consistency boundary.
- `InventoryItem`: stock availability and reservations.
- `DispatchOrder`: logistics workflow for an order.
- `Invoice`: formal billing document scoped by tenant and optionally linked to order/client.
- `Payment`: core payment aggregate for confirmed, pending, failed, rejected, or cancelled payment state.
- `BusinessDocument`: invoice/document readiness and buyer authorization.
- `PaymentProcessRecord`: internal operational payment trace linked to the core payment flow when history is needed.

## Value Objects

Existing value objects remain preferred for identifiers and domain values such as `OrderNumber`, `CatalogItemId`, `ProductId`, `Money`, `Quantity`, `WarehouseLocation`, and temperature ranges.

## Structured Delivery And Payment Data

Purchase requests now store buyer delivery/payment metadata in explicit columns:

- `delivery_address`
- `delivery_district`
- `delivery_city`
- `delivery_province`
- `delivery_reference`
- `payment_option`
- `shipping_estimate`

`comments` remains as temporary narrative text for existing UI screens and audit notes. New request creation should fill the structured fields first.

## Domain Methods Added Or Reinforced

- `Order.AssignTenant(...)`
- `Order.AssignClientAccount(...)`: normalizes the client relationship while retaining the historical customer code.
- `PurchaseRequest.ChangeStatus(...)`: validates status transitions and blocks changes after conversion.
- `PurchaseRequest.MarkAcceptedIntoOrder(...)`: requires prior commercial validation.
- `DispatchOrder.Assign(...)`
- `DispatchOrder.Schedule(...)`
- `DispatchOrder.StartRoute()`
- `DispatchOrder.Complete()`
- `DispatchOrder.ChangeStatus(...)`: routes critical transitions through invariant methods.
- `BusinessDocument.ChangeStatus(...)`: prevents invalid readiness/missing/backward transitions.
- `Payment.Confirm(...)`, `Payment.Reject(...)`, `Payment.Cancel(...)`: enforce positive amount, valid status transitions, and tenant-owned payment flow.
- `PaymentProcessRecord.ChangeStatus(...)`: limits internal payment trace transitions to supported states and prevents unsafe backward moves.
- `UserWorkspaceMembership.ChangeRole(...)`
- `UserWorkspaceMembership.Activate()`
- `UserWorkspaceMembership.Disable()`

## Events

Internal events are conceptually valid for:

- `PurchaseRequestConvertedToOrder`
- `DispatchRouteStarted`
- `DispatchCompleted`
- `BusinessDocumentUploaded`
- `MemberAssignedToWorkspace`

Nexa does not yet need a broker. Outbox is deferred until notifications/integrations require durable async delivery.

## Test Coverage

Current minimum domain tests cover invalid dispatch route start, invalid dispatch completion, negative inventory reservations, insufficient stock reservation, and purchase request conversion before validation.

Additional hardening tests cover invalid business document statuses, invalid payment process backward transitions, Payment amount validation, Payment double-confirm prevention, Payment reject-after-confirm prevention, Payment cancel-after-confirm prevention, and invalid Invoice totals.

API surface tests verify that critical workspace controllers require `WorkspaceMember` and that REST subresource routes exist for purchase request conversion and dispatch route/delivery transitions.

API surface tests also verify invoicing status-change subresources, Payment core subresources, and the internal history contract on `PaymentProcessRecordsController`.

`Microsoft.AspNetCore.Mvc.Testing` runs an integration security scenario against an isolated PostgreSQL test database. It covers anonymous access, valid tenant access, tenant mismatch, inactive and mismatched membership, tenant-isolated listings, client-body tenant tampering, and buyer client-account isolation across clients, orders, payments, and documents.

## Reference Data

Nexa now exposes read-only reference endpoints under `/api/v1/reference` for departments, provinces, districts, countries, payment options, delivery methods, document types, statuses, and units of measure.

This is an MVP compromise:

- Location labels remain denormalized on purchase requests as display snapshots.
- `payment_option` is validated against an allowed set before create/update.
- Future production work can add normalized FK columns such as `district_id` and `province_id` without deleting existing display text.

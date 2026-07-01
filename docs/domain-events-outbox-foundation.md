# Domain Events and Outbox Foundation

## Implemented Now

- `IDomainEvent`: stable contract for domain events.
- `DomainEvent`: base timestamped event record.
- `IHasDomainEvents`: aggregate contract for collected events.
- `DomainEventContainer`: reusable in-memory event collection base.

No aggregate workflow has been changed yet. No event dispatcher is active. No outbox table has been added in this phase.

## Candidate Events

- `PurchaseRequestSubmitted`
- `PurchaseRequestAccepted`
- `OrderCreated`
- `OrderConfirmed`
- `InventoryReserved`
- `DispatchOrderCreated`
- `InvoiceIssued`
- `PaymentRegistered`
- `DispatchOrderDelivered`

## Next Controlled Step

1. Add characterization tests around order creation and purchase-request acceptance.
2. Add an `outbox_messages` table in a reviewed non-destructive migration.
3. Record events in the same transaction as the aggregate state change.
4. Process events through idempotent application handlers.
5. Move cross-context order side effects from synchronous fan-out to outbox handlers only after runtime smoke is green.

## Guardrail

Do not publish events directly from controllers. Controllers remain HTTP adapters; application services own transaction boundaries and dispatch policy.

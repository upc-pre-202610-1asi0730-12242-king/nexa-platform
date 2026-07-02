# Domain Events and Outbox

Nexa aggregates inherit `DomainEventContainer`. Order confirmation, purchase-request submission/acceptance, inventory reservation creation, dispatch status changes, invoice payment, payment confirmation, and workspace creation raise explicit domain events.

`DomainEventOutboxInterceptor` serializes pending events into `outbox_messages` in the same `SaveChanges` transaction. Rows contain event id, UTC occurrence, CLR type, JSON payload, optional tenant/workspace scope, processing timestamp, error, and retry count.

Migration `20260702003115_AddDomainEventOutboxFoundation` is additive. Runtime validation confirmed table creation and a persisted `WorkspaceCreated` event.

Limitation: dispatcher, retry worker, idempotent consumer registry, retention, and operational metrics remain deferred. Existing synchronous workflows were not moved async, preventing duplicate side effects in this final pass.

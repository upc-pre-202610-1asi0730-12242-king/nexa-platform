# Nexa Future Considerations

These items are intentionally deferred. They are not part of the MVP hardening scope.

## Not Implemented Now

- Microservices: Nexa remains a modular monolith.
- Database per tenant: Nexa remains shared database/shared schema.
- Row-Level Security: tenant isolation is enforced in backend middleware, policies, repositories, and services.
- Full Outbox: deferred until Nexa has durable async integrations or notifications that require exactly-once delivery semantics.
- Workspace-level database scope on every operational table: current MVP boundary is tenant-level isolation. Add `workspace_id` only if multiple operational workspaces per tenant become a real product requirement.

## Next Safe Steps

- Remove legacy compatibility routes after consumers use only canonical transition subresources.
- Move direct `AppDbContext` controller actions into Application command/query services.
- Add tenant-scoped reference tables for districts/provinces/departments if location search/reporting becomes important.
- Replace remaining string statuses with enums/value objects or validated reference values context by context.
- Keep improving the core `invoices` and `payments` tables with tenant-safe accounting rules. `PaymentProcessRecord` should remain internal history unless a future migration fully absorbs it into `payments`.

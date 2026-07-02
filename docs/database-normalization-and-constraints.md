# Database Normalization and Constraints

Operational data uses atomic columns and separate aggregate tables (1NF), full-key dependencies on single/composite keys (2NF), and referenced tenant/catalog/order/client/workspace entities instead of repeated master data (3NF). Alternate keys and tenant-aware composite foreign keys support stronger determinants (BCNF direction). Multi-valued lines, memberships, events, and preferences are separate relations (4NF). 5NF is not claimed beyond current verified join dependencies.

Historical order/invoice snapshots are intentional audit data, not accidental duplication.

Local preflight previously found zero invalid quantity/amount/date rows. CHECK constraints remain deferred until target-environment preflight is repeated; exact SQL is in `database-check-constraint-plan.md`. Workspace slug uniqueness is enforced in application and database. Migration `20260702003115_AddDomainEventOutboxFoundation` only adds `outbox_messages`; no destructive normalization ran.

# Multi-Tenancy Hardening Plan

## Implemented Now

- Added `ITenantScoped` for tenant-owned domain models that already expose `TenantId`.
- Added a generic repository guard: tenant-scoped entities cannot use unscoped generic `FindByIdAsync` or `ListAsync`.
- Added `ApplyTenantScope` query helper for tenant-scoped EF queries.

These changes are non-destructive and do not alter database schema.

## Deferred Deliberately

EF Core global query filters are still deferred. They are useful defense in depth, but enabling them broadly can break:

- startup seed flows that run without an HTTP workspace context,
- migrations and design-time tooling,
- platform/admin queries,
- global reference data,
- explicit cross-context read models.

## Next Safe Step

1. Replace repeated repository predicates with `ApplyTenantScope` in one bounded context at a time.
2. Add characterization tests for seed, login, admin, buyer, sales, logistics, and owner reads.
3. Enable EF global filters only after bypass policy is explicit for seed/admin/reference data.
4. Verify runtime smoke after each step:
   - no token -> `401`,
   - wrong tenant/workspace -> `403`,
   - correct tenant/workspace -> `200`.

## Guardrail

No tenant-owned query should depend on the generic `BaseRepository` read methods. Context repositories must apply current tenant/workspace scope explicitly.

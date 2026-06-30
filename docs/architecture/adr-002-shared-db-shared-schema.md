# ADR 002: Shared Database And Shared Schema

## Status

Accepted.

## Context

Nexa is a multi-tenant SaaS. Database-per-tenant is unnecessary for MVP and raises operational complexity.

## Decision

Use one database and one schema. Tenant/workspace isolation is enforced by token claims, backend policies, scoped repositories, and database constraints.

## Consequences

- Lower local/dev/deploy complexity.
- Every sensitive operational table must carry tenant/workspace scope.
- Unique indexes must include tenant scope when business uniqueness is tenant-local.
- Backend queries must not trust frontend-provided tenant ids without membership validation.

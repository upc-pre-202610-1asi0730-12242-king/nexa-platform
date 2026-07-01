# ADR 001: Modular Monolith

## Status

Accepted.

## Context

Nexa needs multiple B2B domains but does not need microservice operational cost for MVP or academic delivery.

## Decision

Use a modular monolith by bounded context. Keep Domain, Application, Infrastructure, and Interfaces inside modules where possible. Shared code remains limited to cross-cutting infrastructure and primitives.

## Consequences

- Lower deployment complexity.
- Clear context ownership.
- Easier transactional workflows across sales, logistics, invoicing, and warehouse.
- Future extraction remains possible only after boundaries prove stable.

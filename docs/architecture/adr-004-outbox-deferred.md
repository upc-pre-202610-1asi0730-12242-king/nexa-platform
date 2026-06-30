# ADR 004: Outbox Deferred

## Status

Deferred.

## Context

Nexa has meaningful domain events, but current workflows are still synchronous inside one monolith and one database.

## Decision

Do not implement a full outbox yet. Keep events documented and use direct transactional writes for order conversion, dispatch, documents, payment traces, messages, and notifications.

## Consequences

- Less infrastructure for MVP.
- No message broker required.
- Future outbox table should be introduced when notifications, integrations, or async processors need durable delivery.

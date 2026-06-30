# ADR 003: Backend Authorization Policies

## Status

Accepted.

## Context

Frontend guards improve UX but cannot protect data. Nexa must prevent cross-workspace access at the backend boundary.

## Decision

Use ASP.NET Core authentication plus policy-based authorization. The token identifies user, tenant, workspace, and membership. Middleware validates membership liveness on protected requests.

Sensitive role policies must load the active `UserWorkspaceMembership` from the database and compare its role against policy roles. Role claims are session hints, not the final authorization source.

## Consequences

- Endpoints return `401` without token.
- Endpoints return `403` for inactive/mismatched membership.
- Endpoints return `403` when a valid token carries a membership that no longer has the required DB role.
- Role/permission rules live server-side and can evolve without trusting browser state.
- Frontend must only send `Authorization: Bearer <token>` plus workspace/tenant selector headers. `X-Nexa-User-Email` and `X-Nexa-Role` are not authorization inputs.
- Catalog, inventory, warehouse, shipment, invoice, and payment controllers require workspace membership.

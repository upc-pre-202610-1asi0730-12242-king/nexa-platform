# Nexa Platform - Architecture Fix Roadmap for Professor Review

## 1. Purpose

This roadmap converts the evidence in `docs/architecture-audit-professor.md` into prioritized, verifiable actions. It covers REST, tactical DDD, layering, patterns, PostgreSQL/EF Core, frontend architecture, integration reliability, and deployment. It favors small compatible changes. It does not authorize destructive migrations, remote history changes, or a microservice rewrite.

## 2. Critical Fixes Before Professor Audit

| Priority | Area | Issue | File/Module | Fix | Acceptance criteria |
|---:|---|---|---|---|---|
| P0 | Order reliability | API can persist order and downstream records before a late response failure | `OrderCommandService.CreateAsync`; Sales manual-order flow | Keep frontend reconciliation by order code; document current non-atomic workflow | Repeated submit gives one visible order and never reports “not created” when order exists |
| P0 | Build/test | Professor can immediately test compilation | Backend and frontend roots | Run official build/test sequence and preserve output | Backend 0 warnings/errors, 40/40 tests, frontend Vite build succeeds |
| P0 | Runtime | Main authenticated path must be demonstrable | Docker API/webapp/PostgreSQL | Verify health, auth, tenant mismatch, and order reads | `/health=200`, no token `401`, wrong scope `403`, correct scope `200` |
| P0 | Defense docs | Architecture claims need evidence and honest caveats | `docs/architecture-*.md` | Keep audit and roadmap current | Every major claim points to a current class, endpoint, table, or runtime result |
| P0 | Error contract | `UnexpectedErrorDetail` localization key leaked to UI | `SharedResource`, global exception middleware | Use resource namespace matching embedded `.resx` | Generic errors return localized text, never resource keys |
| P1 | REST | Unpaginated collections and compatibility action routes | Orders, requests, inventory, logistics, invoicing | Do not rewrite before review; document conventions and aliases | Professor can distinguish canonical resources from compatibility debt |
| P1 | DDD | No domain events and uncertain cross-context order fan-out | Sales/Warehouse/Invoicing/Logistics | Present outbox/domain-event plan; avoid risky last-minute rewrite | Event candidates, owners, consumers and idempotency are documented |
| P1 | Database | Zero CHECK constraints and known duplicated concepts | EF model/migrations | Run read-only validation; defer schema changes pending approval | Live counts/FKs/indexes/risks are available for defense |
| P1 | Deploy | Render secrets and migration policy incomplete | `render.yaml`, `Program.cs` | Document required environment and safe migration policy | No production claim depends on local defaults |

Only P0 documentation, validation, and the already-requested Sales reliability correction should be changed immediately. Event architecture, endpoint migration, and schema hardening require controlled follow-up.

## 3. REST Fix Plan

| Current endpoint | Keep/deprecate/change | Reason | New endpoint if needed | Compatibility strategy |
|---|---|---|---|---|
| `POST /purchase-requests/{id}/submissions` | Keep | Represents a domain transition resource | None | Standardize response and idempotency |
| `POST /purchase-requests/{id}/commercial-validations` | Keep | Explicit Sales transition | None | Document allowed prior states |
| `POST /purchase-requests/{id}/acceptances` | Keep | Acceptance creates a durable business transition | None | Add idempotency key/result link |
| `POST /payments/{id}/confirmations` | Keep | Correct transition-subresource style | None | Reuse convention elsewhere |
| `POST /reservations/{id}/releases` | Keep | Correct resource-oriented release transition | None | Canonical route |
| `POST /inventory-items/{id}/reserve` | Deprecate | Imperative RPC route hides reservation identity | `POST /reservations` | Keep old route for one version; add `Deprecation`/`Sunset` headers |
| `POST /inventory-items/{id}/release-reservation` | Deprecate | Imperative and ambiguous reservation | `POST /reservations/{id}/releases` | Adapter calls canonical service; document replacement |
| `POST /orders/{id}/confirm` | Change | Singular verb inconsistent with newer transitions | `POST /orders/{id}/confirmations` | Keep old alias until frontend migrates |
| `POST /orders/{id}/reject` | Change | Same issue | `POST /orders/{id}/rejections` | Compatibility alias |
| `POST /orders/{id}/cancel` | Change | Same issue; DELETE also overlaps | `POST /orders/{id}/cancellations` | Mark action and DELETE route deprecated |
| `POST /invoices/{id}/paid` | Deprecate | Duplicates generic status change/payment lifecycle | `/invoices/{id}/status-changes` or payment confirmation | Keep adapter and warn in OpenAPI |
| `PUT /business-documents/{id}/mark-ready` | Deprecate | Imperative partial transition | `POST /business-documents/{id}/status-changes` | Retain alias temporarily |
| `/stock-movements` | Deprecate alias | Duplicate vocabulary | `/inventory-movements` | Same service; deprecation metadata |
| `/notifications` and `/notification-records` | Consolidate | Two names plus duplicate read routes | `/notifications`, `POST /{id}/reads` | Frontend migrates first; keep aliases one version |
| `/clients` and `/client-accounts` | Consolidate | Same Sales concept | `/client-accounts` | Alias `/clients`; document canonical term |
| `GET */by-*` paths | Change gradually | Filtering is not a child resource | `GET /resource?field=value` | Support both shapes until all clients migrate |
| All collection GETs | Change | Full arrays do not scale | `?page=1&pageSize=25&sort=-createdAt` plus envelope | Add paged endpoint/optional query without breaking existing clients |
| Buyer/Sales/Logistics detail joins | Add read models | Frontend currently joins many full collections | `/buyer/orders/{id}/view`, `/sales/order-summaries`, `/logistics/dispatches/{id}/view` | Additive endpoints; no current route removal |

Official conventions belong in `docs/api-rest-conventions.md`:

- `/api/v1` and plural nouns.
- Query parameters for filtering/sorting/paging.
- `POST` for resource creation and explicit transition subresources.
- `PATCH` for partial simple state; `PUT` only for complete idempotent replacement.
- `DELETE` only for conceptual deletion, not lifecycle cancellation.
- `application/problem+json` for failures.
- Stable request/response resources and assemblers; no domain serialization.
- Additive rollout followed by documented deprecation; never break the current Vue app first.

## 4. DDD Fix Plan

| Bounded Context | Current issue | Fix | Risk | Acceptance criteria |
|---|---|---|---|---|
| Sales | Commands live in `Domain/Model/Commands` and aggregates consume them | Move use-case commands to Application; introduce domain creation/update methods or factories | Broad namespace churn | Domain compiles without Application/Infrastructure dependency and use cases still pass |
| Sales | `PurchaseRequest` has public setters despite being lifecycle owner | Promote to aggregate; private setters; line operations through methods | EF materialization/migration compatibility | Invalid transitions/negative values cannot be constructed through public API |
| Sales | Order creation commits before synchronous downstream fan-out finishes | Commit core order once, write outbox events in same transaction | Requires event infrastructure | Client receives deterministic result; retries do not duplicate reservations/docs/dispatch |
| Warehouse | Catalog stock and inventory availability can drift | Define inventory as stock authority; catalog consumes availability projection | UI/contract changes | One invariant owner and reconciliation test |
| Warehouse | Reservation creation scans/coordinates multiple records | Add reservation domain service/specification where rule spans lots/items | Allocation behavior regression | Deterministic allocation tests and explicit shortage result |
| Logistics | `DispatchOrder.ChangeStatus` accepts arbitrary default statuses | Typed status VO and complete transition matrix | Existing seed/status compatibility | All current valid transitions pass; unknown status rejected |
| Logistics | Shipment and DispatchOrder overlap | Document context map and canonical fulfillment aggregate | Premature deletion risk | Every endpoint/table has a declared owner and migration plan |
| Invoicing | Invoice/Payment and operational process records overlap | Define canonical aggregate vs process/read records | Billing UI compatibility | One documented source of truth per status/amount |
| Tenant/IAM | `TenantMember`, membership, and user roles overlap | Make IAM user identity global; workspace membership owns authorization; deprecate duplicate tenant member | Admin UI migration | Role policy reads one source; compatibility DTO remains during migration |
| All | No domain events | Add event base, aggregate recording, dispatcher/outbox, idempotent handlers | Infrastructure complexity | Events persisted atomically and each consumer handles duplicate delivery |
| All | Some value objects are wrappers only | Add normalization, range and equality rules where business meaning exists | Over-modeling | VO introduced only where it removes invalid primitive states |

Candidate domain services:

- `InventoryReservationAllocator`: allocation across inventory lots/items.
- `CommercialCreditPolicy`: order/request eligibility across client credit and order amount.
- `OrderPricingService`: only if promotions/taxes require cross-aggregate calculation.
- `DispatchSchedulingPolicy`: route/window/temperature constraints across dispatch resources.

Candidate event order:

```text
PurchaseRequestSubmitted
  -> PurchaseRequestAccepted
  -> OrderCreated
  -> InventoryReserved
  -> DispatchOrderCreated
  -> OrderConfirmed
  -> InvoiceIssued / PaymentRegistered
  -> DispatchOrderDelivered
```

## 5. Layering Fix Plan

| Layer | Issue | Fix | Safe now? | Later? |
|---|---|---|---|---|
| Interfaces | `ReferenceDataController` queries `AppDbContext` | Introduce `IReferenceDataQueryService` and infrastructure repository | Yes, small | Next controlled refactor |
| Interfaces | Multi-controller files are 376-455 lines | Move each controller to one file without behavior change | Yes | Cosmetic, not before core reliability |
| Application | `PurchaseRequestCommandService` is 447 lines | Split submit, validation, acceptance, messaging use cases | Partly | After characterization tests |
| Application | `OrderCommandService` synchronously calls three contexts after commit | Outbox/event handlers with idempotency | No last-minute rewrite | Highest architectural follow-up |
| Domain | Aggregate constructors depend on command classes | Application command -> domain factory/value parameters | Moderate | Incremental per aggregate |
| Infrastructure | Generic BaseRepository exposes unscoped list/find | Restrict to global entities or tenant-aware specification | Yes with usage audit | Before more tenant features |
| Shared | Shared owns central DbContext, security, audit, reference data | Keep shared infrastructure; move business reference ownership to named module/facade | No broad split now | Evolve module ownership |
| Persistence | One `AppDbContext` spans modules | Keep one physical context for current monolith; move entity configurations into module assemblies | Yes | Separate DbContexts only if boundaries/transactions justify it |
| Frontend Application | `data.store.js` is a God Store | Extract `sales`, `warehouse`, `logistics`, `invoicing` stores behind focused actions | Moderate | Incremental route by route |
| Frontend Presentation | Screens depend on browser joins | Backend read-model APIs + small view stores | Requires backend | High-priority later |

## 6. Design Patterns Fix Plan

| Pattern/Smell | Current state | Fix | Priority |
|---|---|---|---:|
| Repository | Context ports/adapters are sound | Document why context repositories add tenant/query semantics over EF | P1 |
| Unit of Work | Works per EF save, not full order workflow | Define transaction boundary and outbox in same UoW | P0 architecture |
| DTO/Assembler | Strong HTTP boundary | Add contract tests and validation | P1 |
| Application Service | Correct but some services are large | Split by use case after tests | P1 |
| Middleware/Interceptor/DI | Well applied | Document pipeline order and audit behavior | P1 |
| Adapter ports | Cross-context interfaces exist | Back with event handlers and idempotency | P0 architecture |
| Specification | Missing for repeated tenant/status/date queries | Add query specifications only for meaningful reuse | P2 |
| Domain Events | Missing | Aggregate events + transactional outbox | P0 architecture |
| Factory | Missing for complex order/request creation | Domain factories with validation result | P1 |
| Domain Service | Limited | Add only for rules spanning aggregates | P1 |
| God Store | Confirmed | Context stores and server read models | P1 |
| Primitive Obsession | Status and configuration strings | Typed VOs/enums with EF converters | P1 |
| Data Clumps | Delivery/address fields repeat | Context value object and mapper | P2 |
| Transaction Script | Present in long operational services | Move state rules into aggregates; keep orchestration in Application | P1 |

## 7. Database Fix Plan

No destructive or broad migration should be executed before professor review without explicit approval. Each schema change needs a generated migration, reviewed SQL, backup/rollback note, and tenant-integrity smoke test.

| Table | Issue | Fix | Migration needed? | Risk | Priority |
|---|---|---|---|---|---:|
| catalog_items | Brand/category labels duplicate catalogs | Add `brand_id/category_id`; decide whether text remains immutable snapshot | Yes | Existing seed/data mapping | P1 |
| user_workspace_memberships | Duplicated full name/email | Document snapshot semantics or read profile from users | Maybe | Historical display vs current profile | P1 |
| tenant_members | Overlaps workspace membership | Deprecate and migrate admin UI/data to memberships | Yes later | User/admin compatibility | P1 |
| orders | Missing status+created query index | Add `(tenant_id,status,created_at)` | Yes | Low write overhead | P1 |
| purchase_requests | Same recency query gap | Add `(tenant_id,status,created_at)` | Yes | Low | P1 |
| dispatch_orders | Same recency query gap | Add `(tenant_id,status,created_at)` | Yes | Low | P1 |
| order_items | No positive quantity/nonnegative money checks | Add validated CHECK constraints after cleanup query | Yes | Existing invalid rows | P1 |
| purchase_request_lines | No positive quantity/weight checks | Add CHECK constraints | Yes | Existing data | P1 |
| inventory_items/lots/reservations | No nonnegative/relationship quantity checks | Add checks for available/reserved/units; define reservation invariant | Yes | Current sign conventions | P0 data integrity |
| payments/invoices/processes | No positive amount/derived-total checks | Add amount checks; define rounding/tolerance | Yes | Decimal/legacy values | P1 |
| temperature_logs/lots/warehouses | No min/max/physical range checks | Add min <= max and agreed operating range | Yes | Domain-specific range selection | P2 |
| business_documents | File name only | Add storage key, provider, checksum, MIME and size | Yes | Storage integration | P2 |
| tenant-scoped tables | No EF global filters | Introduce `ITenantScoped` and request-scope filters with explicit admin bypass | Model migration usually no | Seed/admin query behavior | P0 security |
| all auditable entities | Soft-delete/status inconsistent | Document per-aggregate lifecycle; do not add universal `IsDeleted` blindly | Maybe | Query semantics | P2 |
| migrations | 26 migrations, some broad hardening changes | Keep append-only; review generated SQL; separate data backfill from constraints | Yes | Deployment locks/failure | P0 process |
| seed | Demo data is development-controlled | Make idempotency and environment gate explicit | No/Maybe | Accidental production data | P0 deploy |

Pre-migration validation queries should inspect `information_schema.columns`, then null/invalid values, duplicates, FK violations, and index usage. The validated baseline is 84 FKs, 42 composite tenant FKs, 13 alternate constraints, 171 indexes, and 0 checks.

## 8. Frontend Fix Plan

| Module | Issue | Fix | Backend dependency | Priority |
|---|---|---|---|---:|
| `data.store.js` | 1,474 lines and 23 collection loader calls | Extract bounded-context stores and focused composables | None initially | P0 maintainability |
| Core loader | Errors become empty arrays | Per-resource loading/error/last-good state | None | P0 correctness |
| Sales manual order | Uncertain response could show false failure | Reconcile failed POST by unique order code; route to persisted result | Existing order list/by-number | Done/P0 |
| Sales order creation | Full global reload after mutation | Update local focused store then invalidate targeted queries | Paged/query APIs useful | P1 |
| Buyer portal | Browser joins order/dispatch/docs/payments | Buyer order lifecycle read model | Yes | P1 |
| Sales inbox | Full purchase request arrays | Paged Sales inbox summary endpoint | Yes | P1 |
| Logistics dispatch | Multiple event/evidence/temp collections | Dispatch detail read model with cursor telemetry | Yes | P1 |
| HTTP errors | Raw server/localization keys can reach toast | Error adapter for ProblemDetails and correlation id | ProblemDetails consistency | P0 |
| Router | History mode with hash-based 401/403 redirects | Router push or history URLs consistently | None | P1 |
| Auth token | localStorage bearer token | CSP now; evaluate secure cookie/BFF for production | Backend auth change | P2 |
| Filters | Mixed component/localStorage behavior | URL query as canonical state | Paged filters | P2 |
| Mocks/fallbacks | No active initial dataset, which is good | Add explicit error/empty states; CI grep preventing core mock fallback | None | P1 |
| DTO alignment | Assemblers normalize multiple naming shapes | Remove legacy shapes after API compatibility window | API deprecation plan | P2 |

## 9. Deploy Fix Plan

| Deploy area | Fix | Acceptance criteria |
|---|---|---|
| Render blueprint | Declare JWT secret, issuer, audience, exact CORS origin and migration/seed flags | Fresh service boots without local appsettings and without placeholder secrets |
| Port | Choose `PORT` binding or explicitly document `ASPNETCORE_URLS=:8080` | Render health check reaches application consistently |
| Connection string | Support standard `ConnectionStrings__DefaultConnection`; normalize `DATABASE_URL` if used | Production connects with SSL policy and no credential logging |
| Migrations | Default `APPLY_MIGRATIONS_ON_STARTUP=false`; run one release job | Concurrent app instances never race migrations |
| Seed | Require Development plus explicit `SEED_DEMO_DATA=true` | Production never inserts demo users/data |
| CORS | Production exact frontend origin; localhost only in Development | Unknown origins fail preflight; deployed frontend succeeds |
| Health | Split `/health/live` and `/health/ready` with PostgreSQL readiness | Liveness independent of DB; readiness fails when DB unavailable |
| Swagger | Keep production disabled by default | `/swagger` unavailable in production unless explicit secure switch |
| JWT | Generated secret >= 32 chars; rotation procedure | Startup validation passes and old-token policy is known |
| Data protection | Persistent disk/external key store if required | Restarts do not invalidate protected payloads |
| Runtime user | Run container as non-root | Container passes smoke under non-root UID |
| Package versions | Upgrade EF Core/Npgsql to supported .NET 10-compatible major versions together | Build/tests/migrations/runtime pass with no restore warnings |
| Logging | Correlation id, structured request logs, no secrets/PII | One order request trace can be followed across handlers |
| Rollback | Document image/database compatibility and backup step | Failed release can return to prior image without schema damage |

## 10. Recommended Local Commits

These are proposed commits only. Do not push without explicit approval.

1. `docs: add professor architecture audit`
2. `docs: add architecture fix roadmap`
3. `docs: add api rest conventions`
4. `docs: add ddd and design patterns defense notes`
5. `docs: add database validation queries`
6. `chore: verify backend build before professor audit`
7. `chore: verify frontend build before professor audit`
8. `fix(api): add missing health endpoint if absent`
9. `fix(docs): document render deployment variables`
10. `refactor(api): deprecate clearly rpc-like endpoints if safe`

Because this worktree already contains broad pre-existing changes, any commit must stage exact files only. The Sales uncertain-response fix and localization correction should be separate from documentation if committed later.

## 11. Final Professor Defense Script

“Nexa is a multi-tenant B2B platform for cold-chain commercial and logistics operations. We implemented it as a modular monolith because one deployment and one transactional database are appropriate for the current scale, while bounded contexts preserve business ownership and future extraction options.

The backend has Catalog, IAM, Tenant Management, Sales, Warehouse, Logistics and Invoicing modules. Each follows Domain, Application, Infrastructure and REST Interface layers. Controllers expose versioned resource DTOs, assemblers translate contracts, application services coordinate use cases, aggregates and value objects protect selected rules, and repository interfaces isolate EF Core adapters.

REST uses `/api/v1`, plural resources and transition subresources. Purchase-request acceptances and payment confirmations are deliberate domain transitions, not arbitrary verbs. We retain some compatibility RPC routes, especially inventory reserve/release, and have a non-breaking deprecation plan. Pagination and dedicated screen read models are the main API improvements.

PostgreSQL is a shared schema by design. Tenant identity travels in signed JWT claims, active membership is revalidated by middleware, policies authorize roles, repositories apply tenant predicates, and 42 composite foreign keys prevent cross-tenant relationships. We still want global query filters as defense in depth.

The Vue frontend mirrors bounded contexts, uses Pinia, guards, API clients and assemblers, and consumes the real backend. Its main debt is a large global data store and browser-side joins; backend read models will reduce that.

Deployment uses a .NET 10 multi-stage Docker image, Render and managed PostgreSQL. Health and production Swagger behavior exist, but secrets, readiness and migration execution need production hardening.

The largest architectural risk we found is synchronous order fan-out: after the order commit, inventory, documents and dispatch are provisioned in separate steps. We corrected the immediate false-failure UX and would next use an outbox with idempotent domain-event handlers. We describe the system honestly as defendible with observations, not as finished.”

## 12. Final Readiness Checklist

- [x] Backend builds
- [x] Frontend builds
- [x] Backend tests pass (40/40)
- [x] Main authenticated order creation smoke returns `201`
- [ ] Full Buyer -> Sales -> Logistics -> Buyer browser flow re-run after final image rebuild
- [x] REST conventions documented in the audit
- [x] DDD structure documented
- [x] Patterns documented
- [x] DB schema and live constraint counts documented
- [x] Render deploy risks documented
- [x] Known risks documented
- [x] Professor Q&A prepared
- [ ] Production env/secrets/readiness/migration policy implemented and verified
- [ ] Domain-event/outbox design implemented
- [ ] Pagination/read-model migration implemented

Readiness verdict: **Defendible with observations**. If time is short, do not attempt a broad event-system, endpoint, or schema rewrite. Preserve green builds, demonstrate tenant isolation and one main flow, and use this roadmap to state the next controlled improvements.

# Nexa Platform - Architecture Audit for Professor Review

Audit date: 2026-06-30  
Scope: `nexa-platform`, `nexa-webapp`, `nexa-website`, the running Docker environment, PostgreSQL schema, and workspace references in `references/` and `nexa-saas-library/`.

This assessment uses the project's current code and runtime evidence. The conceptual criteria are DDD, Clean Architecture, PoEAA, REST, dependency inversion, data normalization, and modular-monolith boundaries. No exact book page is claimed.

## 1. Executive Summary

Nexa is a working B2B cold-chain SaaS implemented as a **modular monolith with a shared PostgreSQL database**. The backend combines layered architecture and tactical DDD: each business module has `Domain`, `Application`, `Infrastructure`, and `Interfaces/Rest`; controllers delegate to application services; repositories are domain ports with EF Core adapters; resources and assemblers isolate HTTP contracts. The Vue frontend follows the same bounded-context vocabulary and calls the real API.

The project is **Defendible with observations**. The architecture is more than folder decoration: aggregates such as `Order`, `InventoryItem`, `Invoice`, and `Shipment` protect meaningful invariants; `PurchaseRequest` and `DispatchOrder` implement state transitions; 42 database foreign keys include `tenant_id`; and runtime authorization proves tenant/header/account isolation. However, it is not yet a strict Clean Architecture implementation or a complete event-driven DDD model.

Main strengths:

- Eight explicit backend modules: Catalog Management, IAM, Invoicing, Logistics, Sales, Shared, Tenant Management, and Warehouse.
- 12 aggregates, 33 value-object classes, 23 repository interfaces, 50 REST resource files, and 43 transform/assembler files.
- 183 Swagger paths and 306 operations under `/api/v1`; transition subresources model important business actions.
- Shared-schema tenant hardening: 84 FKs, including 42 tenant-aware composite FKs, plus 13 alternate unique constraints.
- Real Vue-to-API integration with JWT, tenant/workspace headers, role guards, API adapters, and no active JSON-server dataset.
- Current validation: backend builds with 0 warnings, 40/40 tests pass, frontend production build passes, `/health` returns successfully, and an authenticated order POST returns `201`.

Main risks:

- No domain-event mechanism exists; cross-context consequences are synchronous service calls.
- `OrderCommandService.CreateAsync` commits the order, then performs inventory, invoicing, notification, and logistics writes in separate units of work. A late failure can leave a created order while the client receives failure. This occurred in the Sales UI and produced `UnexpectedErrorDetail` after order `SAL-ORD-2026-979868` had already been persisted.
- `data.store.js` is a 1,474-line God Store that loads 23 collections and performs cross-context joins/read-model composition in the browser.
- Most collection endpoints are unpaginated and several filters are encoded as route segments.
- `POST /inventory-items/{id}/reserve` and `/release-reservation` coexist with the better `/reservations` resource.
- No EF global query filters and the generic `BaseRepository` is not tenant-aware. Tenant safety depends on every context repository doing the right thing.
- Database has zero check constraints; catalog and membership data contain acknowledged duplication.
- Production startup always calls `Database.Migrate()`. Render configuration omits explicit JWT secret, migration policy, persistent data-protection keys, and readiness checks backed by PostgreSQL.
- Backend targets .NET 10 while EF Core and Npgsql provider packages remain on major version 9.

### Score

| Area | Score /10 | Evidence | Professor risk | Defense argument |
|---|---:|---|---|---|
| REST API Design | 7.0 | `/api/v1`, resources/assemblers, 306 operations, transition subresources | Unpaginated lists, route-segment filters, compatibility RPC routes | Resource-oriented baseline is real; compatibility debt is explicitly catalogued. |
| DDD tactical patterns | 6.5 | 12 aggregates, 33 VOs, transition methods and repository ports | No domain events; commands live in Domain; some operational models remain mutable records | Tactical DDD is applied selectively, not claimed as pure DDD. |
| Bounded Contexts | 8.0 | Eight modules with independent four-layer folders and DI registration | Direct application-level cross-context interfaces; one shared DbContext | A modular monolith preserves boundaries without premature distributed systems. |
| Layering | 7.0 | Controllers -> application services -> domain ports -> EF adapters | `ReferenceDataController` directly uses `AppDbContext`; broad Shared module | Dependency direction is generally correct with named exceptions. |
| Design Patterns | 7.5 | Repository, UoW, Data Mapper, DTO, Assembler, DI, Middleware, Adapter | God Store; missing events/specifications/factories | Patterns solve actual boundaries and are not merely listed. |
| Database normalization | 7.0 | 49 business tables, separated line/event/pivot tables, 84 FKs | Text brand/category, duplicated user profile fields, zero CHECK constraints | Core transactions are normalized; selected read/snapshot duplication needs governance. |
| Multi-tenancy | 8.0 | JWT claims, membership middleware, scoped repositories, 42 composite FKs | No global filters; generic repository is unsafe for tenant entities | Isolation is enforced in application code and DB relationships, with defense in depth still improvable. |
| Frontend architecture | 6.5 | Vue modules, Pinia, API layer, assemblers, guards, lazy routes | 1,474-line God Store, browser joins, hash redirects with history router | Module structure is credible; global orchestration is declared debt. |
| Backend/frontend integration | 7.5 | Buyer/Sales/Logistics/Invoicing/Owner routes use real `/api/v1` clients | Synchronous order fan-out caused an uncertain response; no dedicated screen read models | Main business chain is persisted end to end and the observed failure has concrete remediation. |
| Deploy readiness | 6.0 | Multi-stage Dockerfile, Render blueprint, `/health`, production Swagger off | Unconditional migrations, incomplete secrets/config, shallow health check | Deploy scaffolding exists; configuration hardening is required before production. |
| Maintainability | 6.5 | 40 tests, XML docs, modular files, DI | Large service/controller files, shared database, package-major mismatch | Current tests and boundaries support change, but hotspots are explicitly prioritized. |

## 2. Real Architecture Map

### Technology and entry points

| Concern | Current implementation | Evidence |
|---|---|---|
| Backend | ASP.NET Core, .NET 10, C# 14, EF Core 9.0.16, Npgsql 9.0.3, Swagger | `King.Nexa.Platform/King.Nexa.Platform.csproj`, `Program.cs` |
| Frontend | Vue 3.5, Vite 5, Pinia 2, Vue Router 4, Axios, PrimeVue 4, vue-i18n | `nexa-webapp/package.json`, `src/main.js` |
| Database | PostgreSQL 16 shared schema, EF migrations | `docker-compose.yml`, `King.Nexa.Platform/Migrations/` |
| Backend entry | Service registration and HTTP pipeline | `King.Nexa.Platform/Program.cs` |
| Frontend entry | Pinia, router, i18n and PrimeVue composition | `nexa-webapp/src/main.js` |
| HTTP entry | 40 Swagger controller tags | `*/Interfaces/Rest/*.cs`, live `/swagger/v1/swagger.json` |

### Detected bounded contexts and layers

| Context | Domain focus | Layer evidence |
|---|---|---|
| Catalog Management | brands, categories, catalog items, stock exposed for sale | `CatalogManagement/{Domain,Application,Infrastructure,Interfaces}` |
| IAM | user identity, password hashing, JWT session | `Iam/{Domain,Application,Infrastructure,Interfaces}` |
| Tenant Management | tenants, workspaces, memberships, subscriptions, features | `TenantManagement/{Domain,Application,Infrastructure,Interfaces}` |
| Sales | clients, purchase requests, orders, promotions, credit requests | `Sales/{Domain,Application,Infrastructure,Interfaces}` |
| Warehouse | inventory, lots, movements, reservations, warehouses | `Warehouse/{Domain,Application,Infrastructure,Interfaces}` |
| Logistics | dispatch, evidence, temperature, shipments | `Logistics/{Domain,Application,Infrastructure,Interfaces}` |
| Invoicing | documents, invoices, payments, notifications | `Invoicing/{Domain,Application,Infrastructure,Interfaces}` |
| Shared | persistence, security context, audit, reference data, middleware | `Shared/{Domain,Application,Infrastructure,Interfaces}` |

```text
Frontend Vue
  -> context API clients / BaseEndpoint / Axios interceptors
  -> Backend REST controllers and stable resource DTOs
  -> Application command/query services
  -> Domain aggregates, entities and value objects
  -> Domain repository ports / outbound service ports
  -> EF Core repository adapters and AppDbContext
  -> PostgreSQL shared schema
```

Runtime flow example:

```text
POST /api/v1/orders
  -> Workspace membership and CanCreateOrder policies
  -> CreateOrderResource assembler
  -> OrderCommandService.CreateAsync
  -> Order aggregate + tenant/client assignment
  -> IOrderRepository + IUnitOfWork
  -> inventory reservation + catalog stock
  -> business documents/payment process/notification
  -> dispatch handoff
  -> OrderResource response
```

Classification: primarily **Modular Monolith + Layered Architecture + selective tactical DDD + shared database**. It is not a microservice system. It is not only transaction scripts because domain objects contain invariants and behavior. It is not strict Clean Architecture because one central EF model spans all modules, some controllers access it directly, and cross-context workflows are synchronous. Some operational CRUD records remain closer to an Active Record-shaped data model even though persistence itself uses EF Data Mapper.

## 3. REST API Audit

The live OpenAPI document contains 183 paths, 306 operations, and 40 controller tags. All rows below refer to current live routes; aliases are shown explicitly.

| Controller | Endpoint | Method | Current assessment | REST issue | Recommended design | Severity |
|---|---|---|---|---|---|---|
| Authentication | `/api/v1/authentication/sign-in` | POST | Correct process endpoint | Authentication is naturally action-like | Keep and document public contract | Low |
| AuditLogs | `/api/v1/audit-logs` | GET | Correct read resource | No cursor/page contract | Add `page`, `pageSize`, date/action filters | Medium |
| Brands | `/api/v1/brands`, `/by-name/{name}` | CRUD/GET | Resource plus route filter | Filter in path; global mutation policy differs from tenant catalog | Prefer `GET /brands?name=` | Low |
| Categories | `/api/v1/categories`, `/by-name/{name}` | CRUD/GET | Resource plus route filter | Same inconsistency as brands | Query parameter, retain alias temporarily | Low |
| CatalogItems | `/api/v1/catalog-items` | CRUD | Correct resource | List is unpaginated | Add page/filter/sort envelope | High |
| CatalogItems | `/by-brand`, `/by-category`, `/cold-chain` | GET | Useful filters | Filter values embedded in path | `GET /catalog-items?brand=&category=&coldChain=` | Medium |
| CatalogItems | `/{id}/deactivations` | POST | Acceptable transition subresource | None material | Keep; represent transition result consistently | Low |
| Clients | `/api/v1/clients` and `/client-accounts` | CRUD | Canonical resource plus compatibility alias | Two public names for same concept | Choose `client-accounts`; deprecate alias with headers/docs | Medium |
| CreditRequests | `/api/v1/credit-requests/{id}/resolutions` | POST | Acceptable transition subresource | Resolution DTO/status semantics need documentation | Keep as created transition resource | Low |
| PurchaseRequests | `/api/v1/purchase-requests` | CRUD | Correct aggregate collection | Missing pagination/read model | Add filtered paged inbox endpoint | High |
| PurchaseRequests | `/{id}/submissions`, `adjustment-requests`, `commercial-validations`, `acceptances`, `rejections`, `cancellations` | POST | Acceptable transition subresources | Response/status conventions vary | Keep; standardize transition resource responses | Low |
| PurchaseRequestLines | `/api/v1/purchase-request-lines` | CRUD | Child resource exposed globally | Can bypass aggregate boundary conceptually | Prefer `/purchase-requests/{id}/lines`; protect writes through request service | Medium |
| ConversationMessages | `/api/v1/conversation-messages` | CRUD | Resource | Collection needs request/order filters and pagination | Query params or nested read endpoints | Medium |
| Orders | `/api/v1/orders` | CRUD | Correct aggregate resource | Unpaginated; PUT and DELETE semantics need tighter contract | Add paged filters; PATCH simple metadata; cancellation transition | High |
| Orders | `/{id}/confirm`, `/reject`, `/cancel` | POST | RPC-like verbs | Inconsistent with newer plural transition nouns | Add `/confirmations`, `/rejections`, `/cancellations`; deprecate old routes | Medium |
| Promotions | `/api/v1/promotions`, `/{id}/activations`, `deactivations` | CRUD/POST | Correct resource and transitions | Unpaginated | Add page/status/date/segment filters | Medium |
| InventoryItems | `/api/v1/inventory-items` | CRUD | Correct inventory resource | Unpaginated; path filters | Query parameters and paged response | High |
| InventoryItems | `/{id}/reserve`, `/{id}/release-reservation` | POST | Problematic RPC-like compatibility routes | Action names and resource identity are hidden | Use `POST /reservations`, `POST /reservations/{id}/releases` | High |
| InventoryLots | `/api/v1/inventory-lots` | CRUD | Correct resource | Unpaginated | Page by status/expiry/warehouse | Medium |
| InventoryMovements | `/inventory-movements` and `/stock-movements` | GET/POST | Resource plus compatibility alias | Duplicate vocabulary | Keep `inventory-movements`; deprecate alias | Medium |
| Reservations | `/api/v1/reservations`, `/{id}/releases` | POST/GET | Preferred resource model | Read collection is unpaginated | Keep; add order/request/status filters | Low |
| Warehouses | `/api/v1/warehouses`, `/by-location/{location}` | CRUD/GET | Correct resource plus route filter | Filter path and unpaginated list | `GET /warehouses?location=` | Low |
| DispatchOrders | `/api/v1/dispatch-orders` | CRUD | Correct aggregate resource | Includes `/by-tenant/{tenantId}` despite authenticated tenant scope | Remove public tenant selector; infer scope | High |
| DispatchOrders | `assignees`, `schedules`, `route-starts`, `deliveries`, `incidents`, `reschedules`, `status-changes` | POST | Mostly acceptable transition subresources | Generic `status-changes` can bypass explicit semantics if unrestricted | Keep explicit transitions; constrain generic status changes | Medium |
| DispatchEvents | `/api/v1/dispatch-events` | CRUD | Resource | No paging/order-specific read model | Add `/dispatch-orders/{id}/events` or filters | Medium |
| ProofOfDeliveryRecords | `/api/v1/proof-of-delivery-records` and nested dispatch route | CRUD/POST | Correct evidence resource | Two creation surfaces require one canonical contract | Prefer nested create, retain collection read | Low |
| TemperatureLogs | `/api/v1/temperature-logs` and nested dispatch route | CRUD/POST | Correct telemetry resource | Unpaginated potentially high-volume data | Time-window cursor pagination | High |
| Shipments | `/api/v1/shipments`, `/{id}/delivered` | CRUD/POST | Legacy aggregate beside dispatch | Overlapping Logistics language and action verb | Clarify bounded responsibility; add `/deliveries` transition if retained | Medium |
| BusinessDocuments | `/api/v1/business-documents` | CRUD | Correct resource | Unpaginated | Filter by order/client/status with page envelope | Medium |
| BusinessDocuments | `/generations`, `/upload`, `/{id}/status-changes`, `/mark-ready` | POST/PUT | Mixed transitions | Singular imperative `/mark-ready` and upload command are inconsistent | Model uploads/readiness as subresources; deprecate PUT alias | Medium |
| Invoices | `/api/v1/invoices` | CRUD | Valid billing aggregate but legacy status is ambiguous | `/paid` verb duplicates `/status-changes` | Keep canonical status transition; deprecate `/paid` | Medium |
| Payments | `/api/v1/payments` | CRUD | Correct resource | Unpaginated | Add client/order/status/date paging | Medium |
| Payments | `confirmations`, `rejections`, `cancellations` | POST | Good transition subresources | None material | Keep | Low |
| PaymentMethodRecords | `/api/v1/payment-method-records` | CRUD | Correct supporting resource | Verbose implementation name leaks into API | Consider `/payment-methods` canonical alias | Low |
| PaymentProcessRecords | `/api/v1/payment-process-records` | CRUD/transitions | Resource but implementation-centric name | May expose orchestration record as business concept | Document as payment checkout/process read model | Medium |
| NotificationRecords | `/notification-records` and `/notifications`; `PUT /read` and `POST /reads` | CRUD/transitions | Heavy compatibility surface | Duplicate nouns and two read semantics | Canonical `/notifications/{id}/reads`; deprecate aliases | Medium |
| ReferenceData | `/api/v1/reference/{type}` | GET | Read-only catalog facade | Controller accesses `AppDbContext` directly | Move queries behind application port; keep facade | Medium |
| Tenants | `/api/v1/tenants`, `/by-slug/{slug}` | GET/CRUD | Tenant resource and public lookup | Ensure public DTO excludes sensitive fields | Keep explicit public lookup response | Low |
| OrganizationRegistrations | `/api/v1/organization-registrations` | POST/GET | Correct onboarding resource | Public polling/security contract must be bounded | Keep opaque external id and limited response | Low |
| TenantMembers | `/api/v1/tenant-members` | CRUD | Admin resource | Duplicates workspace membership model | Define legacy/admin-only status, plan consolidation | High |
| TenantRules | `/api/v1/tenant-rules` | CRUD | Correct admin resource | Generic rule values need schema validation | Keep; validate by rule type | Low |
| TenantCustomFields | `/api/v1/tenant-custom-fields` | CRUD | Correct admin resource | Unpaginated | Page if cardinality grows | Low |
| TenantSubscriptions | `/api/v1/tenant-subscriptions` | CRUD | Correct admin resource | Exposes both current and compatibility routes | Define one canonical tenant-scoped endpoint | Medium |
| WorkspaceFeatures | `/api/v1/workspace-features` | CRUD | Correct admin resource | Entitlement enforcement is not universal | Enforce features at application boundary | Medium |
| WorkspacePreferences | `/api/v1/workspace-preferences` | CRUD | Correct resource | String `value/value_type` is weakly typed | Validate allowed types; consider JSON value | Low |
| Workspaces | `/api/v1/workspaces` | CRUD | Correct resource | List/admin policy must stay constrained | Keep scoped queries and admin mutation policy | Low |
| UserWorkspaceMemberships | `/api/v1/user-workspace-memberships` | CRUD | Correct relationship resource | Profile duplication and broad update DTO | Split membership role/status from user profile | Medium |
| Users | `/api/v1/users` | CRUD | IAM resource | User mutation and membership roles can diverge | Keep identity-only responsibility | Medium |

### REST conventions we should defend

1. Public business resources use plural nouns and `/api/v1` versioning.
2. `POST` creates resources or explicit domain-transition subresources.
3. `PATCH` is reserved for genuine partial changes; `PUT` means full replacement/idempotent update.
4. `DELETE` is used only when conceptual removal is valid; lifecycle cancellation should be a transition resource.
5. Collection filters belong in query parameters, not a growing family of `/by-*` paths.
6. Every unbounded collection should adopt a common `page`, `pageSize`, `sort`, and filter response envelope.
7. HTTP resources are stable DTOs; domain entities are never serialized directly.
8. Problem responses use `application/problem+json` and preserve safe business details.

### REST professor defense

Defend: versioning, plural resources, thin controllers, DTO/assembler isolation, authorization policies, and explicit transition resources such as purchase-request acceptances and payment confirmations. Acknowledge: several old action routes and aliases remain for compatibility, pagination is incomplete, and screen-specific read models are the next REST improvement. Do not claim full Richardson Level 3/HATEOAS.

## 4. DDD Audit

| Bounded Context | Element | File/Class | DDD role | Evidence | Assessment | Issue | Recommendation | Severity |
|---|---|---|---|---|---|---|---|---|
| Sales | Order | `Sales/Domain/Model/Aggregates/Order.cs` | Aggregate root | Creates items, total, status, delivery; validates transitions | Strong tactical model | Constructor depends on a command type in Domain | Use domain factory/parameters; move use-case command to Application | Medium |
| Sales | PurchaseRequest | `Sales/Domain/Model/Entities/SalesOperationalRecords.cs` | Effective aggregate root | Payment/date validation and transition matrix | Rich behavior despite `Entities` placement | Public setters weaken invariant boundary | Promote to aggregate, private setters, controlled line mutation | High |
| Sales | OrderCommandService | `Sales/Application/Internal/CommandServices/OrderCommandService.cs` | Application service | Coordinates client, catalog, inventory, docs, logistics | Correct orchestration role | Non-atomic cross-context fan-out | Transaction/outbox workflow and idempotent handlers | Critical |
| Catalog | CatalogItem | `CatalogManagement/Domain/Model/Aggregates/CatalogItem.cs` | Aggregate root | Money/stock/active behavior | Meaningful aggregate | Brand/category remain text | Normalize identity or document snapshot strategy | Medium |
| Warehouse | InventoryItem | `Warehouse/Domain/Model/Aggregates/InventoryItem.cs` | Aggregate root | Reservation/release quantity rules | Strong invariant owner | Separate catalog stock can drift | Define source of truth and integration event | High |
| Logistics | DispatchOrder | `Logistics/Domain/Model/Entities/LogisticsOperationalRecords.cs` | Effective aggregate root | Assign/schedule/start/complete/incident transitions | Good ubiquitous language | Generic `ChangeStatus` default accepts arbitrary status | Whitelist full state machine; promote aggregate | High |
| Logistics | Shipment | `Logistics/Domain/Model/Aggregates/Shipment.cs` | Aggregate root | Scheduling/delivery/temperature behavior | Valid legacy concept | Overlap with DispatchOrder is unclear | Document context map and migration ownership | Medium |
| Invoicing | Invoice/Payment | `Invoicing/Domain/Model/Aggregates/` | Aggregate roots | Money/status transitions | Real billing models | Legacy vs operational payment-process concepts overlap | Clarify canonical billing model | Medium |
| Invoicing | BusinessDocument | `Invoicing/Domain/Model/Entities/InvoicingOperationalRecords.cs` | Effective aggregate | Controlled status transitions | Useful domain behavior | Public setters allow invalid state | Factory/private setters and invariant tests | Medium |
| IAM | User | `Iam/Domain/Model/Aggregates/User.cs` | Aggregate root | Identity and password state | Correct IAM owner | Platform role and membership role can diverge | Treat membership as authorization source | Medium |
| Tenant Management | Tenant/Workspace | `TenantManagement/Domain/Model/` | Aggregate/entity | Tenant identity plus workspace records | Correct SaaS vocabulary | `TenantMember` overlaps IAM membership | Consolidate model or mark compatibility boundary | High |
| Shared | Repository/UoW | `Shared/Domain/Repositories`, context domain repositories | Persistence ports | 23 interfaces, 24 adapters | Correct dependency inversion | Generic repository is not tenant-safe | Restrict generic use for tenant entities | High |

Answers to required questions:

- Bounded contexts are structurally clear and use domain vocabulary, but the context map and ownership of duplicated concepts need documentation.
- Strong aggregates protect invariants (`Order`, `InventoryItem`, `Invoice`, `Shipment`). PurchaseRequest, DispatchOrder, and BusinessDocument contain behavior but public setters weaken aggregate encapsulation.
- Value objects are real where they validate and encapsulate (`Money`, `Quantity`, `OrderStatus`, identifiers); simple one-property wrappers add type safety but limited behavior.
- Commands are currently in `Domain/Model/Commands`. They are use-case messages and should normally move to Application; the domain should accept values/factories rather than application commands.
- Repository interfaces are correctly in Domain and EF implementations in Infrastructure.
- Current `*CommandService` and `*QueryService` classes are application services, not domain services.
- Domain has no EF Core or Infrastructure imports. This is a genuine strength.
- Controllers are mostly thin. `ReferenceDataController(AppDbContext)` is the primary layering exception; some admin controllers repeat exception-to-response logic.
- Frontend contains presentation/read composition and credit checks. Backend remains authoritative, but the browser performs too many cross-context joins.
- Cross-context coupling is represented through application outbound interfaces such as `IOrderFulfillmentHandoff` and `IOrderDocumentProvisioner`; this is better than direct EF access, but execution remains synchronous.
- No domain events or event dispatcher were found.

### Domain Events candidates

| Event | Origin aggregate | Trigger | Consumers | Benefit | Implementation priority |
|---|---|---|---|---|---|
| PurchaseRequestSubmitted | PurchaseRequest | buyer submits valid request | Sales inbox, audit, notification | Decouples intake side effects | High |
| PurchaseRequestAccepted | PurchaseRequest | commercial validation converts request | Order creation, audit | Idempotent conversion boundary | Critical |
| OrderCreated | Order | initial order commit | Warehouse, Invoicing, Logistics, notification | Fixes current synchronous uncertain-response defect | Critical |
| OrderConfirmed | Order | payment/inventory confirmation | Logistics, buyer timeline | Stable lifecycle propagation | High |
| InventoryReserved | InventoryItem/Reservation | stock reservation succeeds | Sales order, dispatch readiness | Single stock truth and retry safety | Critical |
| DispatchOrderCreated | DispatchOrder | fulfillment handoff completes | Logistics board, buyer timeline | Removes direct Sales-to-Logistics write chain | High |
| DispatchOrderDelivered | DispatchOrder | delivery and evidence complete | Sales, Invoicing, notification | Consistent terminal state | High |
| InvoiceIssued | Invoice | invoice generation | Documents, buyer portal, audit | Clear billing lifecycle | Medium |
| PaymentRegistered | Payment | payment creation/confirmation | Invoice, order, notifications | Removes polling/manual joins | High |

### DDD professor defense

Say: “Nexa applies tactical DDD inside a modular monolith. Context modules own vocabulary and use cases; aggregates and value objects protect selected invariants; domain repository interfaces are persistence ports; application services coordinate workflows. We do not claim perfect DDD: domain events and stricter aggregate encapsulation are the next hardening step, and commands should move from Domain to Application.”

## 5. Layering / Clean Architecture Audit

| File/Class | Current layer | Responsibility found | Correct responsibility | Violation? | Recommendation | Severity |
|---|---|---|---|---|---|---|
| `OrdersController` | Interfaces | authorization, resource mapping, delegation | HTTP adapter | No material violation | Keep thin; standardize ProblemDetails | Low |
| `ReferenceDataController` | Shared Interfaces | Direct EF queries and projections | HTTP adapter only | Yes | Add query service/repository adapter | Medium |
| `OrderCommandService` | Sales Application | Cross-context orchestration | Use-case coordinator | Correct layer, oversized workflow | Replace post-commit fan-out with events/outbox | Critical |
| `PurchaseRequestCommandService` | Sales Application | 447-line workflow and conversion | Use-case coordinator | Hotspot, not infrastructure leak | Split by submit/validate/accept use cases | High |
| `InventoryOperationsCommandService` | Warehouse Application | 287-line inventory workflows | Use-case coordinator | Hotspot | Separate reservation, lot and movement services | Medium |
| Domain repository interfaces | Domain | Persistence abstractions | Domain/Application ports | Correct | Retain | Low |
| EF repositories | Infrastructure | Scoped LINQ, persistence | Adapter | Correct | Add reusable tenant specifications/query helpers | Low |
| `AppDbContext` | Shared Infrastructure | Maps all modules | EF unit of work/data mapper | Pragmatic modular-monolith compromise | Keep one DB now; split mappings/configuration ownership by module | Medium |
| `BaseRepository<TEntity>` | Shared Infrastructure | Generic unscoped find/list | Generic persistence helper | Unsafe for tenant-owned data | Do not expose for tenant aggregates or require tenant specification | High |
| `data.store.js` | Frontend Application | Loads 23 collections, joins contexts, mutations, read models | Focused app state/cases | Yes, God Store | Extract context stores and backend read models | High |
| Vue views | Frontend Presentation | Mostly delegate through store/API layer | Presentation | Mostly correct | Remove remaining domain decisions from views | Medium |

Ideal dependency direction:

```text
Interfaces -> Application -> Domain
Infrastructure -> Application/Domain abstractions
Domain -> no Infrastructure dependency

Frontend presentation -> frontend application -> frontend infrastructure/API
```

Current backend follows this direction in most modules. Infrastructure is composed at `Program.cs`, and no Domain/Application file imports EF Core or `AppDbContext`. Exceptions are explicit, not hidden.

### Layering professor defense

Defend the dependency direction, repository ports, DTO boundary, and modular composition root. Acknowledge the shared `AppDbContext` as a deliberate modular-monolith tradeoff, not “pure Clean Architecture”; acknowledge `ReferenceDataController` and frontend God Store as refactoring targets.

## 6. Design Patterns Audit

| Pattern/Smell | File/Class | Evidence | Assessment | Why it matters | Recommendation | Severity |
|---|---|---|---|---|---|---|
| Repository | `*/Domain/Repositories`, `*/Infrastructure/.../Repositories` | 23 ports and 24 adapters | Well used | Domain is persistence-agnostic | Retain scoped contracts | Low |
| Unit of Work | `IUnitOfWork`, `UnitOfWork` | `CompleteAsync` commits EF changes | Correct PoEAA pattern | Explicit transaction boundary | Expand to atomic use-case transaction | Medium |
| Data Mapper | EF configurations/repositories | Entities mapped separately from tables | Correct | Avoids Active Record persistence methods | Retain | Low |
| DTO | `Interfaces/Rest/Resources` | 50 resource files | Well used | Stable external contract | Add validation attributes/validators | Low |
| Assembler/Mapper | `Interfaces/Rest/Transform` | 43 transform files | Well used | Prevents domain serialization | Retain and contract-test | Low |
| Application Service | `Application/Internal/*Services` | 48 service files | Correct, some oversized | Central use-case coordination | Split hotspots by use case | Medium |
| Dependency Injection | Context DI extensions + `Program.cs` | Eight module registrations | Well used | Composition stays at edge | Retain | Low |
| Middleware | Global exception + membership validation | Cross-cutting HTTP concerns | Correct | Centralized consistency/security | Return ProblemDetails consistently | Low |
| Interceptor | `AuditableEntityInterceptor` | Sets audit timestamps | Correct | Removes repeated persistence concern | Retain | Low |
| Adapter/ACL port | `IOrderFulfillmentHandoff`, `IOrderDocumentProvisioner` | Sales depends on interfaces | Good boundary | Limits direct context dependency | Make event handlers idempotent | Medium |
| Partial CQRS | Command/query services and query records | Separate mutation/read services | Useful, not full CQRS | Clarifies intent without extra stores | Do not oversell as full CQRS | Low |
| Value Object/Aggregate | Domain model | Money, Quantity, status, Order, InventoryItem | Genuine tactical DDD | Centralizes rules | Strengthen public-setter records | Medium |
| God Store | `nexa-webapp/src/app/application/stores/data.store.js` | 1,474 lines, 23 collection loaders | Confirmed smell | High coupling and hard testing | Extract stores/read APIs incrementally | High |
| God Service | `PurchaseRequestCommandService` | 447 lines | Confirmed hotspot | Many reasons to change | Split use cases, publish events | High |
| God Controller file | `LogisticsOperationalControllers.cs` | 455 lines, multiple controllers | Packaging smell more than logic smell | Navigation/review cost | One controller per file | Low |
| Primitive obsession | Operational status strings | String state in several records | Partial smell | Invalid states possible | Typed status VOs/enums with conversion | Medium |
| Data clumps | delivery/address fields | Repeated across request/order/resources | Confirmed | Mapping drift risk | Shared context-specific value object, not global DTO | Medium |
| Missing Specification | Tenant/status/date queries | Repeated LINQ predicates | Missing | Consistency and testability | Add specifications/query objects for complex filters | Medium |
| Missing Factory | Order/purchase conversion | Constructors plus orchestration | Missing for complex creation | Creation invariants span many values | Domain factory returning valid aggregate | Medium |
| Missing Domain Events | Entire solution | No event types/dispatcher | Major gap | Current synchronous coupling/partial commits | Add outbox-backed event flow | Critical |

### Patterns professor defense

Mention Repository, Unit of Work, DTO/Assembler, Application Service, Middleware, Interceptor, DI, Adapter ports, Value Objects, Aggregates, partial CQRS, and Modular Monolith. State that Specification, Domain Events, aggregate factories, and domain services for cross-aggregate rules are planned, not already implemented.

## 7. Database / EF Core Audit

The live schema has 49 business tables plus `__EFMigrationsHistory`, 26 applied migrations, 84 foreign keys, 13 alternate unique constraints, 42 tenant-aware composite foreign keys, 171 indexes, and **zero check constraints**.

| Table/Entity | PK | FKs | Indexes | Tenant scoped | Normalization status | Issue | Recommendation | Severity |
|---|---|---|---|---|---|---|---|---|
| tenants | id | none | unique slug/ruc | Root | 3NF | Plan/status text | Reference constraints or validated VOs | Low |
| workspaces | id | tenant | slug, tenant | Yes | 3NF | Workspace/tenant lifecycle consistency | FK plus lifecycle tests | Low |
| users | id | none | unique email/username | Global identity | 3NF | Global role duplicates membership authorization | Deprecate platform role for tenant decisions | Medium |
| user_workspace_memberships | id | tenant, workspace, user, client | workspace+user unique; tenant indexes | Yes | Intentional snapshot duplication | `full_name/email` duplicate users | Document snapshot or normalize profile ownership | High |
| tenant_members | id | tenant | tenant+email unique | Yes | Duplicates memberships | Competing member source | Consolidate/deprecate one model | High |
| client_accounts | id | tenant | tenant+code/ruc; composite key | Yes | Mostly 3NF | Address/profile fields are dense but atomic | Split addresses only when multiple addresses are required | Low |
| brands/categories | id | none | unique name | Global reference | 3NF | Global ownership differs from tenant catalog | Document global catalog governance | Low |
| catalog_items | id | tenant | tenant+catalog/product unique | Yes | Not strict 3NF | `brand_name` and `category_name` duplicate reference labels | Add nullable brand/category FKs; retain labels as documented snapshots only if needed | High |
| orders | id | tenant, client | tenant+number, tenant+client, alternate key | Yes | 3NF header | Missing tenant+status+created index; no amount check | Add index and nonnegative checks | High |
| order_items | id | tenant+order | order indexes | Yes | 3NF with price snapshot | No quantity/money checks | CHECK quantity > 0 and amounts >= 0 | High |
| purchase_requests | id | tenant+client | tenant+code/status/client | Yes | 3NF header | Missing status+created index | Add `(tenant_id,status,created_at)` | Medium |
| purchase_request_lines | id | tenant+request/catalog | request/catalog | Yes | 3NF | Decimal quantity/weight lack checks | Add positive checks | High |
| inventory_items | id | tenant+catalog | tenant+catalog/product; alternate key | Yes | 3NF | Duplicates catalog availability as separate stock concept | Define authoritative stock and reconciliation | High |
| inventory_lots | id | tenant+inventory/warehouse | tenant+lot/expiry | Yes | 3NF | Quantity/date/temp checks absent | Add quantity/date/temp constraints | High |
| inventory_movements | id | tenant-aware operational FKs | tenant+code/date | Yes | 3NF ledger | Signed quantity semantics not DB-enforced | Check nonzero quantity; document movement sign | Medium |
| inventory_reservation_records | id | tenant-aware inventory/order/request | tenant+code/status | Yes | 3NF | No units check | CHECK units > 0 | High |
| dispatch_orders | id | tenant+order/client | tenant+code/status/order | Yes | 3NF | Missing status+created index | Add composite query index | Medium |
| dispatch_events | id | tenant+dispatch | dispatch/tenant | Yes | 3NF event log | Status vocabulary is free text | Status validation | Medium |
| proof_of_delivery_records | id | tenant+dispatch | unique dispatch | Yes | 3NF | Boolean evidence references do not identify artifact | Store artifact metadata/reference | Medium |
| temperature_logs | id | tenant-aware dispatch/order | tenant+status, dispatch/order | Yes | 3NF time series | No physical temperature range/check; needs time index | Add range policy and `(tenant_id,recorded_at)` | Medium |
| invoices | id | tenant+order | tenant+number/status | Yes | 3NF | Legacy overlap with documents/process records | Clarify canonical billing ownership | Medium |
| payments | id | tenant-aware invoice/order/client/method | tenant+reference/status/date | Yes | 3NF | Amount check absent | CHECK amount > 0 | High |
| payment_process_records | id | tenant-aware payment/order/client/method | tenant+status and relation indexes | Yes | Read/process record | Derived subtotal/IGV/total can drift | Define snapshot semantics and consistency checks | Medium |
| business_documents | id | tenant-aware order/client + type | tenant+order/client/status | Yes | 3NF | File name is not durable storage identity | Add object key/checksum/provider metadata | Medium |
| promotions + promotion_catalog_items | id | tenant-aware promotion/catalog | unique pivot | Yes | Correct many-to-many | Discount semantics are free-form strings | Typed adjustment fields and constraints | Medium |
| audit_logs | id | tenant/workspace/user/membership | tenant+action/date, resource | Yes | Append-oriented | `actor_user_id` non-null may block system actors | Define system actor strategy | Low |
| reference tables | id | hierarchical location FKs | unique keys/codes | Global | 3NF | No soft-delete uniformity beyond `is_active` | Document lifecycle semantics | Low |
| tenant admin tables | id | tenant/workspace | tenant-specific unique indexes | Yes | Mostly 3NF | Generic string configuration weakly typed | Validate schema per setting | Medium |

1NF is generally satisfied: values are scalar except intentionally structured `jsonb` payload/audit metadata. 2NF is satisfied because surrogate primary keys are used and composite alternate keys enforce tenant identity. 3NF is mostly satisfied in transactional tables, with explicit exceptions in catalog labels, membership profile snapshots, and derived payment totals. BCNF is not claimed.

### Known database risks to verify

- Confirmed: `catalog_items.brand_name/category_name` coexist with `brands/categories` without FKs.
- Confirmed: `user_workspace_memberships.full_name/email` duplicate `users` profile data.
- Confirmed: both `tenant_members` and `user_workspace_memberships` model people in a tenant.
- Confirmed: zero PostgreSQL CHECK constraints for quantities, money, temperature, or date ordering.
- Confirmed: core screens often query tenant + status + recency, but not all have `(tenant_id,status,created_at)` indexes.
- Confirmed: `IsActive`/status lifecycle is inconsistent across aggregate types; this is not a universal soft-delete strategy.
- Confirmed: one central `AppDbContext` owns all module sets and mapping application.
- Confirmed: migration history contains several broad normalization/hardening migrations; destructive migration work should not happen before review.

### Database professor defense

Defend PostgreSQL + EF Core Data Mapper/UoW, transactional header/line separation, explicit pivot/event tables, audit fields, demo seed, tenant/workspace relation, and 42 composite tenant-aware FKs. Acknowledge catalog/member duplication, absent checks, selected missing query indexes, and global query filters as hardening work.

## 8. Multi-tenancy Audit

| Area/File | Current mechanism | Risk | Evidence | Recommendation | Severity |
|---|---|---|---|---|---|
| JWT codec/options | Signed claims include tenant/workspace/membership/client | Secret/config drift | `JwtTokenCodec`, `JwtOptions` | Require secret in Render and rotate safely | High |
| `WorkspaceMembershipValidationMiddleware` | Validates claims against active membership and optional headers | Missing claims pass to later authorization by design | Explicit DB predicate over user+tenant+workspace+membership | Keep order before authorization; add ProblemDetails | Low |
| `HttpCurrentWorkspaceContext` | Reads scoped claims | Nullable values can be forgotten | `ICurrentWorkspaceContext` properties | Fail fast in tenant use cases | Medium |
| Authorization policies | Workspace and capability roles | Role strings duplicated across records | `NexaAuthorizationPolicies` | Central role vocabulary and policy tests | Medium |
| Context repositories | Explicit tenant predicates | Human omission risk | 111 tenant-related repository predicate references | Add shared tenant specification/global filters | High |
| `BaseRepository<TEntity>` | Unscoped `FindAsync/ListAsync` | Cross-tenant read if used for tenant entity | `BaseRepository.cs` | Ban/restrict generic repository for `ITenantScoped` | Critical |
| EF model | Composite tenant FKs and alternate keys | Parent rows can still be fetched without filter | 42 live composite FKs, 13 alternate keys | Add query filters plus admin bypass service | High |
| Reference data | Global tables intentionally not tenant scoped | Accidental mutation/governance | brands/categories/location/reference tables | Explicit global marker and platform-only writes | Medium |
| Admin endpoints | Policies for workspace management | Global tenant selector can be abused if policy changes | `/dispatch-orders/by-tenant/{tenantId}` | Remove tenant id from tenant-user API | High |
| Frontend headers | Sends JWT + tenant/workspace hints | localStorage is user-controlled | `nexa-webapp/src/shared/infrastructure/http.js` | Continue treating headers as hints only | Low |

Isolation is enforced by **both code and database relationships**, but row filtering is code-led. Composite FKs prevent a child from linking to another tenant's parent; they do not automatically filter SELECTs. The highest failure point is an omitted repository predicate or use of generic `FindAsync`.

Tenant-owned aggregates/records should implement an `ITenantScoped` marker. EF global query filters are recommended for normal request scopes, with explicit, audited bypass for platform administration, startup seed, and migrations. Global reference catalogs should remain exceptions.

Professor defense: “The JWT identifies user, tenant, workspace and membership; middleware revalidates that membership in PostgreSQL; authorization policies guard capabilities; repositories filter tenant rows; and composite FKs prevent cross-tenant relationships. We recognize that global query filters would add another defense layer.”

## 9. Frontend Architecture Audit

| Frontend module/file | Responsibility | Issue | Backend dependency | Recommendation | Severity |
|---|---|---|---|---|---|
| `src/router/index.js` + context routes | History router and lazy role portals | 401/403 interceptor redirects use hash URLs despite history router | IAM | Use router navigation or history paths consistently | Medium |
| `src/iam/application/iam.store.js` | Session/tenant/membership state | Sensitive token in localStorage; XSS exposure | Authentication | Prefer hardened cookie if deployment supports it; keep CSP | Medium |
| `src/shared/infrastructure/http.js` | Axios clients and auth/tenant headers | Two base URLs increase configuration drift | All API | Validate env at startup and standardize clients | Medium |
| `BaseEndpoint` | HTTP adapter | Assumes array/object contracts only | All resources | Add paged-envelope support and typed error adapter | Medium |
| Context API clients | Resource-specific calls | Mostly correct real backend usage | Matching `/api/v1` endpoints | Retain | Low |
| `data.store.js` | Cross-context global data | 1,474-line God Store; 23 parallel collections; joins and mutations | Nearly all modules | Extract Sales/Warehouse/Logistics/Invoicing stores | High |
| `readCoreCollection` | Collection loader | Converts every API error to `[]`, making outage look like empty business data | All lists | Preserve per-resource error state; do not silently erase truth | High |
| Manual order flow | Creates order, reloads all data | Late/uncertain backend response previously reported false failure after persistence | Orders and downstream contexts | Reconcile by order code; backend outbox/transaction | Critical |
| Buyer portal | Requests/orders/docs/payment methods | Real data, but composed through frontend joins | Sales/Invoicing | Add buyer dashboard/order-detail read models | High |
| Sales screens | Inbox/order/client views | Browser filters and joins over full collections | Sales | Paged inbox/order summary endpoints | High |
| Logistics screens | Dispatch/events/evidence/temperature | Multiple collections joined by ids/codes | Logistics | Dispatch detail read model | High |
| Owner/company administration | Tenant admin resources | Large view/store surface | Tenant Management | Keep module store; split sections/components further | Medium |
| Route filters | URL/localStorage query state | Some filters persist, not standardized | List endpoints | URL query as canonical persistent filter | Low |
| Mocks/fallbacks | Runtime data source | `initial-data.json` removed; no json-server package found | Real API | Keep explicit empty/error states; prohibit silent demo fallback | Low |

Recent visual routes remain wired to real APIs: catalog, clients, purchase requests, orders, dispatch, documents, payments, reference data, promotions, and tenant administration use `BaseEndpoint`/Axios. No active local business dataset was found. This is defendable.

### Frontend professor defense

Defend context-based modules, Pinia/application state, API adapters/assemblers, lazy routes, role-based guards, reusable components, i18n, and real API integration. Acknowledge the global data store and browser read joins as debt; explain that backend paged/read-model endpoints will replace full-collection composition.

## 10. Backend/Frontend Integration Audit

| Flow | Frontend route | Backend endpoint | Tables affected | Works? | Issue | Recommendation |
|---|---|---|---|---|---|---|
| Buyer creates request | `/portal/request-builder` | `POST /purchase-requests`, lines, submission | purchase_requests, purchase_request_lines, messages/audit | Yes | Multi-call composition can partially fail | One atomic submission command/idempotency key |
| Sales validates/accepts | `/ops/commercial/purchase-requests/:id` | `POST .../commercial-validations`, `/acceptances` | purchase_requests, orders, order_items | Yes | Large synchronous conversion service | Event/outbox after one transaction |
| Sales creates manual order | `/ops/commercial/manual-order-entry` | `POST /orders` | orders/items, reservations, catalog, docs, payment process, notifications, dispatch | Yes after reconciliation fix | Previously returned error after order persisted; non-atomic fan-out | Atomic core commit + async idempotent side effects |
| Sales reviews order | `/ops/commercial/purchase-orders/:id` | `GET /orders/{id}` plus joined resources | orders and related tables | Yes | Multiple full collections loaded | Order detail read model |
| Logistics dispatches | `/ops/operations/dispatch-orders/:id` | dispatch transitions/events/evidence/temperature | dispatch_orders/events/POD/temperature | Yes | Generic status endpoint can weaken state machine | Prefer explicit transition endpoints |
| Buyer tracks | `/portal/purchase-orders/:id` | orders, dispatch, events, documents, payments | cross-context reads | Yes | Frontend joins ids/codes | Buyer order lifecycle read model |
| Payments/documents | buyer order + ops business-documents routes | `/business-documents`, `/payments`, transitions | business_documents, payments, process records | Yes | Legacy invoice/payment vocabulary overlaps | Canonical billing contract and paged reads |
| Owner administers | `/ops/operations/company-administration` | tenants/workspaces/memberships/subscriptions/features/preferences | tenant admin tables | Yes | TenantMember vs membership duplication | Consolidate source of truth |

DTO naming is generally aligned by frontend assemblers. Tenant/workspace are carried by JWT and headers; backend claims remain authoritative. Date-only delivery fields use ISO `yyyy-MM-dd`; timestamps are UTC. Role access is enforced in both route guards and backend policies, with backend as security boundary.

## 11. Deploy Readiness Audit

| Area | Current state | Risk | Recommendation | Severity |
|---|---|---|---|---|
| Dockerfile | .NET 10 multi-stage publish/runtime | No non-root user; restore may vary without locked packages | Pin dependencies, add non-root runtime user | Medium |
| Render | Docker service + PostgreSQL + `/health` | Blueprint lacks JWT and data-protection declarations | Add secret/env declarations and persistent key strategy | Critical |
| Port | `ASPNETCORE_URLS=http://+:8080` | Requested `PORT=10000` convention not used directly | Document Render internal port or bind from `PORT` consistently | Low |
| Database URL | Render connection mapped to custom env alias | Nonstandard mapping adds drift | Support `ConnectionStrings__DefaultConnection` directly | Medium |
| JWT | Reads `NEXA_JWT_SECRET/ISSUER/AUDIENCE` | Variables absent from `render.yaml`; production may fail validation | Declare generated secret and fixed issuer/audience | Critical |
| CORS | configured origin + localhost defaults | Production permits localhost origins unnecessarily | Environment-specific exact allowlist | Medium |
| Swagger | Enabled only in Development | Good default | Add explicit config switch only if operationally required | Low |
| Migrations | Always `context.Database.Migrate()` on startup | Multiple replicas/risky deploy can race or block startup | `APPLY_MIGRATIONS_ON_STARTUP=false`; deployment job | Critical |
| Seed | Called in Development; options exist | Production safe, but toggle semantics are unclear | Gate by both environment and `SEED_DEMO_DATA` | Medium |
| Health | Basic `/health` only | Does not prove DB readiness | Add liveness and PostgreSQL readiness endpoints | High |
| Error handling | Central ProblemDetails; production hides exception | Resource marker mismatch previously exposed localization keys | Keep corrected resource namespace; add correlation id | Medium |
| Logs | Structured ASP.NET/EF logs | No request correlation/observability sink | Request logging, trace id, deployment log policy | Medium |
| Data protection | Optional filesystem persistence | Render filesystem is ephemeral by default | Persistent disk/external key store or remove unnecessary DP dependency | High |
| Package compatibility | net10 app with EF/Npgsql 9 | Major-version support mismatch | Upgrade EF Core and provider together to 10 after compatibility test | High |

Expected production configuration contract:

```text
ASPNETCORE_ENVIRONMENT=Production
PORT=10000                         # or document ASPNETCORE_URLS internal port
DATABASE_URL=<render-postgres-url>
ConnectionStrings__DefaultConnection=<normalized connection string>
NEXA_JWT_SECRET=<secret >= 32 chars>
NEXA_JWT_ISSUER=NexaPlatform
NEXA_JWT_AUDIENCE=NexaPlatformUsers
CORS_ALLOWED_ORIGINS=<frontend-url>
ENABLE_SWAGGER=false
APPLY_MIGRATIONS_ON_STARTUP=false
SEED_DEMO_DATA=false
```

Current code does not implement all these exact names; the roadmap treats that mismatch as required deploy hardening.

## 12. Professor Q&A Preparation

| Professor question | Strong answer | Evidence in project | Risk if challenged |
|---|---|---|---|
| Why a modular monolith instead of microservices? | Current team and scale need one deploy/transactional database, while modules preserve future extraction boundaries without distributed-system cost. | Eight context modules and DI composition in `Program.cs` | Do not claim modules are independently deployable. |
| Where are the bounded contexts? | Catalog, IAM, Tenant, Sales, Warehouse, Logistics and Invoicing own distinct models/use cases; Shared contains cross-cutting infrastructure. | Top-level context folders | Shared and duplicated concepts need a context map. |
| Which aggregates exist? | Order, ClientAccount, CatalogItem, InventoryItem, Warehouse, Shipment, Invoice, Payment, User and Tenant are explicit; PurchaseRequest/DispatchOrder behave as aggregate roots but need stronger encapsulation. | `Domain/Model/Aggregates`, operational record classes | Avoid calling every EF entity an aggregate. |
| Which patterns were applied? | Repository, UoW, Data Mapper, DTO/Assembler, Application Service, DI, Middleware, Interceptor, Adapter ports, Value Objects and aggregates. | Concrete classes documented above | Domain events/specifications are not implemented. |
| How is multi-tenancy protected? | JWT claims, active membership validation, policies, tenant predicates, and composite tenant FKs provide layered controls. | Middleware, context, repositories, 42 composite FKs | No global query filters. |
| Why repository if EF is already repository/UoW? | Context-specific ports protect domain/application from EF and centralize tenant-aware query semantics; generic wrappers alone would add little value. | Domain ports and EF adapters | `BaseRepository` currently demonstrates the generic-repository danger. |
| How is REST handled? | Versioned plural resources, stable DTOs, assemblers and transition subresources; compatibility aliases are catalogued for deprecation. | Live Swagger and controllers | Pagination and some action routes remain debt. |
| How do you avoid exposing domain entities? | Controllers receive/return `Resources`; assembler classes translate commands and response models. | 50 resources, 43 transforms | Some operational records have structurally similar DTOs but are still mapped. |
| What technical debt do you recognize? | Domain events/atomic workflow, God Store, pagination/read models, global tenant filters, DB checks, deploy migration policy. | Audit findings | Never say the architecture is complete. |
| What is the next improvement? | Make order acceptance/creation one atomic core transaction and publish outbox events to idempotent Warehouse/Invoicing/Logistics handlers. | Observed Sales false-failure incident | Requires staged design, not a last-minute rewrite. |
| How do you validate persistence? | EF migrations, PostgreSQL metadata/constraint queries, integration tests, Docker runtime, and authenticated endpoint smoke tests. | 26 migrations, 40 tests, live DB queries | Test coverage is not exhaustive. |
| How is it deployed? | Multi-stage Docker image, Render web service and managed PostgreSQL, exact CORS/secret config, health checks, and controlled migrations. | `Dockerfile`, `render.yaml` | Current blueprint needs secret/readiness/migration hardening. |

Final defense position: **Defendible with observations**. The strongest honest statement is that Nexa has a credible modular, tenant-aware architecture and working end-to-end business flow, while its next maturity step is reliability at cross-context boundaries, not adding more folders.

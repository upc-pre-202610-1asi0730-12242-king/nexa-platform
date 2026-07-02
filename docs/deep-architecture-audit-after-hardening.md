# Nexa Platform — Deep Architecture Audit After Hardening

## 1. Executive Verdict

Final verdict: **Risky but demoable**.

The hardening is real in backend security, tenant middleware, repository guardrails, composite foreign keys, route compatibility, and safe Stripe preparation. It is not only surface-level. But the system is not ready to call fully clean: the running Docker webapp cannot log in through `http://localhost:5173` because Compose override sets `VITE_NEXA_API_BASE_URL=/api/v1`; browser calls `http://localhost:5173/api/v1/authentication/sign-in` and receives `404`, while the same credentials work directly against `http://localhost:5068/api/v1/authentication/sign-in`. Frontend God Store debt remains. Outbox/domain-event consistency is absent. Global EF query filters are absent. Several REST compatibility aliases remain.

Overall score: **6.8/10**

| Area | Score /10 | Improved? | Evidence | Remaining risk | Priority |
|---|---:|---|---|---|---|
| Backend overall | 7.4 | Yes | `dotnet test` passes 47/47; 325 Swagger operations; route controllers use resources/assemblers/services. | Multi-step workflows still commit before downstream provisioning; no outbox. | P1 |
| Frontend overall | 5.8 | Partial | Bounded stores exist; Stripe foundation added; `npm run build` passes. | Docker/browser login broken by API base URL override; `data.store.js` still 1482 lines. | P0 |
| REST API | 7.0 | Yes | Canonical transition resources exist beside legacy aliases: orders `/confirmations`, reservations `/releases`, invoices `/status-changes`. | Many `/by-*` routes and legacy imperative aliases remain. | P2 |
| DDD tactical | 6.7 | Partial | `Order` and `InventoryItem` enforce state/inventory invariants. | Application services still orchestrate large cross-context transaction scripts; commands still live under Domain. | P1 |
| Security | 8.1 | Yes | `DefaultPolicy` and `FallbackPolicy` require authenticated users; runtime smoke: no token `401`, wrong tenant `403`, buyer sales endpoint `403`. | Swagger document security metadata still unclear; webhook is public by design and must stay signature-verified. | P1 |
| Tenant/workspace isolation | 8.0 | Yes | Middleware validates JWT claims, membership, tenant header, workspace header; DB has 85 FKs with many composite tenant-aware FKs. | No global query filters; direct `AppDbContext` queries remain possible. | P1 |
| Database | 7.4 | Yes | Latest migration `20260701151138_AddSafeQueryIndexes`; 50 tables; 27 migrations; 85 FKs; 13 unique constraints. | Explicit business check constraints and outbox table absent. | P2 |
| Deploy readiness | 6.3 | Partial | Render docs/env vars updated; startup migrations gated; `/health/live` public, `/health` protected. | Local Compose override breaks direct webapp API path; DataProtection warning about unencrypted keys remains. | P0 |
| Integration | 5.6 | Partial | Direct API login works with seeded ICISA users; protected API smoke passes. | Browser login through running Docker webapp fails with `404` to frontend origin. | P0 |

## 2. Change Verification Against Previous Audit

| Previous issue | Current status | Evidence | Fixed correctly? | Remaining work |
|---|---|---|---|---|
| God Store / `data.store.js` | Partially fixed | Bounded stores exist, but `nexa-webapp/src/app/application/stores/data.store.js` is still 1482 lines and centralizes many contexts. | No | Keep strangling it into context stores/read models. |
| `loadCoreCollections` | Partially fixed | `loadCoreCollections()` still loads 23 collections in one `Promise.all` at `data.store.js:1212-1261`. | No | Replace page-level bulk boot with route-specific read models. |
| Browser-side joins | Partially fixed | `data.store.js:1289-1345` joins warehouses, orders, dispatches, documents, payments, messages, notifications client-side. | No | Move operational dashboards to backend read models. |
| Optional pagination | Improved | Orders controller accepts `page`, `pageSize`, filters, sort; similar pattern exists in current APIs. | Partial | Many collection endpoints still expose unpaged `GET`. |
| Read-model endpoints | Improved | Sales, buyer, catalog, workspace read-model controllers/services exist. | Yes, partial | Frontend still depends heavily on raw collection joins. |
| REST transition endpoints | Improved | Orders have `/confirmations`, `/rejections`, `/cancellations`; invoices have `/status-changes`; reservations have `/releases`. | Yes | Retire legacy aliases later with deprecation docs. |
| Legacy RPC-like endpoints | Partially fixed | Legacy `/confirm`, `/reject`, `/cancel`, `/reserve`, `/release-reservation`, `/paid`, `/mark-ready` remain as aliases. | Partial | Keep only compatibility until professor/demo freeze passes. |
| Endpoint security | Improved | Auth fallback policy enabled in `SharedServiceCollectionExtensions.cs:46-53`. | Yes | Swagger security display still needs verification/documentation. |
| Tenant isolation | Improved | `WorkspaceMembershipValidationMiddleware.cs:28-75` validates headers against JWT and active membership. | Yes | Add global query filters or stronger repository enforcement for all direct query paths. |
| `BaseRepository` tenant safety | Improved | `BaseRepository.cs:15-23` throws for generic `FindByIdAsync`/`ListAsync` on `ITenantScoped`. | Yes | Some repositories still use direct `AppDbContext`; audit must stay continuous. |
| `ITenantScoped` or equivalent | Improved | Tenant-scoped repository guard checks `ITenantScoped`. | Yes | Ensure every tenant data entity implements it. |
| Global Query Filters or alternative tenant hardening | Alternative only | No `HasQueryFilter`; repository scoping and middleware are the hardening mechanism. | Partial | Add EF global filters or scoped query facade for defense in depth. |
| DDD commands in Domain | Untouched | Controllers import `Sales.Domain.Model.Commands`, e.g. `OrdersController.cs:3`. | No | Move application commands to Application layer if strict Clean Architecture is required. |
| Multi-class domain files | Partially fixed | Some grouped operational files remain, e.g. `TenantAdministrationControllers.cs` has multiple controllers in one file. | Partial | Split large/grouped files where it lowers risk. |
| Large controller files | Partial | `OrdersController` is thin; `DispatchOrdersController`, `InvoicesController`, `PaymentsController` remain broad. | Partial | Keep controllers thin and split compatibility aliases later. |
| Aggregate invariants | Improved | `Order` and `InventoryItem` enforce state/stock rules. | Yes | `PurchaseRequest` is still more entity + app-service workflow than aggregate root. |
| Domain events / outbox foundation | Untouched | No `outbox_messages` table; no outbox migration found. | No | Add outbox before real payment/webhook/order fanout reliability. |
| Catalog normalization | Improved | Catalog tables and tenant composite FKs exist; catalog item data is normalized enough for demo. | Partial | Some names/category/brand strings still appear in resource/read paths. |
| Indexes | Improved | DB migration table shows latest `20260701151138_AddSafeQueryIndexes`. | Yes | Keep measuring query plans for dashboard endpoints. |
| Check constraints | Mostly untouched | `rg HasCheckConstraint` found none; `pg_constraint contype='c'` in public returned 0 explicit business checks. | No | Add business CHECK constraints for statuses/positive quantities where safe. |
| Render readiness | Improved | `Program.cs:177-187` gates migrations; `Program.cs:207-211` gates Swagger; Render docs updated. | Partial | Env/proxy semantics must be verified in deployed runtime. |
| Health endpoints | Improved | `/health/live` anonymous, `/health` and `/health/ready` protected by fallback. | Yes | Render health check must use public `/health/live`. |
| Migration/seed flags | Improved | Migrations gated by env/flag; seed only Development with `SeedData:Enabled`. | Yes | Production migration strategy still should be externalized. |
| Vue Stripe readiness | Improved | Frontend uses `VITE_STRIPE_PUBLISHABLE_KEY`; no card data; no fake success. | Yes, foundation only | Real Stripe SDK/Checkout redirect requires backend implementation. |
| Payment backend support | Foundation only | `StripePaymentPreparationService` returns `501/503`; webhook verifies signature but does not process events. | Partial | Implement Stripe SDK server-side and idempotent webhook handlers. |
| Frontend error handling | Improved but incomplete | Axios interceptors clear auth and redirect on `401/403`; payment store shows backend errors. | Partial | Login runtime failure still maps to generic incorrect credentials instead of API base URL/config issue. |

## 3. Backend REST Audit

Swagger runtime inventory produced **325 operations** from `http://localhost:5068/swagger/v1/swagger.json`. Swagger operation `.security` arrays were not reliable proof of auth, so auth evidence comes from code attributes, fallback policy, and runtime smoke.

| Controller | Method | Route | Auth? | Policy/Role? | REST classification | Issue | Severity |
|---|---|---|---|---|---|---|---|
| `AuthenticationController` | POST | `/api/v1/authentication/sign-in` | Public | `[AllowAnonymous]` | Public auth endpoint | Correct public endpoint. | Low |
| `AuthenticationController` | POST | `/api/v1/authentication/sign-up` | Protected by fallback | Fallback authenticated | Protected endpoint | Sign-up is not public onboarding; acceptable if intentional. | Low |
| `TenantsController` | GET | `/api/v1/tenants/by-slug/{slug}` | Public | `[AllowAnonymous]` | Public onboarding/workspace lookup | Public tenant preview works; limited surface. | Low |
| `OrganizationRegistrationsController` | POST | `/api/v1/organization-registrations` | Public | `[AllowAnonymous]` | Public onboarding | Correct if registration is public. | Low |
| Health minimal endpoint | GET | `/health/live` | Public | `.AllowAnonymous()` | Public health/live | Correct for Render/liveness. | Low |
| Health checks | GET | `/health` | Protected | Fallback auth | Protected health | Runtime `401` without token; okay if Render uses `/health/live`. | Low |
| Health ready | GET | `/health/ready` | Protected | Fallback auth | Protected readiness | Runtime `401` without token; okay. | Low |
| `OrdersController` | GET | `/api/v1/orders` | Protected | WorkspaceMember | Correct collection | Optional pagination; unpaged fallback remains. | Medium |
| `OrdersController` | GET | `/api/v1/orders/{id}` | Protected | WorkspaceMember | Correct resource | Tenant scoped through service/repository. | Low |
| `OrdersController` | GET | `/api/v1/orders/{id}/timeline` | Protected | WorkspaceMember | Acceptable subresource | Read model endpoint; good. | Low |
| `OrdersController` | GET | `/api/v1/orders/by-order-number/{orderNumber}` | Protected | WorkspaceMember | Compatibility/query route | Prefer query params `/orders?orderNumber=`. | Low |
| `OrdersController` | GET | `/api/v1/orders/by-customer/{customerId}` | Protected | WorkspaceMember | Compatibility/query route | Prefer `/orders?customerId=`; route leaks query semantics. | Low |
| `OrdersController` | GET | `/api/v1/orders/by-status/{status}` | Protected | WorkspaceMember | Compatibility/query route | Prefer `/orders?status=`. | Low |
| `OrdersController` | POST | `/api/v1/orders` | Protected | `CanCreateOrder` | Correct collection create | Service fanout after commit can cause partial success. | High |
| `OrdersController` | PUT | `/api/v1/orders/{id}` | Protected | `CanCreateOrder` | Correct resource replace/update | Pending-only invariant in domain. | Low |
| `OrdersController` | POST | `/api/v1/orders/{id}/confirm` | Protected | `CanCreateOrder` | Legacy RPC-like alias | Kept intentionally; route exists. Runtime JSON missing id returned `404`. | Low |
| `OrdersController` | POST | `/api/v1/orders/{id}/confirmations` | Protected | `CanCreateOrder` | Correct transition subresource | Canonical route exists; same method as legacy alias. | Low |
| `OrdersController` | POST | `/api/v1/orders/{id}/reject` | Protected | `CanCreateOrder` | Legacy RPC-like alias | Kept intentionally; runtime bad body returned `400`. | Low |
| `OrdersController` | POST | `/api/v1/orders/{id}/rejections` | Protected | `CanCreateOrder` | Correct transition subresource | Canonical route exists. | Low |
| `OrdersController` | POST | `/api/v1/orders/{id}/cancel` | Protected | `CanCreateOrder` | Legacy RPC-like alias | Kept intentionally. | Low |
| `OrdersController` | POST | `/api/v1/orders/{id}/cancellations` | Protected | `CanCreateOrder` | Correct transition subresource | Canonical route exists. | Low |
| `OrdersController` | DELETE | `/api/v1/orders/{id}` | Protected | `CanCreateOrder` | Correct-ish cancellation/delete | Domain cancellation, not physical delete. | Low |
| `InventoryItemsController` | POST | `/api/v1/inventory-items/{id}/reserve` | Protected | `CanManageInventory` | Legacy RPC-like alias | Kept as compatibility over reservation service. | Low |
| `InventoryItemsController` | POST | `/api/v1/inventory-items/{id}/release-reservation` | Protected | `CanManageInventory` | Legacy RPC-like alias | Kept as compatibility. | Low |
| `ReservationsController` | POST | `/api/v1/reservations` | Protected | `CanManageInventory` | Correct transition/resource | Good canonical create. Runtime bad body `400`. | Low |
| `ReservationsController` | POST | `/api/v1/reservations/{id}/releases` | Protected | `CanManageInventory` | Correct transition subresource | Good canonical release. | Low |
| `InvoicesController` | POST | `/api/v1/invoices/{id}/paid` | Protected | `CanManageDocuments` | Legacy RPC-like alias | Kept but `/status-changes` exists. Runtime missing id `404`. | Low |
| `InvoicesController` | POST | `/api/v1/invoices/{id}/status-changes` | Protected | `CanManageDocuments` | Correct transition subresource | Supports Paid/Cancelled only. | Low |
| `InvoicesController` | POST | `/api/v1/invoices/{id}/voidings` | Protected | `CanManageDocuments` | Correct transition subresource | Good replacement for delete/cancel intent. | Low |
| `BusinessDocumentsController` | POST | `/api/v1/business-documents/{id}/status-changes` | Protected | `CanManageDocuments` | Correct transition subresource | Good canonical route. | Low |
| `BusinessDocumentsController` | PUT | `/api/v1/business-documents/{id}/mark-ready` | Protected | `CanManageDocuments` | Legacy RPC-like route | Keep only as compatibility. | Low |
| `BusinessDocumentsController` | PUT | `/api/v1/business-documents/{id}/authorize-buyer` | Protected | `CanManageDocuments` | Legacy RPC-like route | Prefer `/status-changes` or buyer-visibility subresource. | Low |
| `BusinessDocumentsController` | PUT | `/api/v1/business-documents/{id}/mark-missing` | Protected | `CanManageDocuments` | Legacy RPC-like route | Prefer `/status-changes`. | Low |
| `NotificationRecordsController` | GET/POST/PUT | `/api/v1/notification-records` | Protected | WorkspaceMember / `CanManageDocuments` | Correct resource | Also duplicate `/api/v1/notifications` exists. | Medium |
| Notifications alias | GET/POST/PUT | `/api/v1/notifications` | Protected | Same service | Compatibility/duplicate route | Duplicate route increases client ambiguity. | Medium |
| `ClientsController` | GET/POST/PUT | `/api/v1/clients` | Protected | WorkspaceMember / `CanAcceptPurchaseRequest` | Compatibility name | Domain is `client_accounts`; route kept for frontend compatibility. | Medium |
| Client accounts route | GET/POST/PUT | `/api/v1/client-accounts` | Protected | Similar sales policy | Correcter domain resource | Needs migration of clients route later. | Low |
| `DispatchOrdersController` | GET | `/api/v1/dispatch-orders/by-tenant/{tenantId}` | Protected | WorkspaceMember | Unsafe-looking route | Route accepts tenant id; must ignore/validate against auth tenant. Needs targeted test. | High |
| `SalesReadModelsController` | GET | sales read model routes | Protected | `CanAcceptPurchaseRequest` | Read model | Runtime buyer access returned `403`; good. | Low |
| `StripePaymentsController` | POST | `/api/v1/payments/stripe/checkout-sessions` | Protected | WorkspaceMember | Prepared Stripe endpoint | Returns `503` without secret, `501` when configured; no fake payment. | Low |
| `StripePaymentsController` | POST | `/api/v1/payments/stripe/payment-intents` | Protected | WorkspaceMember | Prepared Stripe endpoint | Same safe foundation. | Low |
| `StripePaymentsController` | POST | `/api/v1/payments/stripe/webhook` | Public | `[AllowAnonymous]` + signature verification | Public webhook | Correct public route, but event processing pending. | Medium |

REST conclusion: **improved but mixed**. Canonical transition resources exist, controllers mostly remain thin, DTO/resources/assemblers are used, and no inspected controller returned EF entities directly. Compatibility aliases and `/by-*` query routes remain as deliberate migration debt.

## 4. Endpoint Security Audit

Default rule is now implemented: `SharedServiceCollectionExtensions.cs:46-53` sets both `DefaultPolicy` and `FallbackPolicy` to authenticated users under Nexa JWT scheme.

Public endpoints found and judged intentional:

- `POST /api/v1/authentication/sign-in`: authentication.
- `GET /api/v1/tenants/by-slug/{slug}`: public workspace/onboarding lookup.
- `POST /api/v1/organization-registrations`: public onboarding.
- `GET /health/live`: public liveness.
- `POST /api/v1/payments/stripe/webhook`: public Stripe webhook, requires signature.
- Swagger: exposed only in Development or `ENABLE_SWAGGER=true`.

| Endpoint | Public/Protected | Actual auth evidence | Tenant validation | Role policy | Risk | Fix |
|---|---|---|---|---|---|---|
| `/api/v1/authentication/sign-in` | Public | `[AllowAnonymous]` in `AuthenticationController.cs:29`; runtime `200`. | Workspace slug used during sign-in. | None, correct. | Low | Keep public and rate-limit later. |
| `/api/v1/tenants/by-slug/icisa` | Public | `[AllowAnonymous]`; runtime `200`. | Public preview only. | None. | Low | Ensure response remains non-sensitive. |
| `/api/v1/organization-registrations` | Public | `[AllowAnonymous]` on create. | Creates request, not tenant data exposure. | None. | Low | Keep anti-abuse controls in roadmap. |
| `/health/live` | Public | `.AllowAnonymous()` in `Program.cs:222`; runtime `200`. | None needed. | None. | Low | Use this for Render. |
| `/health` | Protected | Fallback auth; runtime no token `401`. | N/A. | Auth only. | Low | Document Render should not use this. |
| `/health/ready` | Protected | Fallback auth; runtime no token `401`. | N/A. | Auth only. | Low | Good for internal readiness only. |
| `/api/v1/orders` | Protected | Controller `[Authorize(WorkspaceMember)]`; runtime no token `401`, valid sales `200`. | Middleware validates active membership and headers. | Writes require `CanCreateOrder`. | Low | Keep tests. |
| `/api/v1/orders` wrong tenant | Protected | Runtime wrong tenant header `403`. | Header checked against JWT at middleware lines 28-34. | WorkspaceMember. | Low | Good. |
| Sales read model route | Protected | Controller uses `CanAcceptPurchaseRequest`; runtime buyer `403`. | Middleware validates membership. | Sales roles. | Low | Good. |
| Owner workspace write route | Protected | Tenant admin routes use `CanManageWorkspace`; runtime logistics `403`. | Middleware validates membership. | Owner/admin. | Low | Good. |
| Inventory write routes | Protected | `[Authorize(CanManageInventory)]` on mutating actions. | Repository/service scoped by workspace. | Logistics roles. | Low | Good. |
| Document/payment mutating routes | Protected | `[Authorize(CanManageDocuments)]` on document/payment writes. | Tenant-scoped repository. | Document roles. | Low | Good. |
| Stripe checkout/payment-intents | Protected | Controller `[Authorize(WorkspaceMember)]`; runtime no token `401`, auth without secret `503`. | Middleware before authorization. | Workspace member only. | Medium | Later restrict buyer/client ownership for payment records. |
| Stripe webhook | Public | `[AllowAnonymous]`; checks `STRIPE_WEBHOOK_SECRET`; verifies `Stripe-Signature`. Runtime no secret `503`. | Event processing pending. | None. | Medium | Implement idempotent processing with signature + event type validation. |
| Swagger | Development/flagged | `Program.cs:207-211`. | N/A. | N/A. | Low | Keep disabled in production unless explicitly enabled. |

Security smoke results:

| Scenario | Endpoint tested | Result |
|---|---|---|
| Public sign-in | `POST /api/v1/authentication/sign-in` | `200` |
| Public tenant preview | `GET /api/v1/tenants/by-slug/icisa` | `200` |
| Public live health | `GET /health/live` | `200` |
| Protected health no token | `GET /health` | `401` |
| Ready health no token | `GET /health/ready` | `401` |
| No token protected endpoint | `GET /api/v1/orders` | `401` |
| Invalid token protected endpoint | `GET /api/v1/orders` | `401` |
| Wrong tenant header | `GET /api/v1/orders` with valid token + wrong `X-Nexa-Tenant-Id` | `403` |
| Wrong workspace header | `GET /api/v1/orders` with valid token + wrong `X-Nexa-Workspace` | `403` |
| Correct authenticated Sales user | `GET /api/v1/orders` | `200` |
| Buyer accessing sales/admin endpoint | Sales read model route | `403` |
| Logistics accessing owner-only endpoint | Tenant admin write route | `403` |
| Stripe checkout no token | `POST /api/v1/payments/stripe/checkout-sessions` | `401` |
| Stripe checkout auth, no secret | `POST /api/v1/payments/stripe/checkout-sessions` | `503` |
| Stripe webhook, no secret | `POST /api/v1/payments/stripe/webhook` | `503` |

Security conclusion: **backend endpoint auth hardening is defensible**. The biggest remaining security risks are not missing auth by default; they are direct data access paths, role granularity for buyer payment ownership, and continued need for smoke tests around tenant-id route parameters.

## 5. Multi-Tenancy Deep Audit

Tenant isolation is enforced by multiple layers:

- JWT claims: `tenant_id`, `workspace_id`, `membership_id`.
- Middleware: active membership and header consistency check.
- Authorization policies: role requirements backed by workspace membership.
- Repository scoping: tenant/workspace-aware query methods.
- Base repository guard: blocks generic tenant-scoped reads.
- Database: composite tenant-aware FKs on many cross-context references.

Headers are **not trusted alone**. `X-Nexa-Tenant-Id` and `X-Nexa-Workspace` are validated against JWT/membership in middleware.

| Area/File | Mechanism | Evidence | Failure mode | Severity | Recommended fix |
|---|---|---|---|---|---|
| `WorkspaceMembershipValidationMiddleware` | Claims + DB membership validation | Lines 47-54 query active `UserWorkspaceMembership` by user/tenant/workspace/membership. | If endpoint bypasses middleware or claims missing, fallback still auth but tenant context may be absent. | Medium | Reject missing tenant/workspace claims on all business APIs, except explicit public. |
| Tenant header validation | Header checked against JWT tenant | Lines 28-34 return `403` on mismatch; runtime wrong tenant `403`. | Good. | Low | Keep smoke test. |
| Workspace header validation | Header checked against DB workspace slug | Lines 63-75 return `403` on mismatch; runtime wrong workspace `403`. | Good. | Low | Keep smoke test. |
| `BaseRepository` | Blocks generic reads on `ITenantScoped` | `BaseRepository.cs:15-23`. | Developers can still inject `AppDbContext` directly outside BaseRepository. | Medium | Ban direct tenant queries outside repositories/read-model services. |
| EF model | No global query filters | `rg HasQueryFilter` found none. | A new direct `DbSet.ToListAsync()` can leak tenant data if not scoped. | High | Add global filters or scoped query factory. |
| DB schema | Tenant-aware composite FKs | Runtime DB shows 85 FKs, many composite `(tenant_id, foreign_id)` relationships. | Prevents cross-tenant relational corruption, not every read leak. | Medium | Keep DB FK hardening plus app query filters. |
| `DispatchOrdersController` | Route accepts `tenantId` | `/api/v1/dispatch-orders/by-tenant/{tenantId}` exists. | If service trusts route tenant id, cross-tenant exposure possible. | High | Route should validate tenant id equals authenticated tenant or be replaced by query from current context. |
| `ReferenceDataController` | Direct `AppDbContext` in controller | `Shared/Interfaces/Rest/ReferenceDataController.cs` injects `AppDbContext`; protected by WorkspaceMember. | Controller-level EF access can bypass repository conventions. | Medium | Move to query service/read repository. |
| `Program.cs` readiness | Direct `AppDbContext` | `/health/ready` checks DB connectivity. | Not tenant data; protected. | Low | Fine. |
| Stripe payment routes | WorkspaceMember only | Protected by controller policy. | Buyer could potentially request prep for another payment id unless service later checks ownership. | Medium | When real Stripe implemented, validate payment/client ownership before creating session/intent. |
| Buyer data | Client account claim/use | Runtime Buyer cannot access Sales read model; data store filters by client id in UI. | Must verify every buyer endpoint uses client account scoping server-side. | Medium | Add buyer ownership smoke tests per buyer endpoint. |
| Global catalogs | WorkspaceMember protected | Catalog controllers use WorkspaceMember; writes sales/shared-reference policies. | Global/reference data visible to all members; likely intended. | Low | Document reference-data visibility. |

Search-specific findings:

- Direct `AppDbContext` injection remains in infrastructure repositories/services, seed, middleware, audit logging, read models, and `ReferenceDataController`.
- `IgnoreQueryFilters` not found.
- `HasQueryFilter` not found.
- `FindAsync(id)` exists in `BaseRepository`, but generic tenant-scoped read throws; specific repositories use tenant-aware methods.
- Many `.ToListAsync()` calls are safe only if using `Scoped()` helpers or explicit tenant predicates. This is improved but still convention-based.

Tenant conclusion: **stronger than before, not mathematically airtight**. Middleware + repository guard + composite FKs are real. Lack of global filters means accidental cross-tenant read remains possible for future direct `AppDbContext` code.

## 6. DDD Tactical Audit

DDD maturity classification: **mixed tactical DDD**.

There are real aggregates and invariants in some contexts. There is also transaction-script orchestration in Application services and cross-context fanout without outbox.

| Context | Class | DDD role | Evidence | Problem | Severity | Recommendation |
|---|---|---|---|---|---|---|
| Catalog | `CatalogItem` | Entity/Aggregate-like | Catalog controllers/services/resources exist; tenant-scoped repositories. | Some catalog normalization remains pragmatic, with category/brand name duplication in models/resources. | Medium | Continue normalizing with stable category/brand IDs where safe. |
| Catalog | `Category`, `Brand` | Reference entities | Protected by shared reference-data roles. | Reference data may be tenant/global ambiguous. | Low | Document visibility and ownership. |
| Sales | `Order` | Aggregate root | Enforces items, tenant assignment, pending-only update, confirm/reject/cancel state rules. | Create service commits then provisions downstream records. | High | Move fanout to domain events/outbox. |
| Sales | `PurchaseRequest` | Entity/workflow record | Command service accepts/rejects/assigns status and creates order/dispatch/docs. | Not rich aggregate; app service owns many state transitions. | Medium | Make status transition methods/invariants explicit on entity/aggregate. |
| Sales | `ClientAccount` | Tenant-scoped aggregate/entity | Composite FK relationships protect client account scope. | Buyer ownership checks must stay server-side. | Medium | Add buyer ownership tests. |
| Warehouse | `InventoryItem` | Aggregate root | `Reserve`/`Release` enforce stock/reserved quantities. | Reservation workflow split across item and reservation records. | Medium | Keep one canonical reservation aggregate/service boundary. |
| Warehouse | `InventoryReservationRecord` | Operational record | Canonical `/reservations` endpoint exists. | Legacy item action aliases remain. | Low | Deprecate aliases after frontend migration. |
| Logistics | `DispatchOrder` | Workflow aggregate | Dispatch transition routes and policies exist. | Large command/query orchestration; cross-context Sales return behavior. | Medium | Introduce domain events for dispatch state changes. |
| Invoicing | `Invoice` | Aggregate/workflow record | `status-changes`, `voidings`, paid/cancel commands. | Payment/Invoice state transitions partly app-service/controller parsed. | Medium | Centralize invoice/payment state machine in domain. |
| Invoicing | `Payment` | Payment record | Payment states exist; Stripe foundation does not fake paid. | Real provider event handling pending. | High | Add Stripe event idempotency + outbox/payment status transitions. |
| Tenant Management | `Tenant`, `Workspace`, `UserWorkspaceMembership` | Tenant/access aggregates | Middleware validates active membership. | `tenant_members` and `user_workspace_memberships` overlap conceptually. | Medium | Clarify admin/member model and avoid duplicate authority sources. |
| IAM | `User`, session lookup | Identity model | JWT includes workspace claims; sign-in direct API works. | Login UI broken by frontend API config, not IAM. | High | Fix runtime env/proxy. |
| Shared | `ITenantScoped`, value/context services | Cross-cutting DDD support | Base repository guard uses `ITenantScoped`. | Shared DbContext spans all contexts. | Medium | Keep context modules but consider per-context mapping boundaries. |

Specific invariant status:

- `PurchaseRequest` invariants: **partial**. App service validates and transitions; domain object is not yet rich enough.
- `Order` transition rules: **improved**. Domain methods enforce invalid transitions.
- `InventoryItem` reservation/release rules: **improved**. Domain methods enforce stock/reserved quantities.
- `DispatchOrder` state machine: **partial**. Workflow exists; richer explicit state machine still pending.
- `Invoice` and `Payment` status transitions: **partial**. Transition endpoints exist; real provider-backed payment transitions pending.
- Commands moved out of Domain: **no**. Domain command folders remain.
- Multi-class files split: **partial**. Some grouped files remain.
- Domain events/outbox foundation: **no**. No outbox table/foundation found.

DDD conclusion: **not folder-only, not rich domain everywhere**. Best description: mixed tactical DDD with rich islands (`Order`, `InventoryItem`) and transaction-script workflows (`PurchaseRequestCommandService`, order fanout, payment prep).

## 7. Layering / Clean Architecture Audit

Expected direction is mostly present:

- Interfaces/REST -> Application -> Domain.
- Infrastructure implements repositories/services.
- Resources/assemblers shield raw entities in controllers.
- Middleware handles auth/tenant cross-cutting behavior.

Violations and weak spots remain.

| File/Class | Current layer | Responsibility found | Violation? | Evidence | Severity | Fix |
|---|---|---|---|---|---|---|
| `OrdersController` | Interfaces/REST | HTTP mapping, resource assembly, command call. | No | `OrdersController.cs:119-181` delegates to command service and assembler. | Low | Keep thin. |
| `InvoicesController` | Interfaces/REST | Parses `PaymentStatus` in controller for status changes. | Minor | `InvoicesController.cs:164-181`. | Low | Move transition parsing into resource assembler/application command. |
| `BusinessDocumentsController` | Interfaces/REST | Simple validation strings and command dispatch. | Minor | Lines 55-67 validate type/label. | Low | Acceptable, but keep business rules in domain/service. |
| `ReferenceDataController` | Interfaces/REST | Direct EF queries. | Yes | `ReferenceDataController` injects `AppDbContext` and calls `.ToListAsync()`. | Medium | Move to Application query service/repository. |
| `WorkspaceMembershipValidationMiddleware` | Infrastructure/Security | Direct EF validation of active membership. | No | Cross-cutting auth middleware can query DB. | Low | Fine. |
| `BaseRepository` | Infrastructure | Generic persistence with tenant guard. | No | Good guardrail for `ITenantScoped`. | Low | Keep. |
| `PurchaseRequestCommandService` | Application | Creates orders, reserves inventory, provisions docs/handoff/messages. | Design smell | Multiple cross-context responsibilities and commits. | High | Use domain events/outbox/process manager. |
| `OrderCommandService` | Application | Persists order then reserves inventory/docs/logistics. | Design smell | Post-commit fanout caused frontend reconciliation need. | High | Make unit-of-work boundary atomic or event-driven. |
| Domain command classes | Domain | Application input commands under Domain namespace. | Yes, strict CA | Controllers import `Sales.Domain.Model.Commands`. | Medium | Move commands to Application contracts. |
| `data.store.js` | Frontend Application | Loads all contexts, joins data, contains fallback/reconciliation. | Yes | 1482 lines; `loadCoreCollections()` 23 resources; browser joins. | High | Split per bounded context and backend read models. |
| `buyer-payment-methods-view.vue` | Frontend Presentation | Normalizes payment statuses in view. | Minor | `paymentState()` in view. | Low | Could move to domain/status utility; okay short term. |
| `http.js` | Frontend Infrastructure | Token/header interceptors and redirects. | No | Correct layer for Axios interceptors. | Low | Fix env/proxy config. |

Layering conclusion: backend layering improved and is defendable, but cross-context application services and frontend central store remain the main clean-architecture risks.

## 8. Design Patterns and Code Smells Audit

| Pattern/Smell | File/Class | Evidence | Status | Severity | Fix |
|---|---|---|---|---|---|
| Repository | Many Infrastructure repositories | `OrderRepository`, `InventoryItemRepository`, `PaymentRepository`, etc. | Present | Low | Keep scoped repository methods. |
| Unit of Work | `UnitOfWork` | App services call `CompleteAsync`. | Present | Medium | Avoid multiple commits in one business use case. |
| DTO/Resource | `Interfaces/Rest/Resources/*` | Controllers use resources and assemblers. | Present | Low | Good. |
| Assembler/Mapper | `Interfaces/Rest/Transform/*` | Controllers map resources/entities. | Present | Low | Good. |
| Application Service | `*CommandService`, `*QueryService` | Business use cases mediated outside controllers. | Present | Low | Keep thin controllers. |
| Domain Service | Limited | Most logic is aggregate or app service. | Partial | Medium | Introduce only where domain logic spans aggregates. |
| Middleware | `WorkspaceMembershipValidationMiddleware`, exception middleware | Auth/tenant/error cross-cutting. | Present | Low | Good. |
| Interceptor | `AuditableEntityInterceptor`, Axios interceptors | Audit fields and frontend auth headers. | Present | Low | Good. |
| Adapter | `BaseEndpoint`, `BaseApi` | Frontend API adapter. | Present | Medium | Runtime env breaks adapter target. |
| Facade | `data.store.js` | Compatibility facade over many stores/endpoints. | Present as debt | High | Continue strangler migration. |
| Strategy | Not significant | No meaningful strategy pattern found for payments/state. | Missing where useful | Medium | Payment provider strategy can be added when real Stripe arrives. |
| Specification | Not found | Query predicates inline in repositories. | Missing | Low | Optional; do not add unless complexity grows. |
| Factory | Limited | Assemblers create commands/resources. | Partial | Low | Fine. |
| Domain Events | Not found | No outbox/domain event foundation. | Missing | High | Add events/outbox for order/payment/dispatch fanout. |
| Outbox | Not found | No `outbox_messages` table. | Missing | High | Add before real webhook/fanout reliability. |
| CQRS/read models | `WorkspaceReadModelQueryService`, read model controllers | Read models exist. | Partial | Medium | Expand to remove frontend joins. |
| Value Object | `PaymentConfirmation`, `InventoryReservation`, `RejectionReason` | Used by order transitions. | Present | Low | Good. |
| Aggregate | `Order`, `InventoryItem` | Richer methods/invariants. | Present in islands | Medium | Expand to PurchaseRequest/Invoice/Dispatch. |
| God Store | `data.store.js` | 1482 lines, 23 collection load, cross-context state. | Still present | High | Strangle by route/context. |
| God Controller | `TenantAdministrationControllers.cs` | Multiple controllers/classes in one file with inline methods. | Present | Medium | Split for maintainability. |
| God Service | `PurchaseRequestCommandService`, `OrderCommandService` | Cross-context orchestration and multiple responsibilities. | Present | High | Process manager/outbox. |
| Primitive Obsession | Status strings, route/body IDs | Some statuses parsed from strings. | Present | Medium | Use enums/value objects at boundaries. |
| Transaction Script | App service workflows | Accept request -> create order -> reserve -> docs -> handoff. | Present | High | Domain events/process manager. |
| Anemic Domain Model | `PurchaseRequest`, operational records | State handled outside entity. | Partial | Medium | Add domain methods/invariants. |
| Shotgun Surgery | Frontend payment/order flow | Store, APIs, views, backend routes all touched for flow changes. | Present | Medium | Read model/use-case APIs reduce fanout. |
| Duplicate Logic | Legacy + canonical routes | Same controller methods map old/new routes. | Managed duplicate | Low | Remove after compatibility window. |
| Leaky Abstractions | Frontend knows backend shape | `data.store.js` normalizes many raw payloads. | Present | High | Assemblers/read models per context. |

## 9. Database and EF Core Deep Audit

Runtime DB was available in `nexa-postgres` and inspected after checking schema columns through `information_schema.columns`.

Key schema evidence:

- Tables: **50** public tables.
- Migrations applied: **27**.
- Latest migration: `20260701151138_AddSafeQueryIndexes`.
- Seeded rows after fresh runtime: `users=4`, `tenants=1`, `workspaces=1`, `client_accounts=4`, `catalog_items=50`, `inventory_items=50`, `orders=1`, `dispatch_orders=1`, `invoices=1`, `payments=1`.
- Foreign keys: **85**, including many tenant-aware composite relationships.
- Unique constraints: **13**.
- Explicit business check constraints: none found by `rg HasCheckConstraint` and `pg_constraint contype='c'` for public namespace returned 0. `information_schema.table_constraints` reported many CHECK rows, but direct Postgres constraint evidence did not confirm explicit business checks; treat this as not implemented.
- Outbox table: absent; `information_schema.columns` for `outbox_messages` returned 0 rows.

Column checks performed before table claims:

| Table | Column evidence | Audit note |
|---|---|---|
| `orders` | `tenant_id`, `client_account_id`, `status`, `total_amount`, delivery fields, confirmation/rejection fields. | Tenant/client scope now modeled. |
| `order_items` | `tenant_id`, `order_id`, `product_id`, `catalog_item_id`, quantity/price/subtotal. | Tenant-aware FK protects order relation. |
| `purchase_requests` | `tenant_id`, `client_account_id`, code/origin/status/priority/payment/delivery fields. | Good B2B request scope. |
| `inventory_items` | tenant and inventory quantities. | Domain invariants protect reserve/release; DB CHECK still missing. |
| `inventory_lots` | tenant and warehouse/item references. | Tenant composite FKs present. |
| `inventory_reservation_records` | tenant, inventory/order/request refs. | Canonical reservation resource backed by table. |
| `dispatch_orders` | tenant/order/client refs and status fields. | Composite FKs reduce cross-tenant corruption. |
| `payments` | tenant/invoice/order/client/payment method refs, amount/status. | Ready for Stripe foundation, not real provider lifecycle. |
| `invoices` | tenant/order/client refs, payment status. | Status transition endpoints exist. |
| `tenant_members` and `user_workspace_memberships` | member/role/workspace overlap. | Needs conceptual authority cleanup. |

EF Core configuration findings:

| Area | Evidence | Status | Risk | Recommendation |
|---|---|---|---|---|
| Shared DbContext | `AppDbContext.cs` contains DbSets for all contexts and applies configurations. | Pragmatic monolith. | Medium | Accept for course; enforce context boundaries in code. |
| Mappings | Context-specific configuration extension calls exist. | Good. | Low | Keep per-context config files. |
| Global filters | No `HasQueryFilter`. | Missing. | High | Add tenant global filters or scoped query provider. |
| Composite FKs | Runtime FK query shows many `(tenant_id, id)` relationships. | Good. | Medium | Keep expanding where missing. |
| Indexes | Latest safe indexes migration applied. | Improved. | Low | Verify query plans for dashboards. |
| Check constraints | No explicit business CHECK constraints found. | Missing. | Medium | Add positive quantity/status constraints when safe. |
| Seed | Development-only and idempotent enough for demo. | Improved. | Medium | Keep production seed disabled. |
| Audit fields | `AuditableEntityInterceptor` exists. | Good. | Low | Keep. |
| Soft delete | Not confirmed as universal. | Partial/unknown. | Low | Document entities that need soft delete. |
| Outbox | Missing. | Missing. | High | Add for order/payment/dispatch fanout. |

Database conclusion: **real hardening happened**, especially tenant composite FKs and indexes. The remaining gap is defense-in-depth: global query filters, explicit business CHECK constraints, and outbox/idempotency.

## 10. Frontend Architecture Audit

Frontend build passes, and bounded context folders/stores exist. Architecture improved, but central compatibility store still dominates important flows.

Evidence:

- `npm run build` passed with Vite.
- Stores exist under bounded contexts: catalog, IAM, invoicing, logistics, sales, tenant-management, warehouse.
- `data.store.js` remains a 1482-line compatibility facade.
- `loadCoreCollections()` still fetches 23 collections.
- Browser-side joins still build dispatch/order/document/payment/message projections.
- Axios infrastructure centralizes API base URL, tokens, tenant/workspace headers, and `401/403` redirects.

Main frontend issue is runtime config:

- Base URL code: `src/shared/infrastructure/http.js:3` uses `VITE_NEXA_API_BASE_URL` or defaults to `http://localhost:5068/api/v1`.
- Main Compose sets `VITE_NEXA_API_BASE_URL=http://localhost:5068/api/v1`.
- `docker-compose.override.yml:6-8` overrides webapp to `VITE_NEXA_API_BASE_URL=/api/v1`.
- Running Docker webapp calls `http://localhost:5173/api/v1/authentication/sign-in` and gets `404`.
- Direct API sign-in to `http://localhost:5068/api/v1/authentication/sign-in` succeeds with ICISA credentials.

Conclusion: **frontend architecture improved but runtime integration is currently broken in Docker/browser**.

## 11. Backend/Frontend Integration Audit

Direct backend integration:

- `POST /api/v1/authentication/sign-in` with `elena.litano@icisa.pe`, workspace `icisa`, password `Password123!` returns `200`.
- Returned claims include tenant/workspace/client account context.
- Protected backend smoke works with bearer token and tenant/workspace headers.

Browser integration:

- Opened `http://localhost:5173/auth/login`.
- Filled workspace `icisa`, email `elena.litano@icisa.pe`, password `Password123!`.
- UI stayed on login and showed: “Workspace, email or password is incorrect. Check the information and try again.”
- Browser console showed `404` for `http://localhost:5173/api/v1/authentication/sign-in`.

Integration verdict: **API works; webapp runtime path does not**. This is not bad seed data and not invalid ICISA credentials. It is local Compose/proxy/base URL behavior.

## 12. Payment Flow and Vue Stripe Readiness

Payment foundation is safe and intentionally incomplete.

Backend evidence:

- `Program.cs:75-85` maps `STRIPE_SECRET_KEY` and `STRIPE_WEBHOOK_SECRET` from environment variables.
- `StripePaymentsController` exposes:
  - `POST /api/v1/payments/stripe/checkout-sessions`
  - `POST /api/v1/payments/stripe/payment-intents`
  - `POST /api/v1/payments/stripe/webhook`
- Checkout and PaymentIntent endpoints are protected by `WorkspaceMember`.
- Webhook is `[AllowAnonymous]` and verifies signature before accepting.
- `StripePaymentPreparationService.cs:18-31` returns not-created/not-ready messages and does **not** mark payments paid.
- `StripePaymentPreparationService.cs:41-73` verifies Stripe signature using timestamp tolerance and HMAC SHA-256 fixed-time compare.

Frontend evidence:

- `stripe-payments.store.js:6` reads only `VITE_STRIPE_PUBLISHABLE_KEY`.
- It redirects only when backend returns `checkoutUrl`.
- It sets an error if no `clientSecret` or checkout URL is returned.
- `stripe-payment-foundation.vue:54-56` explicitly says Nexa does not mark payment paid until verified backend Stripe/webhook updates the record.
- Buyer payment screen lists states: pending, processing, paid, failed, cancelled.

Stripe verdict: **safe foundation, not full Stripe integration**. This meets the “do not fake payments” requirement. It does not yet support real Stripe Checkout/PaymentIntent creation.

## 13. Render Deployment Readiness

Improved:

- Startup migrations are gated by Development/Testing/`APPLY_MIGRATIONS_ON_STARTUP=true`.
- Seed is Development-only with `SeedData:Enabled`.
- Swagger is Development-only or `ENABLE_SWAGGER=true`.
- `/health/live` is public and suitable for Render liveness.
- Stripe env vars documented/configured in backend render docs.
- DataProtection key path is configurable and Docker ownership was fixed.

Risks:

- Local `docker-compose.override.yml` changes API base URL to `/api/v1`, which only works if proxy route is used correctly. Direct browser on `5173` fails.
- `nexa-proxy` orphan service exists in local compose state. Not a code defect, but audit/runtime instructions should be clear: use proxy URL if override expects proxy routing, or remove override for direct Vite port.
- DataProtection logs still warn that keys may be persisted unencrypted in Development. Not fatal for local demo; production should use a protected key store.

Deploy verdict: **backend deploy posture improved; frontend/proxy local runtime must be fixed or documented before demo**.

## 14. E2E Business Flows

Verified:

| Flow | Evidence | Result | Risk |
|---|---|---|---|
| API sign-in | Direct POST to API with ICISA Buyer credentials | `200` | Low |
| Protected backend access | Orders with no token, invalid token, wrong tenant, correct token | `401`, `401`, `403`, `200` | Low |
| Role enforcement | Buyer to Sales read model; Logistics to owner route | `403`, `403` | Low |
| Stripe no fake payment | Protected route no token `401`; auth no secret `503`; webhook no secret `503` | Safe failure | Low |
| Webapp login | Browser form on `localhost:5173` | Fails; `404` to `5173/api/v1/authentication/sign-in` | High |
| Order transition aliases | Old/new confirm/reject routes exist and return expected validation/not-found classes for bad/missing ids | Route compatibility works | Low |
| Inventory reservation aliases | Old/new reservation routes exist | Route compatibility works | Low |

Not fully verified in browser due login/config failure:

- Buyer catalog -> request -> sales validation -> order -> dispatch -> documents.
- Buyer payment screen after real login.
- Owner/admin full company administration flow.

E2E verdict: **backend smoke is good; browser E2E is currently blocked by webapp API base URL/runtime config**.

## 15. Build/Test/Runtime Health

Commands executed:

| Command | Result |
|---|---|
| `dotnet test` in `nexa-platform` | Passed: 47/47 tests. |
| `npm run build` in `nexa-webapp` | Passed: Vite built 425 modules. |
| Swagger route extraction from API | 325 operations. |
| Runtime security smoke with API | Expected `200/401/403/503` outcomes documented above. |
| Browser login smoke with Playwright | Failed because webapp called `/api/v1` on port `5173`, not API `5068`. |
| DB schema inspection through `information_schema.columns` | Completed for core tables before table-specific claims. |

Runtime state:

- `nexa-platform-api` responds on `localhost:5068`.
- `nexa-webapp` responds on `localhost:5173`.
- `nexa-postgres` responds on `localhost:5432`.
- `nexa-proxy` exists locally on `localhost:8000` from override/proxy setup.

Build/test verdict: **compilation and tests are healthy; direct browser demo route is not**.

## 16. Final Risk Register and Priority Roadmap

| Priority | Risk | Evidence | Required action |
|---|---|---|---|
| P0 | Docker/browser login broken | Browser calls `localhost:5173/api/v1/authentication/sign-in` and gets `404`; API direct login works. | Fix Compose override/proxy usage or Vite API base URL for direct `5173` demo. |
| P1 | Cross-context workflow partial commits | Order/request services provision downstream records after persistence; frontend has reconciliation fallback. | Add atomic transaction boundary or outbox/process manager. |
| P1 | No global tenant query filters | `HasQueryFilter` absent; direct `AppDbContext` reads remain possible. | Add EF global filters or strict scoped query abstractions. |
| P1 | Stripe real payment not implemented | Foundation returns `501/503`; webhook only verifies signature. | Implement server-side Stripe SDK, idempotency, event persistence, payment state transitions. |
| P1 | Route accepting tenant id | `/dispatch-orders/by-tenant/{tenantId}` remains. | Replace with current-tenant route or enforce route tenant equals claim. |
| P2 | God Store remains | `data.store.js` 1482 lines; bulk collection loader and joins. | Continue strangler migration to context read stores. |
| P2 | Legacy RPC-like aliases | `/confirm`, `/reject`, `/cancel`, `/reserve`, `/paid`, `/mark-ready`. | Keep for compatibility now; deprecate later. |
| P2 | No explicit DB business CHECK constraints | No `HasCheckConstraint`; no public `pg_constraint` checks found. | Add non-destructive constraints for status/positive quantities after data audit. |
| P2 | Swagger auth metadata unclear | Swagger route extraction showed empty security arrays even though runtime auth works. | Fix OpenAPI security operation metadata for professor-facing clarity. |
| P3 | Multi-class/large files | Tenant admin controllers and operational repository files remain grouped. | Split only when changing behavior nearby. |

Final conclusion: hardening is **real**, especially backend auth, tenant middleware, repository guardrails, route transitions, DB FKs/indexes, and safe Stripe groundwork. It is **not ready to present as fully clean** until the local webapp login/config issue is fixed and the remaining P1 architecture risks are documented as intentional debt.

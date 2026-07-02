# Design Patterns Implementation

| Pattern | Where implemented | Why | Evidence | Remaining limitation |
|---|---|---|---|---|
| Aggregate | Sales, Warehouse, Invoicing domain | Protect lifecycle/quantity invariants | `Order`, `InventoryItem`, `Invoice`, `Payment` | Some operational records remain entity-style |
| Value Object | Domain model value objects | Valid construction and domain language | `Money`, `StockQuantity`, `WorkspaceSlug` | Some statuses remain strings |
| Repository | Domain interfaces + EF adapters | Isolate persistence | `IOrderRepository`, `OrderRepository` | Some read models use scoped DbContext directly |
| Unit of Work | Shared persistence | One transaction boundary | `IUnitOfWork`, `UnitOfWork` | Cross-context workflows still synchronous |
| Application Service | Context command/query services | Orchestrate use cases | `OrderCommandService`, `PurchaseRequestCommandService` | Large workflows need process managers |
| DTO/Resource + Assembler | REST Resources/Transform | Prevent EF/domain leakage | `OrderResource`, assemblers | Compatibility payloads remain |
| Domain Events | Shared event container + named events | Record business transitions | `BusinessDomainEvents.cs` | Not all entities emit events |
| Outbox | EF interceptor/table | Durable same-transaction integration intent | `DomainEventOutboxInterceptor`, `outbox_messages` | Dispatcher deferred |
| Middleware | Shared security/error pipeline | Cross-cutting auth/error policy | `WorkspaceMembershipValidationMiddleware` | Production telemetry pending |
| Interceptor | Axios + EF interceptors | Headers, errors, audit, outbox | `http.js`, `AuditableEntityInterceptor` | BFF/cookie auth not implemented |
| Adapter | API clients and external services | Isolate transport/provider | `BaseEndpoint`, Stripe services | Stripe keys/webhook not validated |
| Facade | Vue compatibility store | Safe strangler migration | `data.store.js` | Still large and widely consumed |
| CQRS-lite/read model | Query services | Screen-focused reads | `WorkspaceReadModelQueryService` | Pagination not uniform everywhere |
| Dependency Injection | ASP.NET Core container | Invert infrastructure dependencies | service collection extensions | None material locally |

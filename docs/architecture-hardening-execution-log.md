# Architecture Hardening Execution Log

## Initial State

- Start timestamp: 2026-07-01 10:00:37 -05
- Branch: `refactor/architecture-hardening`
- Platform baseline commit: `ff72946905718439f9ca3bc428f677078225c791`
- Webapp baseline commit: `22f160c6b6f3b121190238fa3f6f19c4053b6e32`
- Baseline tag: `baseline-before-architecture-hardening`

## Initial Validation

- Backend restore: passed with `dotnet restore nexa-platform.sln`.
- Backend build: passed with `dotnet build nexa-platform.sln --no-restore`; 0 warnings, 0 errors.
- Backend tests: passed with `dotnet test nexa-platform.sln --no-build`; 40/40 tests passed.
- Frontend build: passed with `npm run build`.

## Phases Completed

- Baseline setup: completed.
- Initial validation: completed.
- Phase 1 REST API consistency: completed for the safest high-impact compatibility routes.
- Phase 4 multi-tenancy hardening: completed safe interface/guard/helper scope without global EF filters.
- Phase 6 design pattern cleanup: completed safe domain-event foundation without activating partial event workflow.
- Phase 7 database hardening: completed safe query indexes and validation documentation.
- Phase 8 Render deploy readiness: completed env-driven port/database/CORS/migration/seed/swagger/health hardening.
- Frontend coordination: completed safe global data facade hardening, additive read-model API clients, and route/error handling alignment.
- Final runtime verification: completed Docker compose rebuild, HTTP smoke, and browser login smoke against local backend-backed data.

## Files Changed

- `docs/architecture-hardening-execution-log.md`: created to track this hardening pass.
- `King.Nexa.Platform/Sales/Interfaces/Rest/OrdersController.cs`: added canonical order transition route aliases.
- `King.Nexa.Platform/Warehouse/Interfaces/Rest/InventoryItemsController.cs`: moved legacy reserve/release item routes onto the reservation application service path.
- `King.Nexa.Platform/Warehouse/Application/CommandServices/IInventoryOperationsCommandService.cs`: added release-by-reservation-draft service contract for legacy compatibility.
- `King.Nexa.Platform/Warehouse/Application/Internal/CommandServices/InventoryOperationsCommandService.cs`: centralized reservation release behavior for id and code-based release flows.
- `King.Nexa.Platform/Warehouse/Domain/Repositories/IInventoryOperationsCommandRepository.cs`: added tenant-scoped active reservation lookup by item and code.
- `King.Nexa.Platform/Warehouse/Infrastructure/Persistence/EntityFrameworkCore/Repositories/InventoryOperationsCommandRepository.cs`: implemented tenant-scoped active reservation lookup.
- `docs/api-rest-conventions.md`: documented canonical resources, compatibility aliases, collection filters, errors, and tenant scope.
- `King.Nexa.Platform/Shared/Domain/Model/Entities/ITenantScoped.cs`: added tenant-scoped marker contract.
- `King.Nexa.Platform/Shared/Infrastructure/Persistence/EntityFrameworkCore/Repositories/BaseRepository.cs`: added fail-closed generic read guard for tenant-scoped entities.
- `King.Nexa.Platform/Shared/Infrastructure/Persistence/EntityFrameworkCore/Repositories/TenantScopedQueryableExtensions.cs`: added reusable tenant-scoping query helper.
- Tenant-owned aggregate/entity files: applied `ITenantScoped` to existing models with `TenantId`.
- `docs/multi-tenancy-hardening-plan.md`: documented deferred global-filter rollout and validation sequence.
- `King.Nexa.Platform/Shared/Domain/Model/Events/*`: added domain event contracts and in-memory aggregate event container.
- `docs/domain-events-outbox-foundation.md`: documented outbox/event candidates and rollout guardrails.
- `King.Nexa.Platform/*/Infrastructure/Persistence/EntityFrameworkCore/Configuration/Extensions/ModelBuilderExtensions.cs`: added safe tenant-scoped query indexes for orders, requests, dispatches, invoices, payments, and catalog filters.
- `King.Nexa.Platform/Migrations/20260701151138_AddSafeQueryIndexes.cs`: generated non-destructive index migration.
- `King.Nexa.Platform/Migrations/AppDbContextModelSnapshot.cs`: updated EF model snapshot for safe query indexes.
- `docs/database-hardening-report.md`: documented implemented indexes and deferred constraints/normalization.
- `docs/database-validation-queries.md`: added schema-first validation queries.
- `King.Nexa.Platform/Program.cs`: added Render `PORT`, `DATABASE_URL`, production CORS allowlist, `/health/live`, `/health/ready`, production Swagger opt-in, and startup migration/seed flags.
- `Dockerfile`: documented runtime port and non-root runtime user.
- `render.yaml`: declared production-safe env variables without hardcoded secrets.
- `docs/render-deploy-backend.md`: added Render setup, env vars, health checks, migration/seed strategy, and smoke commands.
- `../nexa-webapp/src/app/application/stores/data.store.js`: added per-collection error state to stop silent facade failures.
- `../nexa-webapp/src/*/infrastructure/*`: added additive read-model client methods for Buyer, Sales, Catalog, Client, and Dispatch surfaces.
- `../nexa-webapp/src/shared/infrastructure/http.js`: aligned 401/403 handling with history routing and structured backend error messages.
- `../nexa-webapp/docs/frontend-store-architecture.md`: documented the global facade boundary and extraction path.

## Risks

- `nexa-platform/ngrok.yml` is an untracked local file containing a real local tunnel token and was intentionally excluded from the baseline commit.
- The current objective asks for broad hardening. Changes must remain additive, non-destructive, and compatible with current Buyer, Sales, Logistics, and Owner flows.
- Docker publish reports `NU1903` for `Microsoft.OpenApi` 2.4.1. The package was not upgraded in this pass because dependency changes need an explicit approval window.
- The full architecture objective is larger than one safe pass. Backend role-specific read-model endpoints, pagination contracts, outbox dispatching, and broader facade extraction remain planned work.

## Validation After Current Phase

- Backend restore/build/test and frontend production build passed before implementation changes.
- After Phase 1: `dotnet build nexa-platform.sln --no-restore` passed with 0 warnings and 0 errors.
- After Phase 1: `dotnet test nexa-platform.sln --no-build` passed with 40/40 tests.
- After Phase 4 safe hardening: `dotnet build nexa-platform.sln --no-restore` passed with 0 warnings and 0 errors.
- After Phase 4 safe hardening: `dotnet test nexa-platform.sln --no-build` passed with 40/40 tests.
- After Phase 6 foundation: `dotnet build nexa-platform.sln --no-restore` passed with 0 warnings and 0 errors.
- After Phase 6 foundation: `dotnet test nexa-platform.sln --no-build` passed with 40/40 tests.
- After Phase 7 database hardening: `dotnet build nexa-platform.sln --no-restore` passed with 0 warnings and 0 errors.
- After Phase 7 database hardening: `dotnet test nexa-platform.sln --no-build` passed with 40/40 tests.
- After Phase 8 deploy hardening: `dotnet build nexa-platform.sln --no-restore` passed with 0 warnings and 0 errors.
- After Phase 8 deploy hardening: `dotnet test nexa-platform.sln --no-build` passed with 40/40 tests.
- Final backend tests: `dotnet test nexa-platform.sln --no-build` passed with 40/40 tests.
- Final frontend build: `npm run build` passed.
- Final Docker build: `docker build -t nexa-platform:architecture-hardening .` passed; Docker publish surfaced the `Microsoft.OpenApi` NU1903 warning.
- Final compose rebuild: `docker compose -f docker-compose.yml up -d --build postgres api webapp` passed.
- Final HTTP smoke: `/health`, `/health/live`, `/health/ready`, `/swagger/v1/swagger.json`, and webapp `/auth/login` returned 200.
- Final security smoke: anonymous `/api/v1/orders` returned 401; authenticated wrong-tenant request returned 403; authenticated correct-tenant request returned 200.
- Final browser smoke: Playwright login with `icisa` / `valeria.sanchez@icisa.pe` reached `/ops/operations/company-administration`; console warnings/errors were 0 and backend API requests returned 200.

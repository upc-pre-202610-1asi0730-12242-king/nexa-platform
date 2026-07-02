# API Security Audit

## Default rule

Endpoint security is authenticated by default. `AddAuthorization` configures both `DefaultPolicy` and `FallbackPolicy` with the Nexa JWT scheme and `RequireAuthenticatedUser()`.

Any endpoint that is public must use `[AllowAnonymous]` explicitly and must be listed in this document.

## Explicit public endpoints

| Endpoint | Reason | Exposure control |
| --- | --- | --- |
| `POST /api/v1/authentication/sign-in` | User authentication entry point. | Returns JWT only after valid credentials and workspace selection. |
| `POST /api/v1/organization-registrations` | Public onboarding request. | Creates an onboarding registration, not tenant operational access. |
| `GET /api/v1/tenants/by-slug/{slug}` | Workspace discovery for sign-in/onboarding. | Returns preview data only; no tenant id, legal details, RUC or email domain. |
| `GET /health/live` | Deployment liveness. | No database or tenant data. |
| `POST /api/v1/payments/stripe/webhook` | Stripe server-to-server callback. | Requires `STRIPE_WEBHOOK_SECRET` and verifies `Stripe-Signature`; no payment state is changed without verification. |

Swagger is available only in Development or when `ENABLE_SWAGGER=true`.

## Protected business endpoints

All tenant, catalog, sales, buyer, warehouse, logistics, invoicing, audit and reference-data endpoints are protected by default. Controllers may also declare stronger policies.

Role-specific examples:

- Sales read models: `CanAcceptPurchaseRequest`.
- Purchase request acceptance: `CanAcceptPurchaseRequest`.
- Order creation: `CanCreateOrder`.
- Dispatch route start and delivery completion: `CanStartDispatch`.
- Invoicing writes and payment status changes: `CanManageDocuments`.
- Payment method changes: `CanManagePaymentMethods`.
- Tenant administration: `CanManageWorkspace`.

## Tenant and workspace enforcement

Tenant scope is enforced from authenticated JWT claims and active membership records. Frontend headers are not trusted as the source of authorization.

`WorkspaceMembershipValidationMiddleware` rejects:

- missing or invalid workspace membership claims
- inactive membership
- `X-Nexa-Tenant-Id` that does not match the authenticated tenant claim
- `X-Nexa-Workspace` that does not match the authenticated workspace slug

Data queries must still remain tenant scoped at repository/query-service level.

## Smoke-test contract

Required runtime checks:

- no token -> `401` for protected endpoint
- wrong tenant/workspace -> `403`
- correct authenticated user -> `200`
- buyer cannot access sales/admin-only endpoint -> `403`
- logistics cannot access owner-only endpoint -> `403`
- public endpoints still work

## Stripe payment security

Frontend uses only `VITE_STRIPE_PUBLISHABLE_KEY`.

Backend uses only environment-backed secrets:

- `STRIPE_SECRET_KEY`
- `STRIPE_WEBHOOK_SECRET`

Checkout Session and PaymentIntent endpoints do not fake success. Until server-side Stripe object creation is fully implemented, they return a non-success preparation response and do not mark any payment as paid.

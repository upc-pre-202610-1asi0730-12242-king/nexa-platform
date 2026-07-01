# Render Backend Deployment

## Service

- Runtime: Docker
- Dockerfile: `./Dockerfile`
- Health check path: `/health/live`
- Internal port: `8080`

The application also supports Render `PORT`; if `ASPNETCORE_URLS` is not set, startup binds to `http://0.0.0.0:${PORT}`.

## Required Environment Variables

- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__DefaultConnection=<postgres connection string>`
- `NEXA_JWT_SECRET=<secret with at least 32 characters>`
- `NEXA_JWT_ISSUER=nexa-platform`
- `NEXA_JWT_AUDIENCE=nexa-webapp`
- `CORS_ALLOWED_ORIGINS=<frontend origin list>`
- `STRIPE_SECRET_KEY=<backend Stripe secret key>`
- `STRIPE_WEBHOOK_SECRET=<Stripe webhook signing secret>`

`DATABASE_URL` is also supported and normalized at startup when `ConnectionStrings__DefaultConnection` is not provided.

## Optional Environment Variables

- `APPLY_MIGRATIONS_ON_STARTUP=true|false`
- `SEED_DEMO_DATA=true|false`
- `ENABLE_SWAGGER=true|false`
- `DATA_PROTECTION__KEYS_PATH=<persistent key path>`

Production defaults should be:

- `APPLY_MIGRATIONS_ON_STARTUP=false`
- `SEED_DEMO_DATA=false`
- `ENABLE_SWAGGER=false`

## Health Checks

- `/health`: aggregate ASP.NET health endpoint; authenticated by default.
- `/health/live`: process liveness, no database requirement.
- `/health/ready`: database readiness through EF connectivity; authenticated by default.

Use `/health/live` for unauthenticated deployment liveness checks. Use `/health/ready` for authenticated deployment verification after the database is attached.

## Migration Strategy

Do not rely on automatic migrations for multi-instance production deploys. Run migrations as an explicit release step or set `APPLY_MIGRATIONS_ON_STARTUP=true` only for controlled single-instance deployments.

## Seed Strategy

Demo seed is Development-only and requires `SEED_DEMO_DATA=true`. Production must keep seed disabled.

## CORS

Production CORS is allowlist-only through `CORS_ALLOWED_ORIGINS`. Localhost origins are added only in Development.

## Smoke Commands

```bash
curl -i https://<render-service>/health
curl -i https://<render-service>/health/live
curl -i https://<render-service>/health/ready
curl -i https://<render-service>/swagger/index.html
```

Swagger should return 404 or another non-success response in Production unless `ENABLE_SWAGGER=true`.

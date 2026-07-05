# Contributing to Nexa Platform

## Repository Context

`nexa-platform` is the RESTful backend for the Nexa TB2 delivery.

| Field | Value |
|---|---|
| Current release | `v2.0.0` |
| Delivery | TB2 |
| Runtime | .NET 10 / ASP.NET Core |
| Persistence | PostgreSQL / EF Core |
| Live API | https://nexa-platform-20wt.onrender.com |
| Swagger UI | https://nexa-platform-20wt.onrender.com/swagger/index.html |

## Workflow

1. Create a branch from `develop` unless the maintainer requests a release correction from `main`.
2. Keep changes scoped to one bounded context or one documentation concern.
3. Use conventional commits.
4. Validate locally before opening a pull request.
5. Do not commit local credentials, `.env` files, `appsettings.Local.json`, database dumps, or generated build output.

## Branch Names

| Prefix | Use |
|---|---|
| `feature/` | New backend capability |
| `fix/` | Bug fix |
| `docs/` | Documentation update |
| `refactor/` | Internal restructuring without contract change |
| `chore/` | Configuration, tooling, release maintenance |

## Architecture Rules

- Keep controllers thin and route through command/query services.
- Keep domain behavior inside aggregates, entities, value objects, and domain services.
- Keep EF Core mapping, migrations, and repository implementations in Infrastructure.
- Use resources and assemblers for REST contracts.
- Preserve tenant/workspace authorization checks.
- Do not expose secrets, connection strings, or production credentials in source control.

## Validation Checklist

Before requesting review:

```bash
dotnet build nexa-platform.sln
dotnet test nexa-platform.sln
```

For API-facing work, also verify Swagger and relevant protected endpoints with the correct JWT and workspace headers.

## Pull Request Notes

Each pull request should include:

- Scope and bounded context.
- REST contract impact, if any.
- Database migration impact, if any.
- Validation commands and results.
- Deployment or environment notes, if any.

---

Team King · UPC · Aplicaciones Web · TB2 · 2026-10

# Nexa Platform Security Policy

## Supported Version

| Repository | Current Release | Delivery | Security Support |
|---|---:|---|---|
| `nexa-platform` | `v2.0.1` | TB2 | Active |

Older tags remain available for academic traceability, but security corrections are applied to the latest active release line.

## Scope

Security review for this repository includes:

- ASP.NET Core authentication and authorization.
- JWT configuration and signing key handling.
- Workspace and tenant access isolation.
- REST controller policies and protected endpoints.
- PostgreSQL connection handling and EF Core migrations.
- Render deployment configuration and environment variables.
- Swagger/OpenAPI exposure and API documentation.

## Live Security Surface

| Service | URL |
|---|---|
| Platform API | https://nexa-platform-20wt.onrender.com |
| Swagger UI | https://nexa-platform-20wt.onrender.com/swagger/index.html |

## Reporting a Vulnerability

Do not open a public issue for vulnerabilities. Report privately to the Team King maintainers or through GitHub private vulnerability reporting when available.

Include:

- Affected endpoint, file, or configuration.
- Steps to reproduce.
- Expected and actual behavior.
- Risk level and suggested mitigation, if known.

## Security Requirements

- Do not commit secrets, database credentials, JWT signing keys, API tokens, or `.env` files.
- Keep `appsettings.Local.json` local-only.
- Keep tenant and workspace checks active on protected resources.
- Validate any public endpoint with explicit anonymous access requirements.
- Review dependency warnings before release.

---

Team King · UPC · Aplicaciones Web · TB2 · 2026-10

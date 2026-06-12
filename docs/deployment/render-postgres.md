# Render PostgreSQL Deployment Notes

## Services

- Render Web Service: `nexa-platform-api`
- Render PostgreSQL service: `nexa-platform-db`
- PostgreSQL database: `nexa_platform_db`
- PostgreSQL user: `nexa_user`
- PostgreSQL port: `5432`

## Environment Variables

Configure these values in the Render Web Service environment:

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=<internal-render-host>;Port=5432;Database=nexa_platform_db;Username=nexa_user;Password=<render-password>;SSL Mode=Require;Trust Server Certificate=true
SeedData__Enabled=true
AllowedOrigins__0=http://localhost:5173
AllowedOrigins__1=https://<frontend-deployed-url>
```

Do not commit real Render hosts, passwords, private URLs, or full production connection strings.

## First Deploy

1. Connect Render Web Service `nexa-platform-api` to branch `release/av2-render-postgres`.
2. Use Docker deployment from the repository `Dockerfile`.
3. Configure the environment variables above in Render.
4. Deploy once with `SeedData__Enabled=true` so EF Core applies migrations and loads seed data.
5. After the first successful deploy, change:

```text
SeedData__Enabled=false
```

## Security Follow-Up

Rotate any credential that was exposed outside Render or a local ignored secret file. Keep `appsettings.Local.json` ignored and never stage it.

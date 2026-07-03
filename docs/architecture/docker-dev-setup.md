# Nexa Docker Development Setup

Run from `nexa-platform`.

```bash
docker compose config
docker compose config --services
docker compose up --build -d
docker compose ps
```

Expected services:

| Service | Port | Purpose |
|---|---:|---|
| `postgres` | 5432 | PostgreSQL database |
| `api` | 5068 | ASP.NET Core API |
| `webapp` | 5173 | Vue/Vite web application |

Smoke checks:

```bash
/usr/bin/curl -i http://localhost:5068/health
/usr/bin/curl -i http://localhost:5068/swagger/index.html
/usr/bin/curl -i http://localhost:5173
```

The API image must be rebuilt after backend changes. A plain `docker compose up -d` can keep an older API image running.

The compose values are local-development credentials only. Production must provide `CONNECTION_STRINGS__DEFAULT_CONNECTION`, `NEXA_JWT_SECRET`, `NEXA_JWT_ISSUER`, `NEXA_JWT_AUDIENCE`, and allowed origins through its secret/configuration system. Startup rejects placeholder JWT keys.

DataProtection keys are persisted through the configured Docker volume so JWT/session-related protection does not reset on every container restart.

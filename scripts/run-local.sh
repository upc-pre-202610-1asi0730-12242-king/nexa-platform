#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PLATFORM_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
REPOSITORIES_DIR="$(cd "$PLATFORM_DIR/.." && pwd)"
WEBAPP_DIR="$REPOSITORIES_DIR/nexa-webapp"
API_URL="http://localhost:5068"
WEB_URL="http://127.0.0.1:5173"

if [ ! -d "$WEBAPP_DIR" ]; then
  echo "nexa-webapp not found at $WEBAPP_DIR" >&2
  exit 1
fi

cd "$PLATFORM_DIR"
docker compose up -d postgres
docker compose stop api >/dev/null 2>&1 || true

dotnet run --project "$PLATFORM_DIR/King.Nexa.Platform/King.Nexa.Platform.csproj" --urls "$API_URL" &
API_PID=$!
trap 'kill "$API_PID" >/dev/null 2>&1 || true' EXIT

echo "Waiting for Nexa Platform at $API_URL..."
for _ in $(seq 1 60); do
  if /usr/bin/curl -fsS "$API_URL/health" >/dev/null; then
    break
  fi
  sleep 1
done

/usr/bin/curl -fsS "$API_URL/health" >/dev/null

echo ""
echo "Nexa local stack ready"
echo "API:     $API_URL/swagger"
echo "WebApp:  $WEB_URL"
echo ""
echo "Workspace: icisa"
echo "Logistics: roberto.garcia@icisa.pe / Password123!"
echo "Sales:     valeria.sanchez@icisa.pe / Password123!"
echo "Buyer:     elena.litano@icisa.pe / Password123!"
echo ""

cd "$WEBAPP_DIR"
npm run dev -- --host 127.0.0.1

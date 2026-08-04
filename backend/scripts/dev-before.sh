#!/usr/bin/env bash
set -euo pipefail

BACKEND_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$BACKEND_DIR"

set -a; . ./.env; set +a
CONTAINER="${DB_CONTAINER:-zajednica-postgres}"

echo ">>> Postgres"
docker compose up -d

echo -n "    cekam da baza bude spremna"
for _ in $(seq 1 30); do
  if docker exec "$CONTAINER" pg_isready -U "$POSTGRES_USER" -d "$POSTGRES_DB" >/dev/null 2>&1; then
    echo " -> spremna"
    break
  fi
  echo -n "."
  sleep 1
done

echo ">>> Sync IP -> mobile/.env"
"$BACKEND_DIR/scripts/sync-ip.sh" 5265 || true

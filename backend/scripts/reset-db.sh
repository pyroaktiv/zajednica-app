#!/usr/bin/env bash
set -euo pipefail

BACKEND_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$BACKEND_DIR"

set -a; . ./.env; set +a

MODULES=("${@:-}")
[ -z "${MODULES[0]}" ] && MODULES=(Identity Community Feed Chat)

CONTAINER=${DB_CONTAINER:-zajednica-postgres}
DATABASE=${POSTGRES_DB}
DB_USER=${POSTGRES_USER}

dotnet tool restore >/dev/null

for MODULE in "${MODULES[@]}"; do
  SCHEMA=$(echo "$MODULE" | tr '[:upper:]' '[:lower:]')
  PROJECT="src/Modules/$MODULE/Zajednica.$MODULE.Infrastructure"

  echo ">>> $MODULE"
  docker exec "$CONTAINER" psql -U "$DB_USER" -d "$DATABASE" -q \
    -c "DROP SCHEMA IF EXISTS \"$SCHEMA\" CASCADE;"

  ASPNETCORE_ENVIRONMENT=Development dotnet ef database update \
    --project "$PROJECT" \
    --startup-project src/Zajednica.Api \
    --context "${MODULE}DbContext" >/dev/null

  echo "    schema '$SCHEMA' recreated from committed migrations"
done

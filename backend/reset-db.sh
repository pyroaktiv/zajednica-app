#!/usr/bin/env bash
# Regenerates each module's schema from scratch.
#
# Migrations are not committed (see .gitignore): while the domain model is still moving, every
# module gets a single Initial migration generated against a dropped schema. Existing data in the
# target schemas is destroyed.
#
#   ./reset-db.sh                  all modules
#   ./reset-db.sh Identity Feed    only the named ones
set -euo pipefail

cd "$(dirname "$0")"

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

  rm -rf "$PROJECT/Migrations"

  ASPNETCORE_ENVIRONMENT=Development dotnet ef migrations add Initial \
    --project "$PROJECT" \
    --startup-project src/Zajednica.Api \
    --context "${MODULE}DbContext" >/dev/null

  ASPNETCORE_ENVIRONMENT=Development dotnet ef database update \
    --project "$PROJECT" \
    --startup-project src/Zajednica.Api \
    --context "${MODULE}DbContext" >/dev/null

  echo "    schema '$SCHEMA' recreated"
done

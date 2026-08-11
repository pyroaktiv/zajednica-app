#!/usr/bin/env bash
set -euo pipefail

PORT="${1:-5265}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ENV_FILE="$SCRIPT_DIR/../../mobile/.env"

detect_ip() {
  local ip
  ip="$(ip -4 route get 8.8.8.8 2>/dev/null | grep -oP '(?<=src )\d+(\.\d+){3}' | head -1 || true)"
  [ -z "$ip" ] && ip="$(hostname -I 2>/dev/null | tr ' ' '\n' | grep -vE '^127\.' | head -1 || true)"
  echo "$ip"
}

IP="$(detect_ip)"
if [ -z "$IP" ]; then
  echo "sync-ip: nije nadjena LAN IP adresa; ostavljam $ENV_FILE netaknut." >&2
  exit 0
fi

URL="http://$IP:$PORT"
printf 'EXPO_PUBLIC_API_URL=%s\n' "$URL" > "$ENV_FILE"
echo "sync-ip: $ENV_FILE -> $URL"

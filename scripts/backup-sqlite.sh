#!/usr/bin/env bash
# Backs up the SQLite game database using SQLite's own online backup command
# (".backup"), which is safe to run against a live, in-use database (including
# one in WAL mode) because it goes through SQLite's backup API instead of
# copying the file bytes directly.
#
# Usage: scripts/backup-sqlite.sh [source-db] [backup-dir] [retention-days]
# Example cron entry (daily at 03:00, 14 days retention):
#   0 3 * * * /path/to/scripts/backup-sqlite.sh /data/stargate-galactic-command.db /data/backups 14

set -euo pipefail

SOURCE_DB="${1:-stargate-galactic-command.db}"
BACKUP_DIR="${2:-backups}"
RETENTION_DAYS="${3:-14}"

if ! command -v sqlite3 >/dev/null 2>&1; then
  echo "sqlite3 CLI not found; install it (e.g. apt-get install sqlite3) to run backups." >&2
  exit 1
fi

if [ ! -f "$SOURCE_DB" ]; then
  echo "Source database not found: $SOURCE_DB" >&2
  exit 1
fi

mkdir -p "$BACKUP_DIR"
timestamp="$(date -u +%Y%m%dT%H%M%SZ)"
dest="$BACKUP_DIR/$(basename "$SOURCE_DB" .db)-$timestamp.db"

sqlite3 "$SOURCE_DB" ".backup '$dest'"
echo "Backup written to $dest"

find "$BACKUP_DIR" -name "$(basename "$SOURCE_DB" .db)-*.db" -type f -mtime "+$RETENTION_DAYS" -print -delete

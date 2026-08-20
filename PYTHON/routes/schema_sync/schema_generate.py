"""
schema_generate.py -- work out what needs to change, and record it.

Reads AVACONT_SURSA and each target out of information_schema, compares
them in Python, and writes the resulting statements into
schema_diff_log. Executes no DDL of its own.

Usage:
    python schema_generate.py --mode SAFE
    python schema_generate.py --mode FORCE --targets 000_DEMO,018_GRRS
    python schema_generate.py --mode SAFE --no-reset

Options:
    --mode      SAFE (additive plus collation repair) or FORCE (also
                modifies and drops).
    --no-reset  Keep previously generated, unexecuted rows. By default
                they are cleared first, so a second run does not queue
                the same work twice.
    --targets   Override discovery. Default: every distinct DbName in
                AVACONT_COMUN.CAI that exists on the server.
"""

import argparse
import sys

import mysql.connector

from .schema_common import (CONTROL_DB, SchemaSyncError, check_prerequisites,
                            connect, discover_targets, ensure_control_table,
                            fetch_pending, parse_targets, setup_logging,
                            summarise)
from .schema_diff import build_diff


def clear_pending(conn, logger) -> int:
    """Delete unexecuted rows. Executed rows are the history; kept."""
    cur = conn.cursor()
    try:
        cur.execute(f"DELETE FROM {_tbl()} WHERE executed_at IS NULL")
        n = cur.rowcount
    finally:
        cur.close()
    if n:
        logger.info("Șterse %d rânduri neexecutate din rulările anterioare.", n)
    return n


def _tbl() -> str:
    return f"`{CONTROL_DB}`.`schema_diff_log`"


def persist(conn, statements: list, mode: str, logger) -> None:
    """Write the statements into schema_diff_log in one batch."""
    if not statements:
        return
    rows = [(s.target_db, s.table_name, s.object_name, s.object_type,
             s.action_type, s.ddl_sql, mode, int(s.is_destructive),
             s.priority, s.error_msg)
            for s in statements]
    cur = conn.cursor()
    try:
        cur.executemany(
            f"INSERT INTO {_tbl()} "
            f"(target_db, table_name, object_name, object_type, action_type, "
            f" ddl_sql, sync_mode, is_destructive, priority, error_msg) "
            f"VALUES (%s,%s,%s,%s,%s,%s,%s,%s,%s,%s)", rows)
    finally:
        cur.close()
    logger.info("Scrise %d instrucțiuni în schema_diff_log.", len(rows))


def generate(conn, targets, mode, logger, reset=True) -> list:
    """Full generation pass. Returns the pending rows as stored."""
    ensure_control_table(conn, logger)
    if reset:
        clear_pending(conn, logger)

    statements = build_diff(conn, targets, mode, logger)

    blocked = [s for s in statements if s.error_msg]
    for s in blocked:
        logger.error("%s.%s — %s", s.target_db, s.table_name, s.error_msg)

    persist(conn, statements, mode, logger)
    return fetch_pending(conn, targets)


def main(argv=None) -> int:
    parser = argparse.ArgumentParser(
        description="Generează diferențele de schemă în schema_diff_log.")
    parser.add_argument("--mode", choices=["SAFE", "FORCE"], default="SAFE")
    parser.add_argument("--no-reset", action="store_true")
    parser.add_argument("--targets", default=None)
    parser.add_argument("--verbose", action="store_true")
    args = parser.parse_args(argv)

    logger = setup_logging(args.verbose)
    conn = None
    try:
        conn = connect()
        check_prerequisites(conn, logger)
        targets = (parse_targets(args.targets) if args.targets
                   else discover_targets(conn, logger))

        rows = generate(conn, targets, args.mode, logger,
                        reset=not args.no_reset)

        print()
        print(summarise(rows))
        print()
        destructive = [r for r in rows if r["is_destructive"]]
        if destructive:
            print(f"  {len(destructive)} DISTRUCTIVE. Execuția le refuză "
                  f"fără --allow-destructive.")
        return 0

    except SchemaSyncError as exc:
        logger.error("%s", exc)
        return 1
    except mysql.connector.Error as exc:
        logger.error("Eroare MariaDB: [%s] %s",
                     getattr(exc, "errno", "?"), exc)
        return 1
    finally:
        if conn is not None and conn.is_connected():
            conn.close()


if __name__ == "__main__":
    sys.exit(main())

# routes/migrare/execute.py
# -----------------------------------------------------------------------------
# The write pass. Runs only after an analysis of the SAME file said it may.
#
# UPSERT everywhere: `ON DUPLICATE KEY UPDATE <every column> = VALUES(...)`.
# A row that is already on the server is BROUGHT UP TO DATE with the one in the
# Access file, not left as it was -- the Access file is the source of truth of
# the migration, and a half-migrated row that never gets corrected is worse than
# no row at all. Deliberately not `INSERT IGNORE`, which would also degrade type
# errors, truncations and constraint violations to warnings, i.e. swallow real
# failures.
#
# The primary-key columns stay OUT of the UPDATE list: they are what identifies
# the row, and assigning them to themselves is noise. Under this form MariaDB
# reports rowcount 1 for an inserted row, 2 for one it actually changed and 0
# for one that was already identical, so the three counts are exact.
#
# The Access primary keys are copied VERBATIM: none of the sixteen tables is
# AUTO_INCREMENT on MariaDB, and the intra-family FK columns (IDRH, IDRR, IDRZ,
# IDEXF, IDR, IDH ...) only stay valid if the ids do.
# -----------------------------------------------------------------------------

import logging

from . import accdb, tables, validate

logger = logging.getLogger(__name__)

# Cate randuri intr-un singur executemany.
BATCH_ROWS = 500


class ExecuteError(Exception):
    """Scrierea nu poate continua — mesaj în română."""


def run(conn, db_name, fx_path, plan, report, force, only=None, progress=None):
    """
    Write the rows that passed the analysis.

    `plan` is the SAME UnitPlan the analysis used, not a fresh one: otherwise the
    selection could shift between measuring and writing.

    `only` are the tables the operator ticked. They must be among the ANALYSED
    ones -- an unmeasured table is not written.

    `report` is the Report of the analysis of the same file: the foreign-key
    values missing on the target come from there, so the server is not asked twice.

    force=False -> any finding stops the write (the caller should not have got
                   here; we check anyway rather than trusting the UI).
    force=True  -> the offending rows are SKIPPED and counted; blocking findings
                   still stop the write here too, not only in the UI.
    """
    def say(msg):
        logger.info("migrare/scriere: %s", msg)
        if progress:
            progress(msg)

    if report.has_blocking():
        raise ExecuteError(
            "Analiza a găsit probleme blocante (tip, dimensiune, coloană sau tabel "
            "lipsă). Scrierea nu pornește nici forțat — acelea strică date.")
    if not force and not report.is_clean():
        raise ExecuteError(
            "Analiza a găsit probleme de integritate. Pornirea normală nu este "
            "permisă; folosiți «Forțează rularea» dacă acceptați ca rândurile "
            "vinovate să fie sărite.")

    chosen = tables.selected(only)
    unmeasured = [t.name for t in chosen if report.tables and t.name not in report.tables]
    if unmeasured:
        raise ExecuteError(
            "Tabelele %s nu au fost analizate, deci nu se pot scrie. Rulați din nou "
            "analiza cu ele bifate." % ", ".join(unmeasured))

    schema = validate.TargetSchema(conn, db_name, [t.name for t in chosen])
    totals = {}

    for table in chosen:
        stats = {"citite": 0, "ale_unității": 0, "scrise": 0,
                 "actualizate": 0, "neschimbate": 0, "sărite": 0}
        totals[table.name] = stats

        if not schema.has(table.name):
            raise ExecuteError(
                "Tabelul «%s» lipsește din baza «%s». Migrarea nu creează tabele."
                % (table.name, db_name))

        target_columns = schema.columns[table.name]
        selector = plan.selector_for(table)
        pk_columns = schema.primary_key.get(table.name) or [table.primary_key]
        seen_keys = set()
        missing = dict((column, values)
                       for (t, column), values in report.missing_fk.items()
                       if t == table.name)

        say("Se scrie «%s»." % table.name)
        # Only the columns BOTH sides have. A column that exists only on the
        # target keeps its default; one that exists only in Access was already
        # reported as missing by the analysis, so it never gets here.
        access_columns = [c["nume"] for c in accdb.columns(fx_path, table.name)]
        columns = [c for c in access_columns if c in target_columns]
        if not columns:
            raise ExecuteError(
                "Tabelul «%s» nu are nicio coloană comună între Access și «%s»."
                % (table.name, db_name))
        batch = []

        for row in accdb.iter_rows(fx_path, table.name):
            stats["citite"] += 1
            keep, reject = selector.keep(row)
            if reject:
                stats["sărite"] += 1
                continue
            if not keep:
                # The row belongs to another unit in the same file: it is not this
                # database's, and it does not count as skipped.
                continue
            stats["ale_unității"] += 1

            if _row_is_blocked(row, target_columns, pk_columns, seen_keys, missing):
                stats["sărite"] += 1
                continue

            batch.append(tuple(row.get(c) for c in columns))
            if len(batch) >= BATCH_ROWS:
                _write(conn, db_name, table.name, columns, pk_columns, batch, stats)
                batch = []

        if batch:
            _write(conn, db_name, table.name, columns, pk_columns, batch, stats)

        conn.commit()
        say("«%s»: %d scrise, %d actualizate, %d deja identice, %d sărite."
            % (table.name, stats["scrise"], stats["actualizate"],
               stats["neschimbate"], stats["sărite"]))

    return totals


def _row_is_blocked(row, target_columns, pk_columns, seen_keys, missing):
    """
    The same rules as the analysis, applied row by row. Deliberately re-evaluated
    here rather than read from a saved list of keys: the list could no longer match
    the file, and a mismatch would write exactly the wrong row.
    """
    for name, meta in target_columns.items():
        if meta["auto"] and name not in row:
            continue
        if validate.check_value(meta, row.get(name)):
            return True

    pk_value = tuple(row.get(c) for c in pk_columns)
    if pk_value in seen_keys:
        return True
    seen_keys.add(pk_value)

    for column, bad_values in missing.items():
        value = row.get(column)
        if value is None:
            continue
        if value in bad_values or validate.as_int(value) in bad_values:
            return True

    return False


def _write(conn, db_name, table, columns, pk_columns, rows, stats):
    """
    A row that does not exist is INSERTED; one that does is BROUGHT UP TO DATE
    from the Access file. The primary-key columns stay out of the update list --
    they identify the row.
    """
    quoted = ",".join("`%s`" % c for c in columns)
    placeholders = ",".join(["%s"] * len(columns))
    keys = set(c.lower() for c in pk_columns)
    updatable = [c for c in columns if c.lower() not in keys]
    if not updatable:
        # A table where nothing can be updated (every shared column is part of the
        # key): self-assignment, so a duplicate does not fail.
        updatable = [columns[0]]
    updates = ",".join("`%s` = VALUES(`%s`)" % (c, c) for c in updatable)

    sql = ("INSERT INTO `%s`.`%s` (%s) VALUES (%s) ON DUPLICATE KEY UPDATE %s"
           % (db_name, table, quoted, placeholders, updates))

    cur = conn.cursor()
    try:
        for row in rows:
            cur.execute(sql, row)
            # MariaDB: 1 = inserted, 2 = updated, 0 = already identical.
            if cur.rowcount == 1:
                stats["scrise"] += 1
            elif cur.rowcount == 2:
                stats["actualizate"] += 1
            else:
                stats["neschimbate"] += 1
    except Exception as exc:
        conn.rollback()
        raise ExecuteError("Scrierea în «%s» a eșuat: %s" % (table, exc))
    finally:
        cur.close()

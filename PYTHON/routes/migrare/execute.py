# routes/migrare/execute.py
# -----------------------------------------------------------------------------
# The write pass. Runs only after an analysis of the SAME file said it may.
#
# Insert-if-absent, never overwrite: `ON DUPLICATE KEY UPDATE <prima coloană> =
# <prima coloană>` -- a self-assignment that does nothing. Deliberately not
# `INSERT IGNORE`, which would also degrade type errors, truncations and
# constraint violations to warnings, i.e. swallow real failures. Under this form
# cursor.rowcount is 1 per inserted row and 0 per skipped duplicate, so the
# counts reported back are exact.
#
# The Access primary keys are copied VERBATIM: none of the sixteen tables is
# AUTO_INCREMENT on MariaDB, and the intra-family FK columns (IDRH, IDRR, IDRZ,
# IDEXF, IDR, IDH ...) only stay valid if the ids do.
# -----------------------------------------------------------------------------

import logging

from . import accdb, routing, tables, validate

logger = logging.getLogger(__name__)

# Cate randuri intr-un singur executemany.
BATCH_ROWS = 500


class ExecuteError(Exception):
    """Scrierea nu poate continua — mesaj în română."""


def run(conn, db_name, fx_path, plan, report, force, progress=None):
    """
    Scrie randurile care au trecut analiza.

    `plan` e ACELASI RoutingPlan folosit la analiza, nu unul nou: altfel ramura
    aleasa s-ar putea schimba intre masurare si scriere.

    `report` e Report-ul analizei aceluiasi fisier: de acolo vin valorile de cheie
    straina care lipsesc pe tinta, ca sa nu mai intrebam serverul inca o data.

    force=False -> orice constatare opreste scrierea (apelantul nu ar fi trebuit
                   sa ajunga aici; verificam oricum, nu ne bazam pe interfata).
    force=True  -> randurile vinovate se SAR, si sunt numarate; blocantele opresc
                   in continuare, si aici, nu doar in interfata.
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

    schema = validate.TargetSchema(conn, db_name, [t.name for t in tables.ALL])
    totals = {}

    for table in tables.ALL:
        stats = {"citite": 0, "rutate": 0, "scrise": 0, "existente": 0, "sărite": 0}
        totals[table.name] = stats

        if not schema.has(table.name):
            raise ExecuteError(
                "Tabelul «%s» lipsește din baza «%s». Migrarea nu creează tabele."
                % (table.name, db_name))

        target_columns = schema.columns[table.name]
        router = plan.router_for(table)
        pk_columns = schema.primary_key.get(table.name) or [table.primary_key]
        seen_keys = set()
        missing = dict((column, values)
                       for (t, column), values in report.missing_fk.items()
                       if t == table.name)

        say("Se scrie «%s»." % table.name)
        # Doar coloanele pe care le au AMANDOUA. O coloana care exista numai pe
        # tinta isi pastreaza valoarea implicita; una care exista numai in Access
        # a fost deja raportata ca lipsa la analiza, deci nu ajungem aici cu ea.
        access_columns = [c["nume"] for c in accdb.columns(fx_path, table.name)]
        columns = [c for c in access_columns if c in target_columns]
        if not columns:
            raise ExecuteError(
                "Tabelul «%s» nu are nicio coloană comună între Access și «%s»."
                % (table.name, db_name))
        batch = []

        for row in accdb.iter_rows(fx_path, table.name):
            stats["citite"] += 1
            dcs, reject = router.route(row)
            if reject:
                stats["sărite"] += 1
                continue
            if db_name not in dcs:
                continue
            stats["rutate"] += 1

            if _row_is_blocked(row, target_columns, pk_columns, seen_keys, missing):
                stats["sărite"] += 1
                continue

            batch.append(tuple(row.get(c) for c in columns))
            if len(batch) >= BATCH_ROWS:
                _write(conn, db_name, table.name, columns, batch, stats)
                batch = []

        if batch:
            _write(conn, db_name, table.name, columns, batch, stats)

        conn.commit()
        say("«%s»: %d scrise, %d deja existente, %d sărite."
            % (table.name, stats["scrise"], stats["existente"], stats["sărite"]))

    return totals


def _row_is_blocked(row, target_columns, pk_columns, seen_keys, missing):
    """
    Aceleasi reguli ca la analiza, aplicate rand cu rand. Deliberat rescrise aici
    si nu citite dintr-o lista de chei salvata: lista ar putea sa nu mai
    corespunda fisierului, iar o nepotrivire ar scrie exact randul gresit.
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


def _write(conn, db_name, table, columns, rows, stats):
    quoted = ",".join("`%s`" % c for c in columns)
    placeholders = ",".join(["%s"] * len(columns))
    sql = ("INSERT INTO `%s`.`%s` (%s) VALUES (%s) "
           "ON DUPLICATE KEY UPDATE `%s` = `%s`"
           % (db_name, table, quoted, placeholders, columns[0], columns[0]))

    cur = conn.cursor()
    try:
        for row in rows:
            cur.execute(sql, row)
            if cur.rowcount == 1:
                stats["scrise"] += 1
            else:
                stats["existente"] += 1
    except Exception as exc:
        conn.rollback()
        raise ExecuteError("Scrierea în «%s» a eșuat: %s" % (table, exc))
    finally:
        cur.close()

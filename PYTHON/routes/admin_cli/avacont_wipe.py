"""Accounting wipe for the private KBotAdmin CLI: empties every TZ_ table of ONE unit database and the links of the received invoices to
their operations (EF_PrimiteOperatii), so the accounting data can be migrated again from the start.

WRITES, IRREVERSIBLE. The targets are a fixed prefix (`TZ_`) plus ONE named EF_ table, and every target is checked against that rule again right before it is
emptied. The structure stays; only rows go. Nothing else is touched: Parteneri, Clasificatii, FX_, AD_, VC_ and the other EF_ tables (the received invoices
themselves, their lines and messages) stay. No table outside TZ_ has a foreign key into a TZ_ table, so nothing is left pointing at a deleted row.
The operator must type the database name as the confirmation (the command's typed value), checked here and not only in the client.
"""
from routes.admin_cli.admin_common import AdminError, _exec, _journal, _q, _rows

TITLE = "Golire date contabile"
TZ_PREFIX = "TZ_"
# The links of the received invoices to their accounting operations (booked in K-BOT or imported from Access): without the operations they point at nothing.
# The E-Factura import of Access puts the Access links back (it adds the links of invoices that already exist).
EF_TABLES = ("EF_PrimiteOperatii",)
# The databases that only hold the structure: never emptied by this command.
REFUSED_DATABASES = ("avacont_sursa", "avacont_comun")


def _is_target(table):
    return table.startswith(TZ_PREFIX) or table in EF_TABLES


def _targets(conn, db):
    names = [r["name"] for r in _rows(
        conn, "SELECT TABLE_NAME AS name FROM information_schema.TABLES WHERE TABLE_SCHEMA=%s AND TABLE_TYPE='BASE TABLE' "
              "ORDER BY TABLE_NAME", (db,))]
    found = [n for n in names if _is_target(n)]
    if not any(n.startswith(TZ_PREFIX) for n in found):
        raise AdminError("Baza «%s» nu are tabele TZ_: nu este o baza cu date contabile Avacont, nu se goleste nimic." % db)
    return found


def _count(conn, table):
    return int(_rows(conn, "SELECT COUNT(*) AS n FROM %s" % _q(table))[0]["n"])


def wipe(conn, db, param, block):
    if db.casefold() in REFUSED_DATABASES:
        raise AdminError("Baza «%s» tine doar structura: golirea datelor contabile merge numai pe o baza de unitate. Nu s-a sters nimic." % db)
    typed = (param or "").strip()
    if typed.casefold() != db.casefold():
        raise AdminError("Confirmarea nu se potriveste: scrie exact numele bazei «%s» (ai scris «%s»). Nu s-a sters nimic." % (db, typed))
    targets = _targets(conn, db)
    before = {t: _count(conn, t) for t in targets}
    done = []
    try:
        # TRUNCATE is DDL (it commits by itself and resets AUTO_INCREMENT, which a new migration wants); the TZ_ tables reference each other,
        # so the checks are off for this session and back on afterwards.
        _exec(conn, "SET SESSION FOREIGN_KEY_CHECKS=0")
        for table in targets:
            if not _is_target(table):
                raise AdminError("Tabela «%s» nu tine de datele contabile: golirea se opreste." % table)
            _exec(conn, "TRUNCATE TABLE %s" % _q(table))
            done.append(table)
    except Exception as err:
        _journal_wipe(conn, db, "EROARE", "Golire date contabile oprita dupa %d din %d tabele (%s): %s" % (len(done), len(targets), ", ".join(done), err))
        raise
    finally:
        _exec(conn, "SET SESSION FOREIGN_KEY_CHECKS=1")
    left = {t: _count(conn, t) for t in targets}
    total = sum(before.values())
    problems = [t for t in targets if left[t]]
    _journal_wipe(conn, db, "OK", "%d tabele golite (%d TZ_, %d EF_), %d randuri sterse; au ramas randuri in: %s"
                  % (len(targets), sum(1 for t in targets if t.startswith(TZ_PREFIX)), sum(1 for t in targets if not t.startswith(TZ_PREFIX)), total,
                     ", ".join(problems) or "niciuna"))
    summary = block(db, TITLE, ["rezultat"],
                    [["%d tabele golite, %d randuri sterse din baza %s." % (len(targets), total, db)],
                     ["TOATE tabelele sunt goale." if not problems else "ATENTIE, au ramas randuri in: " + ", ".join(problems)]],
                    "Parteneri, Clasificatii, FX_, AD_, VC_ si restul tabelelor EF_ nu au fost atinse. Migrarea poate fi rulata din nou pe aceasta baza: "
                    "Subunitati, Plan de conturi, Jurnale, Sabloane, Operatii, Plati; importul E-Factura din Access pune la loc legaturile facturilor.", raw=True)
    detail = block(db, TITLE + " - pe tabele", ["tabela", "randuri inainte", "randuri dupa"], [[t, str(before[t]), str(left[t])] for t in targets])
    return [summary, detail]


def _journal_wipe(conn, db, result, details):
    _journal(conn, "avacont_golire", db, "TZ_ + EF_PrimiteOperatii", details, result)
    conn.commit()

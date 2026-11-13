"""VERCON, one-time cleanup for the KBotAdmin CLI (slice VC14-04): archives and deletes every document that was annulled BEFORE the
annulment started to archive-and-delete, so they leave the trees and their numbers are free.

WRITES. For each document with Anulat=1 (BU, BP, BT, BS, food lists, BR, BC, FCS, NIR -- the children before the parents) it calls
the same `routes.vercon.archive.purge` the cancel routes use: the document is copied as JSON into VC_ArhivaAnulate, its rows are
deleted from the VC_ tables and the numbering counter is lowered to the highest number still saved. One document = one commit; a
document that cannot be archived is rolled back, listed in the result and left as it was, and the rest goes on.
The operator must type the database name as the confirmation (checked here, not only in the client). Needs the table of
sql/VC14_04_ARHIVA_ANULATE.sql.
"""
from routes.admin_cli.admin_common import AdminError, _journal, _rows
from routes.vercon import archive

TITLE = "Arhivare documente anulate VERCON"
ACTOR = "KBotAdmin"

# (family, SQL that lists the annulled documents of every scope: SubunitId, GestiuneId, ExercitiuId, doc), in the order they are archived.
_ORDER = (
    ("bp", "SELECT DISTINCT SubunitId, GestiuneId, ExercitiuId, IDBP AS doc FROM VC_OI_Predare WHERE Anulat=1 AND IDBP IS NOT NULL"),
    ("bt", "SELECT DISTINCT SubunitId, GestiuneId, ExercitiuId, IDBT AS doc FROM VC_OI_Transfer WHERE Anulat=1 AND IDBT IS NOT NULL"),
    ("bs", "SELECT DISTINCT SubunitId, GestiuneId, ExercitiuId, IDBC AS doc FROM VC_OI_Casare WHERE Anulat=1 AND IDBC IS NOT NULL"),
    ("lista", "SELECT SubunitId, GestiuneId, ExercitiuId, IdLista AS doc FROM VC_LABC WHERE Anulat=1"),
    ("br", "SELECT SubunitId, GestiuneId, ExercitiuId, IdBonConsum AS doc FROM VC_Iesiri_H WHERE Anulat=1 AND COALESCE(Retur,0)=1"),
    ("bc", "SELECT SubunitId, GestiuneId, ExercitiuId, IdBonConsum AS doc FROM VC_Iesiri_H WHERE Anulat=1 AND COALESCE(Retur,0)=0"),
    ("storno", "SELECT SubunitId, GestiuneId, ExercitiuId, IdIntrare AS doc FROM VC_NIR WHERE Anulat=1 AND COALESCE(Storno,0)=1"),
    ("nir", "SELECT SubunitId, GestiuneId, ExercitiuId, IdIntrare AS doc FROM VC_NIR WHERE Anulat=1 AND COALESCE(Storno,0)=0"),
)
_LABEL = {"bu": "BU", "bp": "BP", "bt": "BT", "bs": "BS", "lista": "Lista alimente", "br": "BR (retur)", "bc": "BC", "storno": "FCS", "nir": "NIR"}


def _context(types, scope):
    return {"subunit": scope[0], "gestiune": scope[1], "exercitiu": scope[2], "type": types.get((scope[0], scope[1])), "email": ACTOR}


def _still_there(cursor, family, scope, doc):
    """A dependent purged earlier in the same run (a storno, a return, a BU component) is already gone."""
    ctx = _context({}, scope)
    steps, _, head = archive._plan(cursor, ctx, family, doc)
    return bool(head)


def wipe_one(conn, cursor, family, scope, doc, types, done, skipped):
    try:
        if not _still_there(cursor, family, scope, doc):
            return
        archive.purge(cursor, _context(types, scope), family, doc, None, ACTOR)
        conn.commit()
        done[family] = done.get(family, 0) + 1
    except Exception as err:   # one document must not stop the run: it is rolled back and listed in the result
        conn.rollback()
        skipped.append([_LABEL[family], "%s/%s/%s" % scope, str(doc), "%s: %s" % (type(err).__name__, err)])


def run(conn, db, param, block):
    typed = (param or "").strip()
    if typed.casefold() != db.casefold():
        raise AdminError("Confirmarea nu se potriveste: scrie exact numele bazei «%s» (ai scris «%s»). Nu s-a arhivat nimic." % (db, typed))
    cursor = conn.cursor(dictionary=True)
    try:
        if not archive.ready(cursor):
            raise AdminError("Baza «%s» nu are tabela VC_ArhivaAnulate. Aplica mai intai sql/VC14_04_ARHIVA_ANULATE.sql." % db)
        types = {(r["SubunitId"], r["GestiuneId"]): r["Type"] for r in _rows(conn, "SELECT SubunitId, GestiuneId, Type FROM VC_Gestiuni")}
        done, skipped = {}, []
        # a BU first: its receipt, handover, bons and transfer go with it
        if _rows(conn, "SELECT 1 AS x FROM information_schema.TABLES WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='VC_TransferUnitati'"):
            for link in _rows(conn, "SELECT * FROM VC_TransferUnitati WHERE ANULAT=1 ORDER BY ID"):
                source = _context(types, (link["SURSA_SUBUNITATE"], link["SURSA_GESTIUNE"], link["SURSA_EXERCITIU"]))
                target = _context(types, (link["DEST_SUBUNITATE"], link["DEST_GESTIUNE"], link["DEST_EXERCITIU"]))
                try:
                    archive.purge_bu(cursor, source, target, link, None, ACTOR)
                    conn.commit()
                    done["bu"] = done.get("bu", 0) + 1
                except Exception as err:   # same rule as above
                    conn.rollback()
                    skipped.append([_LABEL["bu"], "%s/%s/%s" % (source["subunit"], source["gestiune"], source["exercitiu"]), str(link["ID"]), "%s: %s" % (type(err).__name__, err)])
        for family, sql in _ORDER:
            for row in _rows(conn, sql + " ORDER BY 1, 2, 3, 4"):
                wipe_one(conn, cursor, family, (row["SubunitId"], row["GestiuneId"], row["ExercitiuId"]), row["doc"], types, done, skipped)
    finally:
        cursor.close()
    total = sum(done.values())
    _journal(conn, "vercon_arhiva_anulate", db, "VC_ArhivaAnulate",
             "%d documente anulate arhivate si sterse (%s); %d sarite" % (total, ", ".join("%s %d" % (_LABEL[f], n) for f, n in done.items()) or "niciunul", len(skipped)),
             "OK" if not skipped else "PARTIAL")
    conn.commit()
    summary = block(db, TITLE, ["familie", "arhivate si sterse"], [[_LABEL[f], str(n)] for f, n in done.items()] or [["-", "0"]],
                    "%d documente arhivate in VC_ArhivaAnulate si sterse din tabelele VC_. Numerele lor sunt libere." % total)
    if not skipped:
        return [summary]
    return [summary, block(db, TITLE + " - sarite (au ramas anulate, neatinse)", ["familie", "context", "id", "motiv"], skipped)]

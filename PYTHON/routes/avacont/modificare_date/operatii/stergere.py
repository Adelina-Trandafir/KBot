# routes/avacont/modificare_date/operatii/stergere.py
"""
«Modificare operatii» (slice AV02-05; the Access buttons «Sterge operatie» / «Sterge Document» of ModificareNC1280): deleting a posted operation or
a whole document.

    GET    /api/avacont/modificare-operatii/operatie/<idoper>/verifica
    GET    /api/avacont/modificare-operatii/document/<iddocument>/verifica
        -> 200 { "refuz": text | null, "intrebare": text, "facturi": [ {numar, data, platit} ] }
           refuz = why it cannot be deleted (nothing to ask then); intrebare = the confirmation the operator answers;
           facturi = the invoices a payment paid, which become unpaid (the Access message 10350).
    DELETE /api/avacont/modificare-operatii/operatie/<idoper>
    DELETE /api/avacont/modificare-operatii/document/<iddocument>
        -> 200 { "operatii": n }   409 with the reason when a check fails (the same checks as /verifica, read again under lock).

Rules (citire.motiv_refuz): closing documents (Fel I, number CCV...), a closed month or an earlier closed year, an ordonantare generated, an evidence
already paid, an order of payment in an OPME or confirmed by a bank statement: REFUSED. A document is deleted only when EVERY operation of it passes.
What goes: a payment with an order through plati.sterge_plata (allocations, order, lines, operation); any other operation: its allocations as a payment,
its link to the received invoice (EF_PrimiteOperatii, so the invoice can be booked again), its debit / credit lines, the operation; then the document
and the note when they stay empty. Access deleted the whole document of an E/P operation even when it had other operations: here only an empty one goes.
"""
import logging
from datetime import date, datetime

from flask import g

from routes.auth.guard import require_session
from utils.database import get_kbot_connection

from ... import avacont_bp
from ...common import json_utf8, rollback
from ...introducere_date.operatii import plati
from . import citire

logger = logging.getLogger(__name__)


class StergereEroare(Exception):
    """A refusal the operator can act on."""

    def __init__(self, message, status=409):
        super().__init__(message)
        self.status = status


def _iso(valoare):
    if isinstance(valoare, (date, datetime)):
        return valoare.strftime("%Y-%m-%d")
    return None if valoare is None else str(valoare)[:10]


def _ro(valoare):
    return valoare.strftime("%d.%m.%Y") if isinstance(valoare, (date, datetime)) else ""


def _bani(valoare):
    return "{:,.2f}".format(float(valoare or 0)).replace(",", " ").replace(".", ",")


def _operatii(cursor, where, params, blocheaza):
    """The rows of the operations to delete; with `blocheaza` the operations are locked first (FOR UPDATE), then read."""
    if blocheaza:
        cursor.execute("SELECT o.IdOper FROM TZ_Oper o WHERE " + where + " FOR UPDATE", params)
        cursor.fetchall()
    rows = citire.operatii(cursor, where, params)
    if not rows:
        raise StergereEroare("Operația nu mai există. Reîncărcați lista.", 404)
    return rows


def _verifica(cursor, rows):
    """(refuz, facturi): the first reason that stops the delete (with the operation named when there are several), and the invoices the payments paid."""
    inchid = citire.inchideri(cursor, {(r["IdSubunitate"], r["An"]) for r in rows})
    for r in rows:
        motiv = citire.motiv_refuz(r, inchid.get((r["IdSubunitate"], r["An"])))
        if motiv:
            if len(rows) > 1:
                motiv = "Operația «%s» (%s): %s" % (r["Explicatie"] or "", _bani(r["SumaDebit"]), motiv)
            return motiv, []
    facturi = []
    for r in rows:
        if r["FacturiPlatite"]:
            cursor.execute("SELECT NumarDocument, DataDocument, Platit FROM TZ_PlatiFacturi WHERE IdPlata = %s ORDER BY DataDocument, NumarDocument", (r["IdOper"],))
            facturi += [{"numar": f["NumarDocument"] or "", "data": _iso(f["DataDocument"]), "platit": float(f["Platit"] or 0)} for f in cursor.fetchall()]
    return None, facturi


def _intrebare(rows, facturi, document):
    r = rows[0]
    if document:
        text = ("Doriți ștergerea documentului %s din %s (nota %s), cu toate cele %d operații ale lui?"
                % (r["NumarDocument"] or "", _ro(r["DataDocument"]), r["NumarNC"], len(rows)))
    else:
        text = ("Doriți ștergerea operației selectate?\n\nDocument: %s din %s\nDebit: %s\nCredit: %s\nSuma: %s"
                % (r["NumarDocument"] or "", _ro(r["DataDocument"]), citire.cont_text(r["ContDebit"], r["PartDebit"], r["LiniiDebit"]),
                   citire.cont_text(r["ContCredit"], r["PartCredit"], r["LiniiCredit"]), _bani(r["SumaDebit"])))
    if facturi:
        text += ("\n\nPlata a achitat facturile de mai jos. După ștergere ele devin neplătite:\n"
                 + "\n".join("%s din %s" % (f["numar"], _ro(datetime.strptime(f["data"], "%Y-%m-%d")) if f["data"] else "") for f in facturi))
    return text


def _sterge_operatie(cursor, r):
    """Deletes one operation that passed the checks; the document / note are cleaned by the caller (or by sterge_plata)."""
    if r["IdOP"] is not None:
        plati.sterge_plata(cursor, r["IdOP"])
        return
    id_oper = r["IdOper"]
    cursor.execute("DELETE FROM TZ_PlatiFacturi WHERE IdPlata = %s", (id_oper,))
    cursor.execute("UPDATE TZ_PlatiFacturi SET IdEvidentiereAnterior = NULL WHERE IdEvidentiereAnterior = %s", (id_oper,))
    cursor.execute("DELETE FROM EF_PrimiteOperatii WHERE IdOperatie = %s", (id_oper,))
    cursor.execute("DELETE FROM TZ_Debit WHERE IdOper = %s", (id_oper,))
    cursor.execute("DELETE FROM TZ_Credit WHERE IdOper = %s", (id_oper,))
    cursor.execute("DELETE FROM TZ_Oper WHERE IdOper = %s", (id_oper,))


def _curata(cursor, id_document, id_nota):
    cursor.execute("DELETE FROM TZ_Documente WHERE IdDocument = %s AND NOT EXISTS (SELECT 1 FROM TZ_Oper WHERE IdDocument = %s)", (id_document, id_document))
    cursor.execute("DELETE FROM TZ_NoteC WHERE IdNota = %s AND NOT EXISTS (SELECT 1 FROM TZ_Documente WHERE IdNota = %s)", (id_nota, id_nota))


def _raspuns_verifica(where, params, document, eticheta):
    db_name = g.session.db_name
    conn = None
    try:
        conn = get_kbot_connection(db_name)
        cursor = conn.cursor(dictionary=True)
        rows = _operatii(cursor, where, params, blocheaza=False)
        refuz, facturi = _verifica(cursor, rows)
        return json_utf8({"refuz": refuz, "intrebare": "" if refuz else _intrebare(rows, facturi, document), "facturi": facturi}, 200)
    except StergereEroare as err:
        return json_utf8({"error": str(err)}, err.status)
    except Exception as err:
        logger.error("[avacont.modificare_operatii] verifica %s %s: %s", eticheta, db_name, err, exc_info=True)
        return json_utf8({"error": "Eroare la verificarea ștergerii: %s" % err}, 500)
    finally:
        if conn is not None:
            conn.close()


def _raspuns_sterge(where, params, eticheta):
    db_name = g.session.db_name
    conn = None
    try:
        conn = get_kbot_connection(db_name)
        conn.autocommit = False
        if not conn.in_transaction:
            conn.start_transaction()
        cursor = conn.cursor(dictionary=True)
        rows = _operatii(cursor, where, params, blocheaza=True)
        refuz, _ = _verifica(cursor, rows)
        if refuz:
            raise StergereEroare(refuz)
        for r in rows:
            _sterge_operatie(cursor, r)
        for id_document, id_nota in {(r["IdDocument"], r["IdNota"]) for r in rows}:
            _curata(cursor, id_document, id_nota)
        conn.commit()
        for r in rows:
            logger.info("[avacont.modificare_operatii] %s %s by %s: IdOper=%s NC=%s doc=%s/%s %s / %s suma=%s",
                        eticheta, db_name, g.session.username, r["IdOper"], r["NumarNC"], r["NumarDocument"], _iso(r["DataDocument"]),
                        r["ContDebit"], r["ContCredit"], r["SumaDebit"])
        return json_utf8({"operatii": len(rows)}, 200)
    except StergereEroare as err:
        rollback(conn)
        return json_utf8({"error": str(err)}, err.status)
    except plati.PlatiEroare as err:
        rollback(conn)
        return json_utf8({"error": str(err)}, err.status)
    except Exception as err:
        rollback(conn)
        logger.error("[avacont.modificare_operatii] sterge %s %s: %s", eticheta, db_name, err, exc_info=True)
        return json_utf8({"error": "Eroare la ștergere: %s" % err}, 500)
    finally:
        if conn is not None:
            conn.close()


@avacont_bp.route("/api/avacont/modificare-operatii/operatie/<int:id_oper>/verifica", methods=["GET"])
@require_session
def get_modificare_operatii_verifica_operatie(id_oper):
    return _raspuns_verifica("o.IdOper = %s", (id_oper,), False, "operatie")


@avacont_bp.route("/api/avacont/modificare-operatii/document/<int:id_document>/verifica", methods=["GET"])
@require_session
def get_modificare_operatii_verifica_document(id_document):
    return _raspuns_verifica("o.IdDocument = %s", (id_document,), True, "document")


@avacont_bp.route("/api/avacont/modificare-operatii/operatie/<int:id_oper>", methods=["DELETE"])
@require_session
def delete_modificare_operatii_operatie(id_oper):
    return _raspuns_sterge("o.IdOper = %s", (id_oper,), "operatie")


@avacont_bp.route("/api/avacont/modificare-operatii/document/<int:id_document>", methods=["DELETE"])
@require_session
def delete_modificare_operatii_document(id_document):
    return _raspuns_sterge("o.IdDocument = %s", (id_document,), "document")

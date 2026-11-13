# routes/avacont/modificare_date/operatii/lista.py
"""
«Modificare operatii» (slice AV02-05; the Access form ModificareNC1280): the list of the posted operations of a subunit and year, and the info panel.

    GET /api/avacont/modificare-operatii/lista?idsubunitate=<n>&an=<yyyy>
        -> 200 { "operatii": [ { idoper, iddocument, numarnc, numardocument, datadocument, dataoperatie, clsf, feldocument, fel, jurnal, explicatie,
                                 debit, credit, suma, chelt, cfpp, ord, receptie, refuz } ], "inchidere": "YYYY-MM-DD" | null }
        ord: '' / 'G' / 'AB' / 'CD' / 'O' (stage of the ordonantare); receptie: '' / 'T' (all invoices have a reception) / 'P' (some);
        refuz: null when the operation can be deleted, else the reason (the client dims the delete buttons and shows it as a tip).
    GET /api/avacont/modificare-operatii/info/<idoper>
        -> 200 { fel, factura: 'INTEGRAL' | 'PARTIAL' | '', op: {numar, data} | null, facturi: [ {numar, data, platit} ],
                 partener: {cod, denumire} | null, datadocument, sold_la_data: {debit, credit}, sold_anual: {debit, credit} }
        The partner's balance counts the opening balance of the year (TZ_SoldInitial: balance + turnover before the opening, accounts of the
        subunit) and the operations of the subunit and year (up to the date of the document for «sold_la_data»), as Access qTSD / qTSC did.

Read only: nothing here writes.
"""
import logging
from datetime import date, datetime
from decimal import Decimal

from flask import g, request

from routes.auth.guard import require_session
from utils.database import get_kbot_connection

from ... import avacont_bp
from ...common import json_utf8
from ...introducere_date.operatii import plati, postare
from . import citire

logger = logging.getLogger(__name__)


class ListaEroare(Exception):
    """A refusal the operator can act on."""

    def __init__(self, message, status=400):
        super().__init__(message)
        self.status = status


def _intreg(valoare, nume):
    if valoare is None or valoare == "" or isinstance(valoare, bool):
        raise ListaEroare("Câmpul «%s» lipsește sau nu este valid." % nume)
    try:
        return int(valoare)
    except (TypeError, ValueError) as err:
        raise ListaEroare("Câmpul «%s» nu este valid." % nume) from err


def _iso(valoare):
    if isinstance(valoare, (date, datetime)):
        return valoare.strftime("%Y-%m-%d")
    return None if valoare is None else str(valoare)[:10]


def _bani(valoare):
    return float(valoare) if valoare is not None else 0.0


def _suma(row):
    """The amount of the operation: the debit total (= the credit total, the posting service checks the balance); one side only when the other is missing."""
    if row["SumaDebit"] is not None:
        return _bani(row["SumaDebit"])
    return _bani(row["SumaCredit"])


def randuri_lista(cursor, where, params):
    """The list rows (dicts with the JSON keys) for a filter on TZ_Oper o / TZ_Documente d / TZ_NoteC n."""
    rows = citire.operatii(cursor, where, params)
    facturi = citire.facturi_ale_operatiilor(cursor, where, params)
    cu_receptie = plati.primite_cu_receptie(cursor, {f for s in facturi.values() for f in s})
    inchid = citire.inchideri(cursor, {(r["IdSubunitate"], r["An"]) for r in rows})
    azi = date.today()
    out = []
    for r in rows:
        out.append({
            "idoper": r["IdOper"], "iddocument": r["IdDocument"], "numarnc": r["NumarNC"], "numardocument": r["NumarDocument"] or "",
            "datadocument": _iso(r["DataDocument"]), "dataoperatie": _iso(r["DataOperatie"]), "clsf": r["Clsf"] or "",
            "feldocument": r["FelDocument"] or "", "fel": (r["FelDoc"] or "").upper(), "jurnal": r["Jurnal"] or "", "explicatie": r["Explicatie"] or "",
            "debit": citire.cont_text(r["ContDebit"], r["PartDebit"], r["LiniiDebit"]),
            "credit": citire.cont_text(r["ContCredit"], r["PartCredit"], r["LiniiCredit"]),
            "suma": _suma(r), "chelt": bool(r["Chelt"]), "cfpp": bool(r["Cfpp"]),
            "ord": citire.stare_ord(r), "receptie": citire.stare_receptie(facturi.get(int(r["IdOper"])), cu_receptie),
            "refuz": citire.motiv_refuz(r, inchid.get((r["IdSubunitate"], r["An"])), azi)})
    return out


@avacont_bp.route("/api/avacont/modificare-operatii/lista", methods=["GET"])
@require_session
def get_modificare_operatii_lista():
    """All the operations of the subunit and year, one row each."""
    db_name = g.session.db_name
    conn = None
    try:
        id_subunitate = _intreg(request.args.get("idsubunitate"), "idsubunitate")
        an = _intreg(request.args.get("an"), "an")
        conn = get_kbot_connection(db_name)
        cursor = conn.cursor(dictionary=True)
        randuri = randuri_lista(cursor, "n.IdSubunitate = %s AND n.An = %s", (id_subunitate, an))
        inchidere = postare.data_ultimei_inchideri(cursor, id_subunitate, an)
        return json_utf8({"operatii": randuri, "inchidere": _iso(inchidere)}, 200)
    except ListaEroare as err:
        return json_utf8({"error": str(err)}, err.status)
    except Exception as err:
        logger.error("[avacont.modificare_operatii] lista %s: %s", db_name, err, exc_info=True)
        return json_utf8({"error": "Eroare la citirea operațiilor: %s" % err}, 500)
    finally:
        if conn is not None:
            conn.close()


def _sold_partener(cursor, id_partener, id_subunitate, an, pana_la):
    """{debit, credit}: the opening balance of the partner on the accounts of the subunit + its operations of the subunit and year (up to `pana_la` when given)."""
    cursor.execute("SELECT COALESCE(SUM(si.SoldDebitor + si.RulajDebitor), 0) AS D, COALESCE(SUM(si.SoldCreditor + si.RulajCreditor), 0) AS C "
                   "FROM TZ_SoldInitial si JOIN TZ_PlanCont pc ON pc.IdCont = si.IdCont "
                   "WHERE si.IdPartener = %s AND si.An = %s AND pc.IdSubunitate = %s", (id_partener, an, id_subunitate))
    si = cursor.fetchone()
    debit, credit = Decimal(si["D"]), Decimal(si["C"])
    conditie = "" if pana_la is None else " AND o.DataOperatie <= %s"
    for tabel in ("TZ_Debit", "TZ_Credit"):
        params = (id_partener, id_subunitate, an) + (() if pana_la is None else (pana_la,))
        cursor.execute("SELECT COALESCE(SUM(x.Suma), 0) AS S FROM " + tabel + " x JOIN TZ_Oper o ON o.IdOper = x.IdOper "
                       "JOIN TZ_Documente d ON d.IdDocument = o.IdDocument JOIN TZ_NoteC n ON n.IdNota = d.IdNota "
                       "WHERE x.IdPartener = %s AND n.IdSubunitate = %s AND n.An = %s" + conditie, params)
        suma = Decimal(cursor.fetchone()["S"])
        if tabel == "TZ_Debit":
            debit += suma
        else:
            credit += suma
    return {"debit": float(debit), "credit": float(credit)}


@avacont_bp.route("/api/avacont/modificare-operatii/info/<int:id_oper>", methods=["GET"])
@require_session
def get_modificare_operatii_info(id_oper):
    """The info panel of one operation (the Access popup MNotaC11)."""
    db_name = g.session.db_name
    conn = None
    try:
        conn = get_kbot_connection(db_name)
        cursor = conn.cursor(dictionary=True)
        rows = citire.operatii(cursor, "o.IdOper = %s", (id_oper,))
        if not rows:
            raise ListaEroare("Operația nu mai există. Reîncărcați lista.", 404)
        r = rows[0]
        fel = (r["FelDoc"] or "").upper()

        factura = ""
        cursor.execute("SELECT COALESCE(SUM(Platit), 0) AS P, COUNT(*) AS N FROM TZ_PlatiFacturi WHERE IdEvidentiere = %s", (id_oper,))
        pe = cursor.fetchone()
        if int(pe["N"]) > 0:
            factura = "INTEGRAL" if Decimal(pe["P"]) >= Decimal(str(_suma(r))) else "PARTIAL"

        op = None if r["IdOP"] is None else {"numar": r["NrOP"], "data": _iso(r["DataOP"])}
        cursor.execute("SELECT NumarDocument, DataDocument, Platit FROM TZ_PlatiFacturi WHERE IdPlata = %s ORDER BY DataDocument, NumarDocument", (id_oper,))
        facturi = [{"numar": f["NumarDocument"] or "", "data": _iso(f["DataDocument"]), "platit": _bani(f["Platit"])} for f in cursor.fetchall()]

        id_partener = r["IdPartDebit"] if r["IdPartDebit"] is not None else r["IdPartCredit"]
        partener, sold_data, sold_an = None, None, None
        if id_partener is not None:
            cursor.execute("SELECT CodPartener, DenumirePartener FROM Parteneri WHERE IdPartener = %s", (id_partener,))
            p = cursor.fetchone()
            if p is not None:
                partener = {"cod": p["CodPartener"] or "", "denumire": p["DenumirePartener"] or ""}
            sold_data = _sold_partener(cursor, id_partener, r["IdSubunitate"], r["An"], r["DataDocument"])
            sold_an = _sold_partener(cursor, id_partener, r["IdSubunitate"], r["An"], None)

        return json_utf8({"fel": fel, "factura": factura, "op": op, "facturi": facturi, "partener": partener,
                          "datadocument": _iso(r["DataDocument"]), "sold_la_data": sold_data, "sold_anual": sold_an}, 200)
    except ListaEroare as err:
        return json_utf8({"error": str(err)}, err.status)
    except Exception as err:
        logger.error("[avacont.modificare_operatii] info %s/%s: %s", db_name, id_oper, err, exc_info=True)
        return json_utf8({"error": "Eroare la citirea informațiilor operației: %s" % err}, 500)
    finally:
        if conn is not None:
            conn.close()

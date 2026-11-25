# routes/avacont/introducere_date/operatii/op9.py
"""
«Operatii diverse» (slice AV02-03; the Access form OP9, routine Sav_Mat2015): the operations of type A posted from a template.

The operator chooses a template of type A, writes the header (note, document number, kind and date, the partner where the template needs one, the amount) and
the amount of each line (it starts as the amount of the header and can be changed line by line). Every line with an amount other than zero becomes ONE document
(Fel = A) and ONE operation (Tip = A) with one debit and one credit line, all in ONE transaction, through NoteC (NoteC_diverse, SLICE-AV08-04).

    GET  /api/avacont/operatii-diverse/context?idsubunitate=<n>&an=<yyyy>
        -> 200 { "subunitate": {idsubunitate, denumire, sursasector}, "note": [{numarnc, datanc, documente}], "urmatoare": n, "inchidere": "YYYY-MM-DD" | null,
           "parteneri": [{idpartener, cod, denumire}] (only the unit of the sector+source of the subunit and the year) }
    POST /api/avacont/operatii-diverse/salveaza
        { idsubunitate, an, idsablon, numarnc, numardocument, feldocument, datadocument, idpartener | null, linii: [ { idlinie, suma } ] }
        -> 200 { idnota, numarnc, an, operatii: [ { idlinie, iddocument, idoper } ] }   (SLICE-AV08-03: no nrang)

Rules (Access, kept): the date is in the work year and after the last closing (CCV); a line needs its accounts in the chart of the subunit; the partner is
required when a side of a line says «the partner of the operation»; no line with an amount = nothing to post. The accounts are read from the template on the
server (the client sends only the line ids and the amounts). The FX/ALG part of Access (commitment contract, account 8067) is left out on purpose.
"""
import logging
from datetime import date, datetime
from decimal import Decimal, InvalidOperation

from flask import g, request

from routes.auth.guard import require_session
from utils.database import get_kbot_connection

from ... import avacont_bp
from ...common import json_utf8
from . import NoteC

logger = logging.getLogger(__name__)

CENT = Decimal("0.01")


class OpEroare(Exception):
    """A refusal the operator can act on."""

    def __init__(self, message, status=400):
        super().__init__(message)
        self.status = status


def _intreg(valoare, nume, obligatoriu=True):
    if valoare is None or valoare == "":
        if obligatoriu:
            raise OpEroare("Câmpul «%s» lipsește." % nume)
        return None
    if type(valoare) is not int and not (isinstance(valoare, str) and valoare.isascii() and valoare.lstrip("-").isdigit()):
        raise OpEroare("Câmpul «%s» nu este valid." % nume)
    try:
        n = int(valoare)
        if not -2147483648 <= n <= 2147483647:
            raise ValueError(n)
        return n
    except (TypeError, ValueError) as err:
        raise OpEroare("Câmpul «%s» nu este valid." % nume) from err


def _data(valoare, nume):
    try:
        return datetime.strptime(str(valoare)[:10], "%Y-%m-%d").date()
    except (TypeError, ValueError) as err:
        raise OpEroare("Câmpul «%s» nu este o dată validă." % nume) from err


def _suma(valoare):
    try:
        suma = Decimal(str(valoare))
        if not suma.is_finite() or abs(suma) >= Decimal("10000000000000000"):
            raise InvalidOperation
        return suma.quantize(CENT)
    except (InvalidOperation, ValueError) as err:
        raise OpEroare("O sumă nu este validă.") from err


def _iso(valoare):
    if isinstance(valoare, (date, datetime)):
        return valoare.strftime("%Y-%m-%d")
    return None if valoare is None else str(valoare)[:10]


@avacont_bp.route("/api/avacont/operatii-diverse/context", methods=["GET"])
@require_session
def get_operatii_diverse_context():
    """The notes of the subunit and year, the next note number and the date of the last closing."""
    db_name = g.session.db_name
    conn = None
    try:
        id_subunitate = _intreg(request.args.get("idsubunitate"), "idsubunitate")
        an = _intreg(request.args.get("an"), "an")
        conn = get_kbot_connection(db_name)
        cursor = conn.cursor(dictionary=True)
        sub = NoteC.subunitate_pentru(cursor, None, id_subunitate)
        note, urmatoare = NoteC.note_ale_subunitatii(cursor, id_subunitate, an)
        inchidere = NoteC.data_ultimei_inchideri(cursor, id_subunitate, an)
        # The partner of the operation: only the ones of the unit of this sector+source and year (each partner is kept once per unit).
        cursor.execute("SELECT IdPartener, CodPartener, DenumirePartener FROM Parteneri WHERE COALESCE(Ascuns, 0) = 0 AND IdUnitate IN (" + NoteC.SQL_UNITATI_SURSEI + ") "
                       "ORDER BY DenumirePartener, IdPartener", (sub["SursaSector"], an))
        parteneri = [{"idpartener": r["IdPartener"], "cod": r["CodPartener"] or "", "denumire": r["DenumirePartener"] or ""} for r in cursor.fetchall()]
        return json_utf8({"subunitate": {"idsubunitate": sub["IdSubunitate"], "denumire": sub["Denumire"], "sursasector": sub["SursaSector"]},
                          "note": [{"idnota": n["IdNota"], "numarnc": n["NumarNC"], "datanc": _iso(n["DataNc"]), "documente": int(n["Documente"])} for n in note],
                          "urmatoare": urmatoare, "inchidere": _iso(inchidere), "parteneri": parteneri}, 200)
    except OpEroare as err:
        return json_utf8({"error": str(err)}, err.status)
    except NoteC.NoteCEroare as err:
        return json_utf8({"error": str(err)}, err.status)
    except Exception as err:
        logger.error("[avacont.op9] context %s: %s", db_name, err, exc_info=True)
        return json_utf8({"error": "Eroare la citirea datelor pentru operații diverse: %s" % err}, 500)
    finally:
        if conn is not None:
            conn.close()


def _citeste_cerere(corp):
    if not isinstance(corp, dict):
        raise OpEroare("Corp JSON lipsă sau nevalid.")
    cerere = {
        "idsubunitate": _intreg(corp.get("idsubunitate"), "idsubunitate"),
        "an": _intreg(corp.get("an"), "an"),
        "idsablon": _intreg(corp.get("idsablon"), "idsablon"),
        "numarnc": _intreg(corp.get("numarnc"), "numarnc"),
        "idnota": _intreg(corp.get("idnota"), "idnota", obligatoriu=False),
        "nota_noua": corp.get("nota_noua"),
        "idpartener": _intreg(corp.get("idpartener"), "idpartener", obligatoriu=False),
        "numardocument": str(corp.get("numardocument") or "").strip(),
        "feldocument": str(corp.get("feldocument") or "").strip(),
        "datadocument": _data(corp.get("datadocument"), "datadocument"),
    }
    if cerere["numarnc"] <= 0:
        raise OpEroare("Numărul notei contabile trebuie să fie pozitiv.")
    if not cerere["numardocument"] or len(cerere["numardocument"]) > 50:
        raise OpEroare("Numărul documentului trebuie să aibă între 1 și 50 de caractere.")
    if not cerere["feldocument"] or len(cerere["feldocument"]) > 50:
        raise OpEroare("Felul documentului trebuie să aibă între 1 și 50 de caractere.")
    linii = corp.get("linii")
    if not isinstance(linii, list):
        raise OpEroare("Câmpul «linii» nu este valid.")
    sume = {}
    for l in linii:
        if not isinstance(l, dict):
            raise OpEroare("O linie nu este validă.")
        id_linie = _intreg(l.get("idlinie"), "idlinie")
        if id_linie in sume:
            raise OpEroare("Aceeași linie de șablon apare de două ori.")
        sume[id_linie] = _suma(l.get("suma"))
    sume = {i: s for i, s in sume.items() if s != 0}
    if not sume:
        raise OpEroare("Nicio linie nu are sumă: nu este nimic de înregistrat.")
    cerere["sume"] = sume
    return cerere


@avacont_bp.route("/api/avacont/operatii-diverse/salveaza", methods=["POST"])
@require_session
def post_operatii_diverse_salveaza():
    """Posts the lines with an amount: a document and an operation each, in one transaction."""
    db_name = g.session.db_name
    user = g.session.username
    try:
        cerere = _citeste_cerere(request.get_json(silent=True))
        with NoteC.salvare(db_name) as conn:
            id_nota, rezultat = NoteC.salveaza_operatii_diverse(conn.cursor(dictionary=True), user, cerere)
        an = cerere["an"]
        logger.info("[avacont.op9] %s: nota %s, %s operatii", db_name, cerere["numarnc"], len(rezultat))
        return json_utf8({"idnota": id_nota, "numarnc": cerere["numarnc"], "an": an, "operatii": rezultat}, 200)
    except OpEroare as err:
        return json_utf8({"error": str(err)}, err.status)
    except NoteC.NoteCEroare as err:
        return json_utf8({"error": str(err)}, err.status)
    except Exception as err:
        logger.error("[avacont.op9] salvare %s: %s", db_name, err, exc_info=True)
        return json_utf8({"error": "Eroare la salvarea operațiilor diverse: %s" % err}, 500)

# routes/avacont/introducere_date/operatii/op9.py
"""
«Operatii diverse» (slice AV02-03; the Access form OP9, routine Sav_Mat2015): the operations of type A posted from a template.

The operator chooses a template of type A, writes the header (note, document number, kind and date, the partner where the template needs one, the amount) and
the amount of each line (it starts as the amount of the header and can be changed line by line). Every line with an amount other than zero becomes ONE document
(Fel = A) and ONE operation (Tip = A) with one debit and one credit line, all in ONE transaction and through the posting service (postare.py).

    GET  /api/avacont/operatii-diverse/context?idsubunitate=<n>&an=<yyyy>
        -> 200 { "subunitate": {idsubunitate, denumire, sursasector}, "note": [{numarnc, datanc, documente}], "urmatoare": n, "inchidere": "YYYY-MM-DD" | null }
    POST /api/avacont/operatii-diverse/salveaza
        { idsubunitate, an, idsablon, numarnc, numardocument, feldocument, datadocument, idpartener | null, linii: [ { idlinie, suma } ] }
        -> 200 { idnota, numarnc, an, operatii: [ { idlinie, iddocument, idoper, nrang } ] }

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
from ...common import json_utf8, rollback
from . import postare

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
    if isinstance(valoare, bool):
        raise OpEroare("Câmpul «%s» nu este valid." % nume)
    try:
        return int(valoare)
    except (TypeError, ValueError) as err:
        raise OpEroare("Câmpul «%s» nu este valid." % nume) from err


def _data(valoare, nume):
    try:
        return datetime.strptime(str(valoare)[:10], "%Y-%m-%d").date()
    except (TypeError, ValueError) as err:
        raise OpEroare("Câmpul «%s» nu este o dată validă." % nume) from err


def _suma(valoare):
    try:
        return Decimal(str(valoare)).quantize(CENT)
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
        sub = postare.subunitate_pentru(cursor, None, id_subunitate)
        note, urmatoare = postare.note_ale_subunitatii(cursor, id_subunitate, an)
        inchidere = postare.data_ultimei_inchideri(cursor, id_subunitate, an)
        return json_utf8({"subunitate": {"idsubunitate": sub["IdSubunitate"], "denumire": sub["Denumire"], "sursasector": sub["SursaSector"]},
                          "note": [{"numarnc": n["NumarNC"], "datanc": _iso(n["DataNc"]), "documente": int(n["Documente"])} for n in note],
                          "urmatoare": urmatoare, "inchidere": _iso(inchidere)}, 200)
    except OpEroare as err:
        return json_utf8({"error": str(err)}, err.status)
    except postare.PostareEroare as err:
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
        sume[_intreg(l.get("idlinie"), "idlinie")] = _suma(l.get("suma"))
    sume = {i: s for i, s in sume.items() if s != 0}
    if not sume:
        raise OpEroare("Nicio linie nu are sumă: nu este nimic de înregistrat.")
    cerere["sume"] = sume
    return cerere


def _id_partener_linie(mod, id_fix, id_operatie):
    if mod == "OPERATIE":
        return id_operatie
    return id_fix if mod == "FIX" else None


@avacont_bp.route("/api/avacont/operatii-diverse/salveaza", methods=["POST"])
@require_session
def post_operatii_diverse_salveaza():
    """Posts the lines with an amount: a document and an operation each, in one transaction."""
    db_name = g.session.db_name
    user = g.session.username
    conn = None
    try:
        cerere = _citeste_cerere(request.get_json(silent=True))
        conn = get_kbot_connection(db_name)
        conn.autocommit = False
        if not conn.in_transaction:
            conn.start_transaction()
        cursor = conn.cursor(dictionary=True)
        sid, an = cerere["idsubunitate"], cerere["an"]
        sub = postare.subunitate_pentru(cursor, None, sid)
        cursor.execute("SELECT s.IdSablon, s.IdJurnal, s.IdSubunitate, s.Tip, s.Explicatie, s.Compartiment, j.SursaSector FROM TZ_SablonOperatii s "
                       "JOIN TZ_Jurnale j ON j.IdJurnal = s.IdJurnal WHERE s.IdSablon = %s", (cerere["idsablon"],))
        sablon = cursor.fetchone()
        if sablon is None or sablon["Tip"] != "A":
            raise OpEroare("Șablonul ales nu există sau nu este de tip «operații diverse».", 404)
        if sablon["IdSubunitate"] not in (None, 0, sid) or sablon["SursaSector"] != sub["SursaSector"]:
            raise OpEroare("Șablonul ales nu aparține subunității în lucru.", 409)
        data = cerere["datadocument"]
        if data.year != an:
            raise OpEroare("Data documentului trebuie să fie în anul de lucru (%d)." % an, 409)
        inchidere = postare.data_ultimei_inchideri(cursor, sid, an)
        if inchidere is not None and data <= inchidere:
            raise OpEroare("Luna este închisă: data documentului trebuie să fie după %s." % inchidere.strftime("%d.%m.%Y"), 409)
        cursor.execute("SELECT IdLinie, Forma, ContDebit, AnaliticDebit, ContCredit, AnaliticCredit, PartDebitMod, IdPartDebit, PartCreditMod, IdPartCredit, "
                       "PA, Ordonantare, Chelt, IdClsf FROM TZ_SablonOperatiiLinie WHERE IdSablon = %s ORDER BY IdLinie", (cerere["idsablon"],))
        linii = [l for l in cursor.fetchall() if l["IdLinie"] in cerere["sume"]]
        if len(linii) != len(cerere["sume"]):
            raise OpEroare("O linie nu aparține șablonului ales (șablonul s-a schimbat între timp).", 409)
        if any("OPERATIE" in (l["PartDebitMod"], l["PartCreditMod"]) for l in linii) and cerere["idpartener"] is None:
            raise OpEroare("Alegeți partenerul: șablonul are linii cu «partenerul operației».", 409)
        if cerere["idpartener"] is not None:
            cursor.execute("SELECT 1 AS X FROM Parteneri WHERE IdPartener = %s", (cerere["idpartener"],))
            if cursor.fetchone() is None:
                raise OpEroare("Partenerul ales nu mai există.", 404)
        id_nota = postare.nota_existenta(cursor, sid, an, cerere["numarnc"])
        if id_nota is None:
            id_nota, _ = postare.creeaza_nota(cursor, sid, an, user, cerere["numarnc"])
        rezultat = []
        for l in linii:
            suma = cerere["sume"][l["IdLinie"]]
            clsf_f = clsf_e = None
            if l["IdClsf"]:
                cursor.execute("SELECT ClsfF, ClsfE FROM Clasificatii WHERE IDClsf = %s", (l["IdClsf"],))
                c = cursor.fetchone()
                if c is not None:
                    clsf_f, clsf_e = c["ClsfF"], c["ClsfE"]
            cont_d = postare.cont_pentru(cursor, sid, l["ContDebit"], l["AnaliticDebit"], clsf_f, clsf_e)
            cont_c = postare.cont_pentru(cursor, sid, l["ContCredit"], l["AnaliticCredit"], clsf_f, clsf_e)
            id_doc = postare.creeaza_document(cursor, id_nota, cerere["numardocument"], data, cerere["feldocument"], "A", True, user)
            antet = {"IdJurnal": sablon["IdJurnal"], "IdClsf": l["IdClsf"], "IdSablon": cerere["idsablon"], "DataOperatie": data,
                     "Explicatie": sablon["Explicatie"], "Tip": "A", "Compartiment": sablon["Compartiment"], "Angajament": l["PA"], "Propunere": l["PA"],
                     "Ordonantare": l["Ordonantare"], "Chelt": l["Chelt"], "Auto": True}
            id_oper, nr_ang = postare.creeaza_operatie(
                cursor, sid, an, id_doc, antet,
                [(cont_d, _id_partener_linie(l["PartDebitMod"], l["IdPartDebit"], cerere["idpartener"]), suma)],
                [(cont_c, _id_partener_linie(l["PartCreditMod"], l["IdPartCredit"], cerere["idpartener"]), suma)], user)
            rezultat.append({"idlinie": l["IdLinie"], "iddocument": id_doc, "idoper": id_oper, "nrang": nr_ang})
        conn.commit()
        logger.info("[avacont.op9] %s: nota %s, %s operatii", db_name, cerere["numarnc"], len(rezultat))
        return json_utf8({"idnota": id_nota, "numarnc": cerere["numarnc"], "an": an, "operatii": rezultat}, 200)
    except OpEroare as err:
        rollback(conn)
        return json_utf8({"error": str(err)}, err.status)
    except postare.PostareEroare as err:
        rollback(conn)
        return json_utf8({"error": str(err)}, err.status)
    except Exception as err:
        rollback(conn)
        logger.error("[avacont.op9] salvare %s: %s", db_name, err, exc_info=True)
        return json_utf8({"error": "Eroare la salvarea operațiilor diverse: %s" % err}, 500)
    finally:
        if conn is not None:
            conn.close()

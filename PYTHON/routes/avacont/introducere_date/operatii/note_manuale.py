"""SLICE-AV02-07, SLICE-AV08. HTTP adapter of the shared accounting editor (the rules live in NoteC.py / NoteC.py).

    GET  /api/avacont/note-manuale/context?idsubunitate&an[&idoper]   the lists of the editor; the accounts and the classifications carry ClsfF / ClsfE
         (and the accounts Cont / Analitic) for «Protectie operatii» (SLICE-AV08-01); Detalii = the operation being edited, with Refuz / Avertisment.
    GET  /api/avacont/note-manuale/documente?idsubunitate&an&idnota   the documents of a note and which ones can receive an operation.
    POST /api/avacont/note-manuale/salveaza   -> 200 {IdOper, IdDocument, IdNota, NumarNC, NumarDocument, DataDocument, DataOperatie, OperatiiDocument}
         refusal: {error, reason, camp?, parte?, linie?, confirmare?}; reason CONFIRMARE = a warning to answer, VERSIUNE = reload the operation.
"""
from contextlib import closing
import logging

from flask import g, request

from routes.auth.guard import require_session
from utils.database import get_kbot_connection

from ... import avacont_bp
from ...common import json_utf8
from . import NoteC
from .op9 import OpEroare, _intreg

logger = logging.getLogger(__name__)


def _istoric_antet(cursor, sid):
    cursor.execute(
        "SELECT DISTINCT UPPER(TRIM(d.FelDocument)) AS Valoare FROM TZ_Documente d "
        "JOIN TZ_NoteC n ON n.IdNota = d.IdNota WHERE n.IdSubunitate = %s AND TRIM(d.FelDocument) <> ''", (sid,))
    feluri = [row["Valoare"] for row in cursor.fetchall()]
    cursor.execute(
        "SELECT DISTINCT UPPER(TRIM(o.Compartiment)) AS Valoare FROM TZ_Oper o "
        "JOIN TZ_Documente d ON d.IdDocument = o.IdDocument JOIN TZ_NoteC n ON n.IdNota = d.IdNota "
        "WHERE n.IdSubunitate = %s AND TRIM(o.Compartiment) <> ''", (sid,))
    compartimente = [row["Valoare"] for row in cursor.fetchall()]
    cursor.execute(
        "SELECT DISTINCT UPPER(TRIM(o.Explicatie)) AS Valoare FROM TZ_Oper o "
        "JOIN TZ_Documente d ON d.IdDocument = o.IdDocument JOIN TZ_NoteC n ON n.IdNota = d.IdNota "
        "WHERE n.IdSubunitate = %s AND TRIM(o.Explicatie) <> ''", (sid,))
    return {"FeluriDocument": feluri, "Compartimente": compartimente, "Explicatii": [row["Valoare"] for row in cursor.fetchall()]}


def _context(cursor, sid, an, id_oper):
    sub = NoteC.subunitate_pentru(cursor, None, sid)
    ss = sub["SursaSector"]
    detalii = NoteC.detalii(cursor, sid, an, id_oper) if id_oper else None
    operatie = detalii["Operatie"] if detalii else {}
    cursor.execute("SELECT IdNota AS Id, NumarNC FROM TZ_NoteC WHERE IdSubunitate = %s AND An = %s ORDER BY NumarNC DESC", (sid, an))
    note = cursor.fetchall()
    cursor.execute("SELECT IdJurnal AS Id, Cod, Denumire FROM TZ_Jurnale WHERE SursaSector = %s OR IdJurnal = %s", (ss, operatie.get("IdJurnal")))
    jurnale = cursor.fetchall()
    cursor.execute("SELECT IDClsf AS Id, Clsf AS Cod, Denumire, ClsfF, ClsfE, Articol FROM Clasificatii WHERE SS = %s OR IDClsf = %s", (ss, operatie.get("IdClsf")))
    clasificatii = cursor.fetchall()
    linii = detalii["Debit"] + detalii["Credit"] if detalii else []
    conturi_istorice = sorted({r["IdCont"] for r in linii})
    parteneri_istorici = sorted({r["IdPartener"] for r in linii if r["IdPartener"] is not None})
    conditie_cont = " OR IdCont IN (" + ", ".join(["%s"] * len(conturi_istorice)) + ")" if conturi_istorice else ""
    cursor.execute("SELECT IdCont AS Id, SimbolCont AS Cod, Denumire, Cont, Analitic, ClsfF, ClsfE, Activ FROM TZ_PlanCont "
                   "WHERE (IdSubunitate = %s AND Activ = 1)" + conditie_cont,
                   (sid, *conturi_istorice))
    conturi = cursor.fetchall()
    conditie_partener = " OR IdPartener IN (" + ", ".join(["%s"] * len(parteneri_istorici)) + ")" if parteneri_istorici else ""
    cursor.execute("SELECT IdPartener AS Id, CodPartener AS Cod, DenumirePartener AS Denumire FROM Parteneri "
                   "WHERE (COALESCE(Ascuns, 0) = 0 AND IdUnitate IN (" + NoteC.SQL_UNITATI_SURSEI + "))" + conditie_partener,
                   (ss, an, *parteneri_istorici))
    parteneri = cursor.fetchall()
    return {"Subunitate": sub["Denumire"], "Note": note, "Jurnale": jurnale, "Clasificatii": clasificatii,
            "Conturi": conturi, "Parteneri": parteneri, "Detalii": detalii, **_istoric_antet(cursor, sid),
            "TipuriOperatie": [{"Cod": cod, "Denumire": denumire} for cod, denumire in NoteC.TIPURI_OPERATIE_MANUALA.items()]}


@avacont_bp.route("/api/avacont/note-manuale/context", methods=["GET"])
@require_session
def get_nota_manuala_context():
    try:
        sid = _intreg(request.args.get("idsubunitate"), "idsubunitate")
        an = _intreg(request.args.get("an"), "an")
        id_oper = _intreg(request.args.get("idoper"), "idoper", obligatoriu=False)
        with closing(get_kbot_connection(g.session.db_name)) as conn:
            return json_utf8(_context(conn.cursor(dictionary=True), sid, an, id_oper), 200)
    except (OpEroare, NoteC.NoteCEroare) as err:
        return json_utf8({"error": str(err)}, err.status)
    except Exception:
        logger.exception("Citirea datelor pentru nota manuală a eșuat.")
        return json_utf8({"error": "Datele notei contabile nu au putut fi încărcate."}, 500)


@avacont_bp.route("/api/avacont/note-manuale/documente", methods=["GET"])
@require_session
def get_nota_manuala_documente():
    try:
        sid = _intreg(request.args.get("idsubunitate"), "idsubunitate")
        an = _intreg(request.args.get("an"), "an")
        id_nota = _intreg(request.args.get("idnota"), "idnota")
        toate = request.args.get("toate") == "1"
        with closing(get_kbot_connection(g.session.db_name)) as conn:
            cursor = conn.cursor(dictionary=True)
            cursor.execute(
                "SELECT d.IdDocument, d.IdNota, d.NumarDocument, d.DataDocument, d.FelDocument, d.Fel, d.IdSalariiAccess "
                "FROM TZ_Documente d JOIN TZ_NoteC n ON n.IdNota = d.IdNota "
                "WHERE n.IdNota = %s AND n.IdSubunitate = %s AND n.An = %s ORDER BY d.DataDocument DESC, d.NumarDocument", (id_nota, sid, an))
            documente = cursor.fetchall()
            for doc in documente:
                doc["PoatePrimiOperatie"] = bool(
                    not doc.pop("IdSalariiAccess") and (doc["Fel"] or "").upper() in ("", "A", "V")
                    and not NoteC.este_document_inchidere(doc["Fel"], doc["NumarDocument"]))
            if not toate:
                documente = [doc for doc in documente if doc["PoatePrimiOperatie"]]
            return json_utf8(documente, 200)
    except OpEroare as err:
        return json_utf8({"error": str(err)}, err.status)
    except Exception:
        logger.exception("Citirea documentelor notei contabile a eșuat.")
        return json_utf8({"error": "Documentele notei contabile nu au putut fi încărcate."}, 500)


@avacont_bp.route("/api/avacont/note-manuale/salveaza", methods=["POST"])
@require_session
def post_nota_manuala():
    try:
        rol = (getattr(g.session, "ctx", None) or {}).get("Role")
        rezultat = NoteC.salveaza_nota_manuala(g.session.db_name, g.session.username, rol, request.get_json(silent=True))
        return json_utf8(rezultat, 200)
    except NoteC.NoteCEroare as err:
        return json_utf8(err.corp(), err.status)
    except Exception:
        logger.exception("Salvarea notei contabile a eșuat.")
        return json_utf8({"error": "Nota contabilă nu a putut fi salvată. Reîncărcați lista înainte de a relua salvarea."}, 500)

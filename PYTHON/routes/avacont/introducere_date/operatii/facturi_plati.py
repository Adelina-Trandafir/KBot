# routes/avacont/introducere_date/operatii/facturi_plati.py
"""
«Facturi si plati» (slice AV02-04; the Access form OPuri2017): invoices typed by hand, invoices already evidenced and not paid in full, and the payment with its
order (OP) for ONE partner. The evidence and the payment use the same services as «Contari» of the received e-invoices (NoteC.py, NoteC.py): one invoice = one
document + one evidence operation (the template of type M, kind E, of the classification); one payment = one document «OP» + one payment operation + one OP, with
the allocation on each invoice in TZ_PlatiFacturi (IdEvidentiere = the evidence operation; IdPrimita is set when that operation has an exact e-invoice link).

    GET    /api/avacont/facturi-plati/context?idsubunitate=<n>&an=<yyyy>
           -> { subunitate, note, urmatoare, inchidere, parteneri: [{idpartener, cod, denumire, cui, codclient, iban}],
                clasificatii: [{idclsf, clsf, denumire, forma, sabloane_e: [{idsablon, explicatie}], sabloane_p: [{idsablon, explicatie, felop}]}] }
    GET    /api/avacont/facturi-plati/deschise?idsubunitate=<n>&an=<yyyy>&idpartener=<n>&idclsf=<n>
           -> { curent: [invoice], anterior: [invoice], plati: [{idop, nrop, dataop, valoare, explicatie, documente, blocat, motiv}], numarop: n, coduri: {...}, contiban }
              invoice = { idoper, numar, data, fel, valoare, platit, rest }; only the ones with a rest. «anterior» = the evidence of the previous year that this
              database holds (the previous year before the port is not in it: see the worklog).
    POST   /api/avacont/facturi-plati/salveaza
           { idsubunitate, an, idpartener, idclsf, idsablone | null, idsablonp | null, numarnc, dataop, explicatieop,
             noi: [{numar, data, suma, fel, numarnc, platit}], existente: [{idoper, platit}] }
              numarnc of a new invoice = its accounting note (MANDATORY, positive; a number that does not exist yet opens a new note);
              numarnc of the request = the note of the PAYMENT (mandatory only when idsablonp is given).
           -> { numarnc, note: [every note used, ascending], an, evidente: [{numar, idoper}], plata: {idoper, idop, nrop, valoare, dataop} | null }
    DELETE /api/avacont/facturi-plati/plata/<idop>   -> { idop, nrop }; refused when the order is in an OPME or confirmed by a bank statement.

The number of the order shown BEFORE the save is provisional («numarop»); the save returns the real one in «plata.nrop».
"""
import logging
from datetime import datetime
from decimal import Decimal, InvalidOperation

from flask import g, request

from routes.auth.guard import require_session
from utils.database import get_kbot_connection

from ... import avacont_bp
from ...common import json_utf8
from . import NoteC
from .op9 import OpEroare, _data, _intreg, _iso

logger = logging.getLogger(__name__)

CENT = Decimal("0.01")


def _suma(valoare, nume):
    try:
        suma = Decimal(str(valoare))
        if not suma.is_finite() or abs(suma) >= Decimal("10000000000000000"):
            raise InvalidOperation
        return suma.quantize(CENT)
    except (InvalidOperation, ValueError) as err:
        raise OpEroare("Suma «%s» nu este validă." % nume) from err


def _lista(corp, cheie):
    valoare = corp.get(cheie)
    if valoare is None:
        return []
    if not isinstance(valoare, list):
        raise OpEroare("Câmpul «%s» nu este valid." % cheie)
    return valoare


def _clasificatii_cu_sabloane(cursor, id_subunitate, ss):
    index = NoteC.sabloane_subunitatii(cursor, id_subunitate)
    out = []
    for c in NoteC.clasificatii_cu_sabloane(cursor, index):
        if c["SS"] != ss:
            continue
        slot = index[(c["Cheie"], c["SS"])]
        out.append({"idclsf": c["IdClsf"], "clsf": c["Clsf"], "denumire": c["Denumire"], "forma": c["Forma"],
                    "sabloane_e": [{"idsablon": s["IdSablon"], "explicatie": s["Explicatie"]} for s in slot["E"]],
                    "sabloane_p": [{"idsablon": s["IdSablon"], "explicatie": s["Explicatie"], "felop": s["FelOP"]} for s in slot["P"]]})
    return out


@avacont_bp.route("/api/avacont/facturi-plati/context", methods=["GET"])
@require_session
def get_facturi_plati_context():
    """Notes, partners and the classifications that have templates in the subunit."""
    db_name = g.session.db_name
    conn = None
    try:
        sid = _intreg(request.args.get("idsubunitate"), "idsubunitate")
        an = _intreg(request.args.get("an"), "an")
        conn = get_kbot_connection(db_name)
        cursor = conn.cursor(dictionary=True)
        sub = NoteC.subunitate_pentru(cursor, None, sid)
        note, urmatoare = NoteC.note_ale_subunitatii(cursor, sid, an)
        # Parteneri.CodClient comes with sql/AVACONT/IntroducereDate/Operatii/AV02_04_parteneri_cod_client.sql; until it is run the code stays empty.
        cursor.execute("SHOW COLUMNS FROM Parteneri LIKE 'CodClient'")
        coloana_cod_client = ", CodClient" if cursor.fetchone() else ""
        cursor.execute("SELECT IdPartener, CodPartener, DenumirePartener, CodFiscal, ContIBAN" + coloana_cod_client + " FROM Parteneri WHERE COALESCE(Ascuns, 0) = 0 "
                       "AND IdUnitate IN (" + NoteC.SQL_UNITATI_SURSEI + ") ORDER BY DenumirePartener, IdPartener", (sub["SursaSector"], an))
        parteneri = [{"idpartener": r["IdPartener"], "cod": r["CodPartener"] or "", "denumire": r["DenumirePartener"] or "", "cui": r["CodFiscal"] or "",
                      "codclient": r.get("CodClient") or "", "iban": r["ContIBAN"] or ""} for r in cursor.fetchall()]
        return json_utf8({
            "subunitate": {"idsubunitate": sub["IdSubunitate"], "denumire": sub["Denumire"], "sursasector": sub["SursaSector"]},
            "note": [{"idnota": n["IdNota"], "numarnc": n["NumarNC"], "datanc": _iso(n["DataNc"]), "documente": int(n["Documente"])} for n in note], "urmatoare": urmatoare,
            "inchidere": _iso(NoteC.data_ultimei_inchideri(cursor, sid, an)), "parteneri": parteneri,
            "clasificatii": _clasificatii_cu_sabloane(cursor, sid, sub["SursaSector"])}, 200)
    except (OpEroare, NoteC.NoteCEroare) as err:
        return json_utf8({"error": str(err)}, err.status)
    except Exception as err:
        logger.error("[avacont.facturi_plati] context %s: %s", db_name, err, exc_info=True)
        return json_utf8({"error": "Eroare la citirea datelor pentru facturi și plăți: %s" % err}, 500)
    finally:
        if conn is not None:
            conn.close()


def _facturi_deschise(cursor, sid, an, id_partener):
    """The evidence operations (Tip E) of the subunit and year with a credit line of the partner, and what is left to pay on each (only the ones with a rest)."""
    cursor.execute(
        "SELECT DISTINCT o.IdOper, d.NumarDocument, d.DataDocument, d.FelDocument FROM TZ_Credit c JOIN TZ_Oper o ON o.IdOper = c.IdOper "
        "JOIN TZ_Documente d ON d.IdDocument = o.IdDocument JOIN TZ_NoteC n ON n.IdNota = d.IdNota "
        "WHERE c.IdPartener = %s AND o.Tip = 'E' AND n.IdSubunitate = %s AND n.An = %s "
        "ORDER BY d.DataDocument, d.NumarDocument", (id_partener, sid, an))
    randuri = cursor.fetchall()
    ids = [r["IdOper"] for r in randuri]
    legaturi = NoteC.primite_pe_evidentieri(cursor, ids, strict=False)
    valori = NoteC.valori_pe_operatii(cursor, ids, id_partener, legaturi)
    cu_receptie = NoteC.primite_cu_receptie(cursor, (e[0] for e in legaturi.values()))
    out = []
    for r in randuri:
        valoare, platit, ramas = valori[r["IdOper"]]
        if ramas != 0:
            id_primita = legaturi.get(r["IdOper"], (None,))[0]
            out.append({"idoper": r["IdOper"], "numar": r["NumarDocument"], "data": _iso(r["DataDocument"]), "fel": r["FelDocument"], "valoare": float(valoare),
                        "platit": float(platit), "rest": float(ramas), "idprimita": id_primita,
                        "fara_receptie": id_primita is not None and id_primita not in cu_receptie})
    return out


@avacont_bp.route("/api/avacont/facturi-plati/deschise", methods=["GET"])
@require_session
def get_facturi_plati_deschise():
    """The unpaid invoices of the partner (current and previous year), its payments of the year and the provisional number of the next order."""
    db_name = g.session.db_name
    conn = None
    try:
        sid = _intreg(request.args.get("idsubunitate"), "idsubunitate")
        an = _intreg(request.args.get("an"), "an")
        id_partener = _intreg(request.args.get("idpartener"), "idpartener")
        id_clsf = _intreg(request.args.get("idclsf"), "idclsf", obligatoriu=False)
        conn = get_kbot_connection(db_name)
        cursor = conn.cursor(dictionary=True)
        NoteC.subunitate_pentru(cursor, None, sid)
        cursor.execute("SELECT o.IdOP, o.NrOP, o.DataOP, o.Valoare, o.Explicatie, o.Documente FROM TZ_OPuri o WHERE o.IdSubunitate = %s AND o.IdPartener = %s "
                       "AND YEAR(o.DataOP) = %s ORDER BY o.DataOP DESC, o.IdOP DESC", (sid, id_partener, an))
        ordine = []
        for o in cursor.fetchall():
            motiv = NoteC.motiv_blocare(cursor, o["IdOP"])
            ordine.append({"idop": o["IdOP"], "nrop": o["NrOP"], "dataop": _iso(o["DataOP"]), "valoare": float(o["Valoare"]), "explicatie": o["Explicatie"] or "",
                           "documente": o["Documente"] or "", "blocat": motiv is not None, "motiv": motiv or ""})
        iban, coduri = NoteC.cont_iban_partener(cursor, id_partener, id_clsf)
        return json_utf8({"curent": _facturi_deschise(cursor, sid, an, id_partener), "anterior": _facturi_deschise(cursor, sid, an - 1, id_partener),
                          "plati": ordine, "numarop": NoteC.numar_op_urmator(cursor, sid), "contiban": iban,
                          "coduri": {"codang": (coduri or {}).get("CodAng", ""), "codind": (coduri or {}).get("CodInd", "")} if coduri else None}, 200)
    except (OpEroare, NoteC.NoteCEroare, NoteC.PlatiEroare) as err:
        return json_utf8({"error": str(err)}, err.status)
    except Exception as err:
        logger.error("[avacont.facturi_plati] deschise %s: %s", db_name, err, exc_info=True)
        return json_utf8({"error": "Eroare la citirea facturilor partenerului: %s" % err}, 500)
    finally:
        if conn is not None:
            conn.close()


def _citeste_cerere(corp):
    if not isinstance(corp, dict):
        raise OpEroare("Corp JSON lipsă sau nevalid.")
    cerere = {"sid": _intreg(corp.get("idsubunitate"), "idsubunitate"), "an": _intreg(corp.get("an"), "an"),
              "id_partener": _intreg(corp.get("idpartener"), "idpartener"), "id_clsf": _intreg(corp.get("idclsf"), "idclsf"),
              "id_e": _intreg(corp.get("idsablone"), "idsablone", obligatoriu=False), "id_p": _intreg(corp.get("idsablonp"), "idsablonp", obligatoriu=False),
              "numar_nc": _intreg(corp.get("numarnc"), "numarnc", obligatoriu=False) or 0,
              "idnota": _intreg(corp.get("idnota"), "idnota", obligatoriu=False), "nota_noua": corp.get("nota_noua"),
              "protectie": corp.get("protectie") is True, "confirmari_receptie": set(), "noi": [], "existente": {},
              "confirmari": [str(x) for x in _lista(corp, "confirmari")]}   # SLICE-AV08-07: the warning codes the operator answered «Da» (LUNA_NOUA)
    for id_primita in _lista(corp, "confirmari_receptie"):
        cerere["confirmari_receptie"].add(_intreg(id_primita, "idprimita"))
    # numarnc of the request = the note of the PAYMENT (checked when there is one); every typed invoice carries its own note
    if cerere["numar_nc"] < 0:
        raise OpEroare("Numărul notei contabile trebuie să fie pozitiv.")
    cerere["data_op"] = _data(corp.get("dataop"), "dataop") if corp.get("dataop") else datetime.now().date()
    cerere["explicatie_op"] = str(corp.get("explicatieop") or "")
    for f in _lista(corp, "noi"):
        if not isinstance(f, dict):
            raise OpEroare("O factură nu este validă.")
        numar = str(f.get("numar") or "").strip()
        if not numar or len(numar) > 100:
            raise OpEroare("Numărul unei facturi trebuie să aibă între 1 și 100 de caractere.")
        suma = _suma(f.get("suma"), numar)
        if suma == 0:
            raise OpEroare("Factura %s are suma zero." % numar)
        platit = _suma(f.get("platit") or 0, numar)
        if not NoteC.in_limita(platit, suma):
            raise OpEroare("Suma plătită a facturii %s nu poate depăși valoarea ei (%s)." % (numar, suma))
        fel = str(f.get("fel") or "Factură").strip()[:100] or "Factură"
        numar_nc = _intreg(f.get("numarnc"), "numarnc al facturii " + numar, obligatoriu=False) or 0
        if numar_nc <= 0:
            raise OpEroare("Factura %s nu are numărul notei contabile (obligatoriu)." % numar)
        cerere["noi"].append({"numar": numar, "data": _data(f.get("data"), "data facturii " + numar), "suma": suma, "platit": platit, "fel": fel,
                              "numar_nc": numar_nc, "idnota": _intreg(f.get("idnota"), "idnota", obligatoriu=False), "nota_noua": f.get("nota_noua")})
    for f in _lista(corp, "existente"):
        if not isinstance(f, dict):
            raise OpEroare("O factură nu este validă.")
        id_oper = _intreg(f.get("idoper"), "idoper")
        if id_oper in cerere["existente"]:
            raise OpEroare("Aceeași factură apare de două ori.")
        cerere["existente"][id_oper] = _suma(f.get("platit") or 0, str(id_oper))
    return cerere


@avacont_bp.route("/api/avacont/facturi-plati/salveaza", methods=["POST"])
@require_session
def post_facturi_plati_salveaza():
    """Books the typed invoices (evidence) and/or the payment with its order, in ONE transaction."""
    db_name = g.session.db_name
    user = g.session.username
    try:
        c = _citeste_cerere(request.get_json(silent=True))
        with NoteC.salvare(db_name) as conn:
            rezultat = NoteC.salveaza_facturi_plati(conn.cursor(dictionary=True), db_name, user, (getattr(g.session, "ctx", None) or {}).get("Role"), c)
        logger.info("[avacont.facturi_plati] %s: note %s, %s facturi noi, plata %s", db_name, rezultat["note"], len(rezultat["evidente"]),
                    rezultat["plata"]["nrop"] if rezultat["plata"] else "-")
        return json_utf8(rezultat, 200)
    except NoteC.NoteCEroare as err:
        return json_utf8(err.corp(), err.status)   # SLICE-AV08-07: reason CONFIRMARE + the code = a warning the page asks Da / Nu
    except (OpEroare, NoteC.PlatiEroare) as err:
        return json_utf8({"error": str(err)}, err.status)
    except Exception as err:
        logger.error("[avacont.facturi_plati] salvare %s: %s", db_name, err, exc_info=True)
        return json_utf8({"error": "Eroare la salvarea facturilor și plăților: %s" % err}, 500)


@avacont_bp.route("/api/avacont/facturi-plati/plata/<int:id_op>", methods=["DELETE"])
@require_session
def delete_facturi_plati_plata(id_op):
    """Deletes an UNLOCKED order with its payment (SLICE-AV08-04: inside NoteC.salvare, like every write of the operations)."""
    db_name = g.session.db_name
    try:
        with NoteC.salvare(db_name) as conn:
            numar = NoteC.sterge_plata(conn.cursor(dictionary=True), id_op)
        return json_utf8({"idop": id_op, "nrop": numar}, 200)
    except (NoteC.PlatiEroare, NoteC.NoteCEroare) as err:
        return json_utf8({"error": str(err)}, err.status)
    except Exception as err:
        logger.error("[avacont.facturi_plati] stergere %s: %s", db_name, err, exc_info=True)
        return json_utf8({"error": "Eroare la ștergerea plății: %s" % err}, 500)

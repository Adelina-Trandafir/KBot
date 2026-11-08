# routes/avacont/introducere_date/operatii/facturi_plati.py
"""
«Facturi si plati» (slice AV02-04; the Access form OPuri2017): invoices typed by hand, invoices already evidenced and not paid in full, and the payment with its
order (OP) for ONE partner. The evidence and the payment use the same services as «Contari» of the received e-invoices (postare.py, plati.py): one invoice = one
document + one evidence operation (the template of type M, kind E, of the classification); one payment = one document «OP» + one payment operation + one OP, with
the allocation on each invoice in TZ_PlatiFacturi (IdEvidentiere = the evidence operation; IdPrimita stays NULL because nothing here comes from EF_Primite).

    GET    /api/avacont/facturi-plati/context?idsubunitate=<n>&an=<yyyy>
           -> { subunitate, note, urmatoare, inchidere, parteneri: [{idpartener, cod, denumire, cui, iban}],
                clasificatii: [{idclsf, clsf, denumire, forma, sabloane_e: [{idsablon, explicatie}], sabloane_p: [{idsablon, explicatie, felop}]}] }
    GET    /api/avacont/facturi-plati/deschise?idsubunitate=<n>&an=<yyyy>&idpartener=<n>&idclsf=<n>
           -> { curent: [invoice], anterior: [invoice], plati: [{idop, nrop, dataop, valoare, explicatie, documente, blocat, motiv}], numarop: n, coduri: {...}, contiban }
              invoice = { idoper, numar, data, fel, valoare, platit, rest }; only the ones with a rest. «anterior» = the evidence of the previous year that this
              database holds (the previous year before the port is not in it: see the worklog).
    POST   /api/avacont/facturi-plati/salveaza
           { idsubunitate, an, idpartener, idclsf, idsablone | null, idsablonp | null, numarnc, dataop, explicatieop,
             noi: [{numar, data, suma, fel, platit}], existente: [{idoper, platit}] }
           -> { idnota, numarnc, an, evidente: [{numar, idoper}], plata: {idoper, idop, nrop, valoare, dataop} | null }
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
from ...common import json_utf8, rollback
from . import plati, postare
from .op9 import OpEroare, _data, _intreg, _iso

logger = logging.getLogger(__name__)

CENT = Decimal("0.01")
TREZORERIE_BIC = "TREZROBU"


def _suma(valoare, nume):
    try:
        return Decimal(str(valoare)).quantize(CENT)
    except (InvalidOperation, ValueError) as err:
        raise OpEroare("Suma «%s» nu este validă." % nume) from err


def _in_limita(platit, rest):
    """A payment of the same sign as the rest and not above it (a credit note has a negative rest)."""
    if rest >= 0:
        return 0 <= platit <= rest
    return rest <= platit <= 0


def _lista(corp, cheie):
    valoare = corp.get(cheie)
    if valoare is None:
        return []
    if not isinstance(valoare, list):
        raise OpEroare("Câmpul «%s» nu este valid." % cheie)
    return valoare


def _clasificatii_cu_sabloane(cursor, id_subunitate, ss):
    from routes.efactura import primite_contari as C
    index = C._sabloane(cursor, id_subunitate)
    out = []
    for c in C._clasificatii(cursor, index):
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
        sub = postare.subunitate_pentru(cursor, None, sid)
        note, urmatoare = postare.note_ale_subunitatii(cursor, sid, an)
        cursor.execute("SELECT IdPartener, CodPartener, DenumirePartener, CodFiscal, ContIBAN FROM Parteneri WHERE COALESCE(Ascuns, 0) = 0 "
                       "ORDER BY DenumirePartener, IdPartener")
        parteneri = [{"idpartener": r["IdPartener"], "cod": r["CodPartener"] or "", "denumire": r["DenumirePartener"] or "", "cui": r["CodFiscal"] or "",
                      "iban": r["ContIBAN"] or ""} for r in cursor.fetchall()]
        return json_utf8({
            "subunitate": {"idsubunitate": sub["IdSubunitate"], "denumire": sub["Denumire"], "sursasector": sub["SursaSector"]},
            "note": [{"numarnc": n["NumarNC"], "datanc": _iso(n["DataNc"]), "documente": int(n["Documente"])} for n in note], "urmatoare": urmatoare,
            "inchidere": _iso(postare.data_ultimei_inchideri(cursor, sid, an)), "parteneri": parteneri,
            "clasificatii": _clasificatii_cu_sabloane(cursor, sid, sub["SursaSector"])}, 200)
    except (OpEroare, postare.PostareEroare) as err:
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
        "SELECT o.IdOper, d.NumarDocument, d.DataDocument, d.FelDocument, SUM(c.Suma) AS Valoare FROM TZ_Credit c JOIN TZ_Oper o ON o.IdOper = c.IdOper "
        "JOIN TZ_Documente d ON d.IdDocument = o.IdDocument JOIN TZ_NoteC n ON n.IdNota = d.IdNota "
        "WHERE c.IdPartener = %s AND o.Tip = 'E' AND n.IdSubunitate = %s AND n.An = %s "
        "GROUP BY o.IdOper, d.NumarDocument, d.DataDocument, d.FelDocument ORDER BY d.DataDocument, o.IdOper", (id_partener, sid, an))
    randuri = cursor.fetchall()
    rest = plati.rest_pe_operatii(cursor, [r["IdOper"] for r in randuri], id_partener)
    out = []
    for r in randuri:
        valoare, ramas = plati.cent(r["Valoare"]), rest[r["IdOper"]]
        if ramas != 0:
            out.append({"idoper": r["IdOper"], "numar": r["NumarDocument"], "data": _iso(r["DataDocument"]), "fel": r["FelDocument"], "valoare": float(valoare),
                        "platit": float(valoare - ramas), "rest": float(ramas)})
    return out


def _cont_iban(cursor, id_partener, id_clsf):
    from routes.efactura import primite_contari as C
    coduri = C._coduri(cursor, id_partener, id_clsf) if id_clsf else None
    iban = (coduri or {}).get("ContBancar") or ""
    if not iban:
        cursor.execute("SELECT ContIBAN FROM Parteneri WHERE IdPartener = %s", (id_partener,))
        row = cursor.fetchone()
        iban = (row["ContIBAN"] if row else "") or ""
    return iban, coduri


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
        postare.subunitate_pentru(cursor, None, sid)
        cursor.execute("SELECT o.IdOP, o.NrOP, o.DataOP, o.Valoare, o.Explicatie, o.Documente FROM TZ_OPuri o WHERE o.IdSubunitate = %s AND o.IdPartener = %s "
                       "AND YEAR(o.DataOP) = %s ORDER BY o.DataOP DESC, o.IdOP DESC", (sid, id_partener, an))
        ordine = []
        for o in cursor.fetchall():
            motiv = plati.motiv_blocare(cursor, o["IdOP"])
            ordine.append({"idop": o["IdOP"], "nrop": o["NrOP"], "dataop": _iso(o["DataOP"]), "valoare": float(o["Valoare"]), "explicatie": o["Explicatie"] or "",
                           "documente": o["Documente"] or "", "blocat": motiv is not None, "motiv": motiv or ""})
        iban, coduri = _cont_iban(cursor, id_partener, id_clsf)
        return json_utf8({"curent": _facturi_deschise(cursor, sid, an, id_partener), "anterior": _facturi_deschise(cursor, sid, an - 1, id_partener),
                          "plati": ordine, "numarop": plati.numar_op_urmator(cursor, sid), "contiban": iban,
                          "coduri": {"codang": (coduri or {}).get("CodAng", ""), "codind": (coduri or {}).get("CodInd", "")} if coduri else None}, 200)
    except (OpEroare, postare.PostareEroare, plati.PlatiEroare) as err:
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
              "numar_nc": _intreg(corp.get("numarnc"), "numarnc"), "noi": [], "existente": {}}
    if cerere["numar_nc"] <= 0:
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
        if not _in_limita(platit, suma):
            raise OpEroare("Suma plătită a facturii %s nu poate depăși valoarea ei (%s)." % (numar, suma))
        fel = str(f.get("fel") or "Factură").strip()[:100] or "Factură"
        cerere["noi"].append({"numar": numar, "data": _data(f.get("data"), "data facturii " + numar), "suma": suma, "platit": platit, "fel": fel})
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
    conn = None
    try:
        c = _citeste_cerere(request.get_json(silent=True))
        cu_plata = c["id_p"] is not None
        total = sum((f["platit"] for f in c["noi"]), Decimal(0)) + sum(c["existente"].values(), Decimal(0))
        if not c["noi"] and not (cu_plata and total != 0):
            raise OpEroare("Nu este nimic de înregistrat: adăugați o factură nouă sau alegeți un șablon de plată și sume de plătit.")
        if c["noi"] and c["id_e"] is None:
            raise OpEroare("Alegeți șablonul de evidențiere pentru facturile noi.")
        if cu_plata and total <= 0:
            raise OpEroare("Suma de plată trebuie să fie pozitivă.", 409)
        if not cu_plata and total != 0:
            raise OpEroare("Alegeți șablonul de plată: sunt sume de plată pe facturi.")
        conn = get_kbot_connection(db_name)
        conn.autocommit = False
        if not conn.in_transaction:
            conn.start_transaction()
        cursor = conn.cursor(dictionary=True)
        sid, an = c["sid"], c["an"]
        sub = postare.subunitate_pentru(cursor, None, sid)
        from routes.efactura import primite_contari as C
        from routes.efactura import primite_plata
        index = C._sabloane(cursor, sid)
        clsf = next((x for x in C._clasificatii(cursor, index) if x["IdClsf"] == c["id_clsf"] and x["SS"] == sub["SursaSector"]), None)
        if clsf is None:
            raise OpEroare("Clasificația aleasă nu are șabloane de operații în subunitatea în lucru.", 409)
        slot = index[(clsf["Cheie"], clsf["SS"])]
        if c["id_e"] is not None and c["id_e"] not in {s["IdSablon"] for s in slot["E"]}:
            raise OpEroare("Șablonul de evidențiere ales nu se potrivește clasificației.", 409)
        if c["id_p"] is not None and c["id_p"] not in {s["IdSablon"] for s in slot["P"]}:
            raise OpEroare("Șablonul de plată ales nu se potrivește clasificației.", 409)
        cursor.execute("SELECT IdPartener, DenumirePartener, CodFiscal FROM Parteneri WHERE IdPartener = %s", (c["id_partener"],))
        partener = cursor.fetchone()
        if partener is None:
            raise OpEroare("Partenerul ales nu mai există.", 404)
        # the existing invoices: evidence operations of this subunit with a credit line of the partner; the rest is read under lock
        ids_oper = list(c["existente"])
        existente = {}
        if ids_oper:
            cursor.execute(
                "SELECT o.IdOper, d.NumarDocument, d.DataDocument FROM TZ_Oper o JOIN TZ_Documente d ON d.IdDocument = o.IdDocument JOIN TZ_NoteC n ON n.IdNota = d.IdNota "
                "WHERE o.IdOper IN (" + ", ".join(["%s"] * len(ids_oper)) + ") AND o.Tip = 'E' AND n.IdSubunitate = %s FOR UPDATE", tuple(ids_oper) + (sid,))
            existente = {r["IdOper"]: r for r in cursor.fetchall()}
            if len(existente) != len(ids_oper):
                raise OpEroare("O factură aleasă nu mai există sau nu aparține subunității.", 409)
            valori = plati.valori_pe_operatii(cursor, ids_oper, c["id_partener"])
            rest = {i: v[1] for i, v in valori.items()}
            for id_oper in ids_oper:
                if c["existente"][id_oper] != 0 and not _in_limita(c["existente"][id_oper], rest[id_oper]):
                    raise OpEroare("Suma de plată a facturii %s depășește restul de plată (%s)." % (existente[id_oper]["NumarDocument"], rest[id_oper]), 409)
        # note
        inchidere = postare.data_ultimei_inchideri(cursor, sid, an)
        id_nota = postare.nota_existenta(cursor, sid, an, c["numar_nc"])
        if id_nota is None:
            id_nota, _ = postare.creeaza_nota(cursor, sid, an, user, c["numar_nc"])
        # evidence of the typed invoices
        evidente, alocari = [], []
        if c["noi"]:
            cursor.execute("SELECT IdJurnal, Explicatie, Compartiment FROM TZ_SablonOperatii WHERE IdSablon = %s", (c["id_e"],))
            sablon_e = cursor.fetchone()
            linii_e = C._linii_sablon(cursor, c["id_e"], clsf["Forma"])
            if sablon_e is None or not linii_e:
                raise OpEroare("Șablonul de evidențiere nu are linie pentru forma %s a clasificației." % clsf["Forma"], 409)
            linie_e = linii_e[0]
            cont_d = postare.cont_pentru(cursor, sid, linie_e["ContDebit"], linie_e["AnaliticDebit"], clsf["ClsfF"], clsf["ClsfE"])
            cont_c = postare.cont_pentru(cursor, sid, linie_e["ContCredit"], linie_e["AnaliticCredit"], clsf["ClsfF"], clsf["ClsfE"])
            for f in c["noi"]:
                data, _m = postare.ajusteaza_data(f["data"], None, an)
                data, _m = postare.ajusteaza_data(data, inchidere, None)
                if data.year != an:
                    raise OpEroare("Data facturii %s iese din anul de lucru (%d)." % (f["numar"], an), 409)
                id_doc = postare.creeaza_document(cursor, id_nota, C.numar_document(f["numar"]), data, f["fel"], "E", True, user)
                antet = {"IdJurnal": sablon_e["IdJurnal"], "IdClsf": clsf["IdClsf"], "IdSablon": c["id_e"], "DataOperatie": data, "Explicatie": sablon_e["Explicatie"],
                         "Tip": "E", "Compartiment": sablon_e["Compartiment"], "Angajament": linie_e["PA"], "Propunere": linie_e["PA"],
                         "Ordonantare": linie_e["Ordonantare"], "Chelt": linie_e["Chelt"], "Auto": True}
                id_oper, _nr = postare.creeaza_operatie(
                    cursor, sid, an, id_doc, antet,
                    [(cont_d, plati.id_partener_linie(linie_e["PartDebitMod"], linie_e["IdPartDebit"], c["id_partener"]), f["suma"])],
                    [(cont_c, plati.id_partener_linie(linie_e["PartCreditMod"], linie_e["IdPartCredit"], c["id_partener"]), f["suma"])],
                    user)
                evidente.append({"numar": f["numar"], "idoper": id_oper})
                if f["platit"] != 0:
                    alocari.append({"IdOper": id_oper, "NumarDocument": C.numar_document(f["numar"]), "DataDocument": data, "Valoare": f["suma"], "Platit": f["platit"]})
        for id_oper, platit in c["existente"].items():
            if platit != 0:
                r = existente[id_oper]
                alocari.append({"IdOper": id_oper, "NumarDocument": r["NumarDocument"], "DataDocument": r["DataDocument"], "Valoare": valori[id_oper][0], "Platit": platit})
        # payment and order
        plata = None
        if cu_plata:
            data_op, _m = postare.ajusteaza_data(c["data_op"], inchidere, None)
            if data_op.year != an:
                raise OpEroare("Data ordinului de plată iese din anul de lucru (%d)." % an, 409)
            iban, coduri = _cont_iban(cursor, c["id_partener"], c["id_clsf"])
            if not iban:
                raise OpEroare("Nu se poate adăuga operația de plată: partenerul selectat nu are niciun cont bancar configurat!", 409)
            documente = [a["NumarDocument"] for a in alocari]
            cursor.execute("SELECT IdJurnal, Explicatie, Compartiment, FelOP FROM TZ_SablonOperatii WHERE IdSablon = %s", (c["id_p"],))
            sablon_p = cursor.fetchone()
            linii_p = C._linii_sablon(cursor, c["id_p"], clsf["Forma"])
            if sablon_p is None or not linii_p:
                raise OpEroare("Șablonul de plată nu are linie pentru forma %s a clasificației." % clsf["Forma"], 409)
            explicatie = c["explicatie_op"].strip() or " ".join(x for x in ["F " + " ".join(documente) if documente else "", sablon_p["FelOP"] or ""] if x)
            explicatie = C.text_op(explicatie, C.MAX_EXPLICATIE_OP)
            if len(explicatie) < C.MIN_EXPLICATIE_OP:
                raise OpEroare("Explicația ordinului de plată trebuie să aibă între %d și %d caractere." % (C.MIN_EXPLICATIE_OP, C.MAX_EXPLICATIE_OP))
            den_unit, cf_unit, program = primite_plata._date_unitate(cursor, db_name, clsf["SS"], an)
            id_oper, id_op, numar, _doc = plati.posteaza_ordin(cursor, {
                "sid": sid, "an_nota": an, "id_nota": id_nota, "data_op": data_op, "id_p": c["id_p"], "sablon": sablon_p, "linie": linii_p[0], "clsf": clsf,
                "id_partener": c["id_partener"], "total": total, "user": user,
                "header": {"Explicatie": explicatie, "Documente": ";".join(documente), "DenPartener": partener["DenumirePartener"], "CFPartener": partener["CodFiscal"],
                           "ContBeneficiar": iban, "DenUnitate": den_unit, "CFUnitate": cf_unit, "pBIC": TREZORERIE_BIC,
                           "CodAngajament": (coduri or {}).get("CodAng") or None, "CodIndicator": (coduri or {}).get("CodInd") or None, "CodProgram": program}})
            plati.aloca_pe_operatii(cursor, id_oper, id_op, alocari, data_op, total, user)
            plata = {"idoper": id_oper, "idop": id_op, "nrop": numar, "valoare": float(total), "dataop": _iso(data_op)}
        conn.commit()
        logger.info("[avacont.facturi_plati] %s: nota %s, %s facturi noi, plata %s", db_name, c["numar_nc"], len(evidente), plata["nrop"] if plata else "-")
        return json_utf8({"idnota": id_nota, "numarnc": c["numar_nc"], "an": an, "evidente": evidente, "plata": plata}, 200)
    except (OpEroare, postare.PostareEroare, plati.PlatiEroare) as err:
        rollback(conn)
        return json_utf8({"error": str(err)}, err.status)
    except Exception as err:
        rollback(conn)
        logger.error("[avacont.facturi_plati] salvare %s: %s", db_name, err, exc_info=True)
        return json_utf8({"error": "Eroare la salvarea facturilor și plăților: %s" % err}, 500)
    finally:
        if conn is not None:
            conn.close()


@avacont_bp.route("/api/avacont/facturi-plati/plata/<int:id_op>", methods=["DELETE"])
@require_session
def delete_facturi_plati_plata(id_op):
    """Deletes an UNLOCKED order with its payment."""
    db_name = g.session.db_name
    conn = None
    try:
        conn = get_kbot_connection(db_name)
        conn.autocommit = False
        if not conn.in_transaction:
            conn.start_transaction()
        cursor = conn.cursor(dictionary=True)
        numar = plati.sterge_plata(cursor, id_op)
        conn.commit()
        return json_utf8({"idop": id_op, "nrop": numar}, 200)
    except plati.PlatiEroare as err:
        rollback(conn)
        return json_utf8({"error": str(err)}, err.status)
    except Exception as err:
        rollback(conn)
        logger.error("[avacont.facturi_plati] stergere %s: %s", db_name, err, exc_info=True)
        return json_utf8({"error": "Eroare la ștergerea plății: %s" % err}, 500)
    finally:
        if conn is not None:
            conn.close()

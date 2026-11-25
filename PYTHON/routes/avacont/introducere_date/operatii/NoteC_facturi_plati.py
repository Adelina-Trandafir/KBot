# routes/avacont/introducere_date/operatii/NoteC_facturi_plati.py
"""SLICE-AV08-04 (moved from facturi_plati.py, slice AV02-04): the SAVE of «Facturi si plati» -- the typed invoices of one partner (each one an evidence,
through NoteC.operatie_din_sablon) and / or the payment with its order of payment (NoteC.salveaza_plata), in the transaction of NoteC.salvare."""
from decimal import Decimal

from . import NoteC
from .NoteC_contari import date_unitate
from .NoteC_contari_ajutatoare import MAX_EXPLICATIE_OP, MIN_EXPLICATIE_OP, _clasificatii, _iso, _sabloane, text_op
from .NoteC_tranzactie import TREZORERIE_BIC, NoteCEroare


def _refuz(mesaj, status=400):
    return NoteCEroare(mesaj, "FACTURI_PLATI", status)


def salveaza_facturi_plati(cursor, db_name, user, rol, c):
    """c = the request read by facturi_plati._citeste_cerere. Returns {numarnc, note, an, evidente, plata}."""
    cu_plata = c["id_p"] is not None
    total = sum((f["platit"] for f in c["noi"]), Decimal(0)) + sum(c["existente"].values(), Decimal(0))
    if not c["noi"] and not (cu_plata and total != 0):
        raise _refuz("Nu este nimic de înregistrat: adăugați o factură nouă sau alegeți un șablon de plată și sume de plătit.")
    if c["noi"] and c["id_e"] is None:
        raise _refuz("Alegeți șablonul de evidențiere pentru facturile noi.")
    if cu_plata and total <= 0:
        raise _refuz("Suma de plată trebuie să fie pozitivă.", 409)
    if cu_plata and c["numar_nc"] <= 0:
        raise _refuz("Plata nu are numărul notei contabile (obligatoriu).")
    if not cu_plata and total != 0:
        raise _refuz("Alegeți șablonul de plată: sunt sume de plată pe facturi.")
    sid, an = c["sid"], c["an"]
    sub = NoteC.subunitate_pentru(cursor, None, sid)
    index = _sabloane(cursor, sid)
    clsf = next((x for x in _clasificatii(cursor, index) if x["IdClsf"] == c["id_clsf"] and x["SS"] == sub["SursaSector"]), None)
    if clsf is None:
        raise _refuz("Clasificația aleasă nu are șabloane de operații în subunitatea în lucru.", 409)
    slot = index[(clsf["Cheie"], clsf["SS"])]
    if c["id_e"] is not None and c["id_e"] not in {s["IdSablon"] for s in slot["E"]}:
        raise _refuz("Șablonul de evidențiere ales nu se potrivește clasificației.", 409)
    if c["id_p"] is not None and c["id_p"] not in {s["IdSablon"] for s in slot["P"]}:
        raise _refuz("Șablonul de plată ales nu se potrivește clasificației.", 409)
    partener = NoteC.partener_al_sursei(cursor, c["id_partener"], sub["SursaSector"], an)
    if partener is None:
        raise _refuz("Partenerul ales nu mai există.", 404)
    # the existing invoices: evidence operations of this subunit with a credit line of the partner; the rest is read under lock
    ids_oper = list(c["existente"])
    existente = {}
    if ids_oper:
        cursor.execute(
            "SELECT o.IdOper, d.NumarDocument, d.DataDocument FROM TZ_Oper o JOIN TZ_Documente d ON d.IdDocument = o.IdDocument JOIN TZ_NoteC n ON n.IdNota = d.IdNota "
            "WHERE o.IdOper IN (" + ", ".join(["%s"] * len(ids_oper)) + ") AND o.Tip = 'E' AND n.IdSubunitate = %s AND n.An IN (%s, %s) FOR UPDATE",
            (*ids_oper, sid, an, an - 1))
        existente = {r["IdOper"]: r for r in cursor.fetchall()}
        if len(existente) != len(ids_oper):
            raise _refuz("O factură aleasă nu mai există sau nu aparține subunității.", 409)
        valori = NoteC.valori_pe_operatii(cursor, ids_oper, c["id_partener"])
        rest = {i: v[2] for i, v in valori.items()}
        for id_oper in ids_oper:
            if c["existente"][id_oper] != 0 and not NoteC.in_limita(c["existente"][id_oper], rest[id_oper]):
                raise _refuz("Suma de plată a facturii %s depășește restul de plată (%s)." % (existente[id_oper]["NumarDocument"], rest[id_oper]), 409)
    # note
    inchidere = NoteC.data_ultimei_inchideri(cursor, sid, an)
    # SLICE-AV08-07, before anything is written: the dates are booked exactly as typed (a date out of the year or in a closed period is refused), then
    # Access 217 asks once when an invoice falls in a month after the newest operation
    for f in c["noi"]:
        NoteC.verifica_data_document(f["data"], an, inchidere, "Data facturii %s" % f["numar"])
    if cu_plata:
        NoteC.verifica_data_document(c["data_op"], an, inchidere, "Data ordinului de plată")
    NoteC.luna_noua(cursor, c["confirmari"], sid, an, [(None, f["data"], "Data facturii %s" % f["numar"]) for f in c["noi"]])
    note = {}

    def id_nota_pentru(destinatie):
        numar_nc = destinatie["numar_nc"]
        alegere = (destinatie["idnota"], destinatie["nota_noua"])
        if numar_nc not in note:
            id_nota, _ = NoteC.rezolva_nota(cursor, sid, an, user, numar_nc, *alegere)
            note[numar_nc] = (id_nota, alegere)
        id_nota, alegere_anterioara = note[numar_nc]
        if alegere != alegere_anterioara:
            raise _refuz("Aceeași notă contabilă are două destinații diferite în lot.", 409)
        return id_nota

    # evidence of the typed invoices (each in its own note)
    evidente, alocari = [], []
    if c["noi"]:
        sablon_e, linie_e = NoteC.sablonul(cursor, c["id_e"], clsf["Forma"], "evidențiere")
        for f in c["noi"]:
            data = f["data"]   # the date and the number exactly as typed (checked above)
            antet, debit, credit = NoteC.operatie_din_sablon(cursor, sid, sablon_e, linie_e, clsf, "E", data, c["id_partener"], f["suma"])
            _, id_oper = NoteC.document_si_operatie(cursor, sid, an, id_nota_pentru(f), f["numar"], data, f["fel"], "E", antet,
                                                    debit, credit, user)
            evidente.append({"numar": f["numar"], "idoper": id_oper})
            if f["platit"] != 0:
                alocari.append({"IdOper": id_oper, "NumarDocument": f["numar"], "DataDocument": data, "Valoare": f["suma"], "Platit": f["platit"]})
    for id_oper, platit in c["existente"].items():
        if platit != 0:
            r = existente[id_oper]
            alocari.append({"IdOper": id_oper, "NumarDocument": r["NumarDocument"], "DataDocument": r["DataDocument"], "Valoare": valori[id_oper][0], "Platit": platit})
    # payment and order
    plata = None
    if cu_plata:
        data_op = c["data_op"]   # checked above
        iban, coduri = NoteC.cont_iban_partener(cursor, c["id_partener"], c["id_clsf"])
        if not iban:
            raise _refuz("Nu se poate adăuga operația de plată: partenerul selectat nu are niciun cont bancar configurat!", 409)
        documente = [a["NumarDocument"] for a in alocari]
        sablon_p, linie_p = NoteC.sablonul(cursor, c["id_p"], clsf["Forma"], "plată")
        explicatie = c["explicatie_op"].strip() or " ".join(x for x in ["F " + " ".join(documente) if documente else "", sablon_p["FelOP"] or ""] if x)
        explicatie = text_op(explicatie, MAX_EXPLICATIE_OP)
        if len(explicatie) < MIN_EXPLICATIE_OP:
            raise _refuz("Explicația ordinului de plată trebuie să aibă între %d și %d caractere." % (MIN_EXPLICATIE_OP, MAX_EXPLICATIE_OP))
        den_unit, cf_unit, program = date_unitate(cursor, db_name, clsf["SS"], an)
        id_oper, id_op, numar, _doc = NoteC.salveaza_plata(cursor, {
            "sid": sid, "an_nota": an, "id_nota": id_nota_pentru(c), "data_op": data_op, "id_p": c["id_p"], "sablon": sablon_p, "linie": linie_p, "clsf": clsf,
            "protectie": c["protectie"], "confirmari_receptie": c["confirmari_receptie"],
            "rol": rol,
            "id_partener": c["id_partener"], "total": total, "user": user,
            "header": {"Explicatie": explicatie, "Documente": ";".join(documente), "DenPartener": partener["DenumirePartener"], "CFPartener": partener["CodFiscal"],
                       "ContBeneficiar": iban, "DenUnitate": den_unit, "CFUnitate": cf_unit, "pBIC": TREZORERIE_BIC,
                       "CodAngajament": (coduri or {}).get("CodAng") or None, "CodIndicator": (coduri or {}).get("CodInd") or None, "CodProgram": program}}, operatii=alocari)
        plata = {"idoper": id_oper, "idop": id_op, "nrop": numar, "valoare": float(total), "dataop": _iso(data_op)}
    numere_note = sorted(note)
    return {"numarnc": c["numar_nc"] if cu_plata else (numere_note[0] if numere_note else 0), "note": numere_note, "an": an,
            "evidente": evidente, "plata": plata}

# routes/avacont/introducere_date/operatii/NoteC_contari.py
"""SLICE-AV08-04 (moved from efactura/primite_contari.py and primite_plata.py; slices 01EF-04, 04EF-01): the SAVE of the «Contari» of received e-invoices,
the evidence and / or the payment, in ONE transaction of NoteC.salvare."""
import logging
from decimal import Decimal

from routes.efactura import primite, primite_ke
from routes.efactura.primite_parteneri import activeaza as activeaza_partener, leaga as leaga_partener
from routes.efactura.tokens import EfEroare

from . import NoteC
from .NoteC_contari_calcul import calcul_contari
from .NoteC_tranzactie import TREZORERIE_BIC
from .NoteC_contari_ajutatoare import (CONTARI_SQL, MAX_FACTURI, MODURI, MIN_EXPLICATIE_OP, MAX_EXPLICATIE_OP, _dec, _num, _GEMENI, text_op, numar_document, _iso, _ids, _opt_int, _marks, _facturi, _salvat, _sabloane, _clasificatii, _coduri, _sugestie, _parti, _linii_sablon, _simbol, _partener_text, _operatie)

logger = logging.getLogger(__name__)


def _cent(value):
    return primite_ke._cent(value)


def date_unitate(cursor, dc, ss, an):
    """(name, tax code, program code) of the unit for the order header."""
    party = primite._unit_party(cursor, dc)
    program = None
    try:
        cursor.execute("SELECT CodProgram FROM AVACONT_COMUN.Unitati_Ani WHERE DC = %s AND AN = %s AND SS = %s", (dc, an, ss))
        row = cursor.fetchone()
        program = row["CodProgram"] if row else None
    except Exception:   # the lookup only fills a header field; a missing row or table must not stop the payment, but it is logged
        logger.warning("[plata] CodProgram nu a putut fi citit pentru %s/%s/%s", dc, an, ss, exc_info=True)
    return party.get("nume") or "", party.get("cui") or "", program


def plata_e_facturi(cursor, dc, user, ctx):
    """Books the payment and its order. ctx: interne (clsf, partener, subunitate, facturi), sid, an_nota, id_nota, data_op, id_p, linie_p, sablon_p, explicatie_op,
    cont_iban, coduri, facturi_platite [{f, Platit}]. Returns {IdOper, IdOP, NrOP, IdDocument, Valoare, DataOP}."""
    clsf, partener, sid = ctx["clsf"], ctx["partener"], ctx["sid"]
    facturi = [dict(IdPrimita=f["IdPrimita"], NrFact=f["NrFact"], DataFact=f["DataFact"], Total=_cent(f["Total"]) * int(f["Semn"] or 1), Platit=_cent(p))
               for f, p in ctx["facturi_platite"] if _cent(p) != 0]
    total = sum((f["Platit"] for f in facturi), Decimal(0))
    if not facturi or total <= 0:
        raise EfEroare("Suma de plată trebuie să fie pozitivă.", "PLATA_NULA", 409)
    # the rest is read again here, in the transaction, with the evidence of the same save already visible: a payment never goes over what is evidenced and unpaid
    rest = NoteC.rest_de_plata(cursor, [f["IdPrimita"] for f in facturi])
    for f in facturi:
        r = rest[f["IdPrimita"]]
        if (r >= 0 and (f["Platit"] < 0 or f["Platit"] > r)) or (r < 0 and (f["Platit"] > 0 or f["Platit"] < r)):
            raise EfEroare("Suma de plată a facturii %s depășește restul de plată (%s)." % (f["NrFact"], r), "PLATA_PESTE_REST", 409)
    if not ctx["cont_iban"]:
        raise EfEroare("Nu se poate adăuga operația de plată: partenerul selectat nu are niciun cont bancar configurat!", "CONT_BANCAR_LIPSA", 409)
    explicatie = text_op(ctx["explicatie_op"], MAX_EXPLICATIE_OP)
    if len(explicatie) < MIN_EXPLICATIE_OP:
        raise EfEroare("Explicația ordinului de plată trebuie să aibă între %d și %d caractere." % (MIN_EXPLICATIE_OP, MAX_EXPLICATIE_OP), "EXPLICATIE_OP", 400)
    try:
        den_unit, cf_unit, program = date_unitate(cursor, dc, clsf["SS"], ctx["an_nota"])
        coduri = ctx.get("coduri") or {}
        id_oper, id_op, numar, id_doc = NoteC.salveaza_plata(cursor, {
            "sid": sid, "an_nota": ctx["an_nota"], "id_nota": ctx["id_nota"], "data_op": ctx["data_op"], "id_p": ctx["id_p"], "sablon": ctx["sablon_p"],
            "linie": ctx["linie_p"], "clsf": clsf, "id_partener": partener["IdPartener"], "total": total, "user": user,
            "header": {"Explicatie": explicatie, "Documente": ";".join(numar_document(f["NrFact"]) for f in facturi), "DenPartener": partener.get("Denumire"),
                       "CFPartener": ctx.get("cui"), "ContBeneficiar": ctx["cont_iban"], "DenUnitate": den_unit, "CFUnitate": cf_unit, "pBIC": TREZORERIE_BIC,
                       "CodAngajament": coduri.get("CodAng") or None, "CodIndicator": coduri.get("CodInd") or None, "CodProgram": program}}, e_facturi=facturi)
    except (NoteC.NoteCEroare, NoteC.PlatiEroare) as err:
        raise EfEroare(str(err), err.reason, err.status) from err
    return {"IdOper": id_oper, "IdOP": id_op, "NrOP": numar, "IdDocument": id_doc, "Valoare": float(total), "DataOP": ctx["data_op"].isoformat()}


# ---------------------------------------------------------------------------------------------
# the save of the EVIDENCE (01EF-04)
# ---------------------------------------------------------------------------------------------
def salveaza_contari(dc, body, role, user):
    try:
        return _salveaza(dc, body, role, user)
    except (NoteC.NoteCEroare, NoteC.PlatiEroare) as err:
        raise EfEroare(str(err), err.reason, err.status) from err


def _salveaza(dc, body, role, user):
    """Books, through NoteC and in ONE transaction, the EVIDENCE and/or the PAYMENT of the ticked invoices (`mod`: evidentiere | plata | ambele).
    Evidence: one accounting note, a document per invoice, an operation per template line with its debit and credit, and a row of EF_PrimiteOperatii that ties the
    operation to the invoice. Payment (04EF-01, primite_plata.py): one document «OP», one payment operation, one order of payment, the allocations on the invoices.
    body: the same fields as the calculation plus `confirmari` (the codes of the warnings the operator confirmed) and, for a payment, `dataOP`, `explicatieOP`.
    Role «Contabil» only (checked here)."""
    primite_ke.cere_contabil(role)
    from routes.efactura import primite_plata
    confirmari = body.get("confirmari") or []
    if not isinstance(confirmari, list) or any(not isinstance(c, str) for c in confirmari):
        raise EfEroare("Câmpul «confirmari» nu este valid.", "CAMP_INVALID", 400)
    mod = body.get("mod") or "evidentiere"
    if mod not in MODURI:
        raise EfEroare("Câmpul «mod» nu este valid.", "CAMP_INVALID", 400)
    cu_evidenta, cu_plata = mod != "plata", mod != "evidentiere"
    an = _opt_int(body, "an")
    numar_cerut = _opt_int(body, "numarNC")
    data_op = primite_plata.data_ordin(body) if cu_plata else None
    with NoteC.salvare(dc) as conn:
        interne = {}
        r = calcul_contari(dc, body, role, conn=conn, interne=interne, blocheaza=True, fara_plata=not cu_plata)
        clsf, id_e, id_p, sub = interne.get("clsf"), interne.get("id_e"), interne.get("id_p"), interne.get("subunitate")
        if clsf is None or (cu_evidenta and not id_e):
            raise EfEroare("Alegeți clasificația și șablonul de evidențiere.", "ALEGERE_INCOMPLETA", 400)
        if cu_plata and not id_p:
            raise EfEroare("Alegeți șablonul de plată.", "ALEGERE_INCOMPLETA", 400)
        for a in r["avertismente"]:
            if a["Grav"] == "eroare":
                raise EfEroare(a["Text"], "AVERTISMENT_BLOCANT", 409)
        necesare = [a for a in r["avertismente"] if a["Grav"] == "confirmare" and a["Cod"] not in confirmari]
        if necesare:
            err = EfEroare("Confirmați avertismentele înainte de salvare.", "CONFIRMARE_NECESARA", 409)
            err.extra = {"confirmari_necesare": [{"Cod": a["Cod"], "Text": a["Text"]} for a in necesare]}
            raise err
        randuri = [(f, interne["ramas"][f["IdPrimita"]]) for f in interne["facturi"] if primite_ke._cent(interne["ramas"][f["IdPrimita"]]) != 0] if cu_evidenta else []
        asteptata = body.get("evidentaAsteptata")
        if cu_evidenta and asteptata is not None:
            if not isinstance(asteptata, dict):
                raise EfEroare("Valorile evidențierii nu sunt valide.", "CAMP_INVALID", 400)
            actuala = {str(i): primite_ke._cent(suma) for i, suma in interne["ramas"].items()}
            if set(asteptata) != set(actuala) or any(_dec(asteptata[i]) != suma for i, suma in actuala.items()):
                raise EfEroare("Sumele rămase de evidențiat s-au schimbat. Recalculați înainte de salvare.", "EVIDENTA_SCHIMBATA", 409)
        platite = [(f, interne["platit"][f["IdPrimita"]]) for f in interne["facturi"] if interne["platit"][f["IdPrimita"]] != 0] if cu_plata else []
        if sub is None or (mod == "evidentiere" and not randuri) or (mod == "plata" and not platite) or (mod == "ambele" and not randuri and not platite):
            raise EfEroare("Nimic de înregistrat: facturile alese sunt deja înregistrate integral." if not cu_plata
                           else "Nimic de plătit: nicio factură aleasă nu are sumă de plată.", "NIMIC_DE_INREGISTRAT", 409)
        cursor = conn.cursor(dictionary=True)
        ani = {interne["date_doc"][f["IdPrimita"]].year for f, _ in randuri}
        if cu_plata:
            inchidere = NoteC.data_ultimei_inchideri(cursor, sub["IdSubunitate"], data_op.year)
            # SLICE-AV08-07 (Access EF_P, message 362): a date in a closed period is refused, never moved; the work year is checked just below (AN_DIFERIT)
            NoteC.verifica_data_document(data_op, data_op.year, inchidere, "Data ordinului de plată")
            ani.add(data_op.year)
        an_nota = an or max(ani)
        if ani - {an_nota}:
            raise EfEroare("Data unui document iese din anul de lucru (%d). Alegeți alt an sau corectați data facturii/plății." % an_nota, "AN_DIFERIT", 409)
        operatii, plata = [], None
        try:
            sid = sub["IdSubunitate"]
            # the note the operator chose: an existing one takes the new documents, a number not used yet makes a new note with that number, none = the next number
            id_nota, numar_nota = NoteC.rezolva_nota(cursor, sid, an_nota, user, numar_cerut, _opt_int(body, "idNota"), body.get("notaNoua"))
            partener = interne["partener"]
            activeaza_partener(cursor, partener["IdPartener"])   # the partner the booking is made for enters the flow (not hidden) and is kept for the tax code, in this transaction
            leaga_partener(cursor, interne["facturi"][0]["CuiNormalizat"], partener["IdPartener"], user)
            if randuri:
                sablon, linie = NoteC.sablonul(cursor, id_e, clsf["Forma"], "evidențiere")
                for f, ramas in randuri:
                    suma = primite_ke._cent(ramas)
                    data = interne["date_doc"][f["IdPrimita"]]
                    antet, debit, credit = NoteC.operatie_din_sablon(cursor, sid, sablon, linie, clsf, "E", data, partener["IdPartener"], suma)
                    id_doc, id_oper = NoteC.document_si_operatie(
                        cursor, sid, an_nota, id_nota, numar_document(f["NrFact"]), data, "Notă credit" if f["Tip"] == "NC" else "Factură", "E", antet,
                        debit, credit, user)
                    NoteC.leaga_evidentiere(cursor, f["IdPrimita"], id_oper, clsf, suma)
                    operatii.append({"IdPrimita": f["IdPrimita"], "IdDocument": id_doc, "IdOper": id_oper})
            if platite:
                sablon_p, linie_p = NoteC.sablonul(cursor, id_p, clsf["Forma"], "plată")
                cerut = body.get("explicatieOP")
                explicatie = cerut if isinstance(cerut, str) and cerut.strip() else interne["explicatie_op"]
                plata = plata_e_facturi(cursor, dc, user, {
                    "clsf": clsf, "partener": partener, "sid": sid, "an_nota": an_nota, "id_nota": id_nota, "data_op": data_op, "id_p": id_p, "linie_p": linie_p,
                    "sablon_p": sablon_p, "explicatie_op": explicatie, "cont_iban": interne["cont_iban"], "coduri": interne["coduri"], "cui": interne["cui"],
                    "facturi_platite": platite})
        except NoteC.NoteCEroare as err:
            raise EfEroare(str(err), err.reason, err.status) from err
    return {"IdNota": id_nota, "NumarNC": numar_nota, "An": an_nota, "Subunitate": sub["Denumire"], "Operatii": operatii, "Plata": plata}


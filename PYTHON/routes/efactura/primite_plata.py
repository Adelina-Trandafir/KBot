# routes/efactura/primite_plata.py
"""
04EF-01: the PAYMENT of received invoices from «Contari» (the part of primite_contari.salveaza that runs after the evidence, or alone).

One payment = one accounting operation (the payment template, e.g. 401 = 770) = one order of payment (OP) = one document «OP» whose number is the number of the OP.
The allocation of the payment on each invoice is kept in TZ_PlatiFacturi (never in EF_PrimiteOperatii, which holds evidence only). Everything runs in the
CALLER'S transaction: the cursor comes from `salveaza`, nothing is committed here.
"""
import logging
from datetime import date, datetime
from decimal import Decimal

from . import primite
from .tokens import EfEroare

logger = logging.getLogger(__name__)

TREZORERIE_BIC = "TREZROBU"   # the payer bank of every order (Access SAV_OP_2017: «TREZROBU»)


def _cent(value):
    from . import primite_ke
    return primite_ke._cent(value)


def data_ordin(body):
    """The date of the payment/OP: `dataOP` (ISO) or today."""
    raw = body.get("dataOP")
    if raw is None:
        return date.today()
    try:
        return datetime.strptime(str(raw)[:10], "%Y-%m-%d").date()
    except ValueError as err:
        raise EfEroare("Data ordinului de plată nu este validă.", "DATA_OP_INVALIDA", 400) from err


def _date_unitate(cursor, dc, ss, an):
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


def posteaza(cursor, dc, user, ctx):
    """Books the payment and its order. ctx: interne (clsf, partener, subunitate, facturi), sid, an_nota, id_nota, data_op, id_p, linie_p, sablon_p, explicatie_op,
    cont_iban, coduri, facturi_platite [{f, Platit}]. Returns {IdOper, IdOP, NrOP, IdDocument, Valoare, DataOP}."""
    from routes.avacont.introducere_date.operatii import plati, postare
    from . import primite_contari as C
    clsf, partener, sid = ctx["clsf"], ctx["partener"], ctx["sid"]
    facturi = [dict(IdPrimita=f["IdPrimita"], NrFact=f["NrFact"], DataFact=f["DataFact"], Total=_cent(f["Total"]) * int(f["Semn"] or 1), Platit=_cent(p))
               for f, p in ctx["facturi_platite"] if _cent(p) != 0]
    total = sum((f["Platit"] for f in facturi), Decimal(0))
    if not facturi or total <= 0:
        raise EfEroare("Suma de plată trebuie să fie pozitivă.", "PLATA_NULA", 409)
    # the rest is read again here, in the transaction, with the evidence of the same save already visible: a payment never goes over what is evidenced and unpaid
    rest = plati.rest_de_plata(cursor, [f["IdPrimita"] for f in facturi])
    for f in facturi:
        r = rest[f["IdPrimita"]]
        if (r >= 0 and (f["Platit"] < 0 or f["Platit"] > r)) or (r < 0 and (f["Platit"] > 0 or f["Platit"] < r)):
            raise EfEroare("Suma de plată a facturii %s depășește restul de plată (%s)." % (f["NrFact"], r), "PLATA_PESTE_REST", 409)
    if not ctx["cont_iban"]:
        raise EfEroare("Nu se poate adăuga operația de plată: partenerul selectat nu are niciun cont bancar configurat!", "CONT_BANCAR_LIPSA", 409)
    explicatie = C.text_op(ctx["explicatie_op"], C.MAX_EXPLICATIE_OP)
    if len(explicatie) < C.MIN_EXPLICATIE_OP:
        raise EfEroare("Explicația ordinului de plată trebuie să aibă între %d și %d caractere." % (C.MIN_EXPLICATIE_OP, C.MAX_EXPLICATIE_OP), "EXPLICATIE_OP", 400)
    try:
        den_unit, cf_unit, program = _date_unitate(cursor, dc, clsf["SS"], ctx["an_nota"])
        coduri = ctx.get("coduri") or {}
        id_oper, id_op, numar, id_doc = plati.posteaza_ordin(cursor, {
            "sid": sid, "an_nota": ctx["an_nota"], "id_nota": ctx["id_nota"], "data_op": ctx["data_op"], "id_p": ctx["id_p"], "sablon": ctx["sablon_p"],
            "linie": ctx["linie_p"], "clsf": clsf, "id_partener": partener["IdPartener"], "total": total, "user": user,
            "header": {"Explicatie": explicatie, "Documente": ";".join(C.numar_document(f["NrFact"]) for f in facturi), "DenPartener": partener.get("Denumire"),
                       "CFPartener": ctx.get("cui"), "ContBeneficiar": ctx["cont_iban"], "DenUnitate": den_unit, "CFUnitate": cf_unit, "pBIC": TREZORERIE_BIC,
                       "CodAngajament": coduri.get("CodAng") or None, "CodIndicator": coduri.get("CodInd") or None, "CodProgram": program}})
        plati.aloca_pe_evidentieri(cursor, id_oper, id_op, facturi, ctx["data_op"], total, user)
    except (postare.PostareEroare, plati.PlatiEroare) as err:
        raise EfEroare(str(err), err.reason, err.status) from err
    return {"IdOper": id_oper, "IdOP": id_op, "NrOP": numar, "IdDocument": id_doc, "Valoare": float(total), "DataOP": ctx["data_op"].isoformat()}


def sterge(dc, id_op, user):
    """Deletes an UNLOCKED order with its payment (04EF-01). Locked (in an OPME or confirmed by a statement): refused."""
    from routes.avacont.introducere_date.operatii import plati
    from . import facturi as F
    from . import primite_contari as C
    with F._unit(dc, C.CONTARI_SQL) as conn:
        cursor = conn.cursor(dictionary=True)
        try:
            numar = plati.sterge_plata(cursor, id_op)
        except plati.PlatiEroare as err:
            raise EfEroare(str(err), err.reason, err.status) from err
        conn.commit()
    return {"IdOP": id_op, "NrOP": numar}

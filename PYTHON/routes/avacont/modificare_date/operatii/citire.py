# routes/avacont/modificare_date/operatii/citire.py
"""
Reading the posted operations for «Modificare operatii» (slice AV02-05, the Access form ModificareNC1280 / query qMNC2025). No Flask here.

ONE row per TZ_Oper: every join below is on a key that gives at most one row (TZ_Documente, TZ_NoteC, Clasificatii, TZ_Jurnale by primary key;
TZ_OPuri by its UNIQUE IdOper; FX_ORD_TBL / FX_ORD by primary key), and the debit / credit side is read through its FIRST line (the amount is the
sum of the side). Access joined OPuri and PlatiFacturi without grouping and showed an operation twice when an invoice was paid in two payments.

The same SELECT feeds the list and the delete checks, so a row the list shows as deletable is checked by the same rule on delete.
"""
from datetime import date

from ...introducere_date.operatii import postare

FEL_INCHIDERE = "I"

SQL_OPERATII = (
    "SELECT o.IdOper, o.IdDocument, n.IdNota, n.IdSubunitate, n.An, n.NumarNC, d.NumarDocument, d.DataDocument, d.FelDocument, d.Fel AS FelDoc, "
    "o.DataOperatie, o.Explicatie, o.Chelt, o.Cfpp, o.IdOrdTbl, o.Tip, c.Clsf, j.Cod AS Jurnal, "
    "(SELECT SUM(x.Suma) FROM TZ_Debit x WHERE x.IdOper = o.IdOper) AS SumaDebit, "
    "(SELECT COUNT(*) FROM TZ_Debit x WHERE x.IdOper = o.IdOper) AS LiniiDebit, "
    "pd.SimbolCont AS ContDebit, ppd.CodPartener AS PartDebit, dl.IdPartener AS IdPartDebit, "
    "(SELECT SUM(x.Suma) FROM TZ_Credit x WHERE x.IdOper = o.IdOper) AS SumaCredit, "
    "(SELECT COUNT(*) FROM TZ_Credit x WHERE x.IdOper = o.IdOper) AS LiniiCredit, "
    "pc.SimbolCont AS ContCredit, ppc.CodPartener AS PartCredit, cl.IdPartener AS IdPartCredit, "
    "ot.IDORDTBLP AS OrdRand, ord.Semnatura AS OrdSemnatura, "
    "op.IdOP, op.NrOP, op.DataOP, op.IdOPME, "
    "(CASE WHEN op.IdOP IS NULL THEN 0 WHEN EXISTS (SELECT 1 FROM FX_Extrase x WHERE x.IDOP = COALESCE(op.IdOPAccess, op.IdOP)) THEN 1 ELSE 0 END) AS OPExtras, "
    "(SELECT COUNT(*) FROM TZ_PlatiFacturi pf WHERE pf.IdEvidentiere = o.IdOper) AS PlatiPeEvidenta, "
    "(SELECT COUNT(*) FROM TZ_PlatiFacturi pf WHERE pf.IdPlata = o.IdOper) AS FacturiPlatite "
    "FROM TZ_Oper o "
    "JOIN TZ_Documente d ON d.IdDocument = o.IdDocument "
    "JOIN TZ_NoteC n ON n.IdNota = d.IdNota "
    "LEFT JOIN Clasificatii c ON c.IDClsf = o.IdClsf "
    "LEFT JOIN TZ_Jurnale j ON j.IdJurnal = o.IdJurnal "
    "LEFT JOIN TZ_Debit dl ON dl.IdLinie = (SELECT MIN(x.IdLinie) FROM TZ_Debit x WHERE x.IdOper = o.IdOper) "
    "LEFT JOIN TZ_PlanCont pd ON pd.IdCont = dl.IdCont "
    "LEFT JOIN Parteneri ppd ON ppd.IdPartener = dl.IdPartener "
    "LEFT JOIN TZ_Credit cl ON cl.IdLinie = (SELECT MIN(x.IdLinie) FROM TZ_Credit x WHERE x.IdOper = o.IdOper) "
    "LEFT JOIN TZ_PlanCont pc ON pc.IdCont = cl.IdCont "
    "LEFT JOIN Parteneri ppc ON ppc.IdPartener = cl.IdPartener "
    "LEFT JOIN FX_ORD_TBL ot ON ot.IDORDTBLP = o.IdOrdTbl "
    "LEFT JOIN FX_ORD ord ON ord.IDORDP = ot.IDORDP "
    "LEFT JOIN TZ_OPuri op ON op.IdOper = o.IdOper ")


def operatii(cursor, where, params):
    """The rows of SQL_OPERATII for a WHERE clause (written by the caller, with %s placeholders)."""
    cursor.execute(SQL_OPERATII + "WHERE " + where, params)
    return cursor.fetchall()


def cont_text(simbol, cod_partener, linii):
    """The account as Access showed it: «simbol» or «simbol.PCOD»; «(+n)» when the side has more lines than the first one."""
    if simbol is None:
        return ""
    text = simbol + ((".P" + str(cod_partener).upper()) if cod_partener else "")
    if linii and int(linii) > 1:
        text += " (+%d)" % (int(linii) - 1)
    return text


def stare_ord(row):
    """The stage of the ordonantare of the operation: '' none, 'G' generated (row in FX_ORD_TBL), then the last signature reached:
    'AB', 'CD', 'O' (Ordonator). The roles are the ones the server stores in FX_ORD.Semnatura (routes/forexe/pdf.py: AB, CD, Ordonator)."""
    if row["IdOrdTbl"] is None or row["OrdRand"] is None:
        return ""
    roluri = {r.strip() for r in (row["OrdSemnatura"] or "").split(",") if r.strip()}
    if "Ordonator" in roluri:
        return "O"
    if "CD" in roluri:
        return "CD"
    if "AB" in roluri:
        return "AB"
    return "G"


def facturi_ale_operatiilor(cursor, where, params):
    """{IdOper: set(IdPrimita)}: the received invoices behind each operation of the filter. An evidence reaches its invoice through
    EF_PrimiteOperatii, a payment through the invoices it paid (TZ_PlatiFacturi.IdPrimita; an invoice typed by hand has none)."""
    out = {}
    baza = ("FROM TZ_Oper o JOIN TZ_Documente d ON d.IdDocument = o.IdDocument JOIN TZ_NoteC n ON n.IdNota = d.IdNota ")
    cursor.execute("SELECT e.IdOperatie AS IdOper, e.IdPrimita " + baza + "JOIN EF_PrimiteOperatii e ON e.IdOperatie = o.IdOper WHERE " + where, params)
    for r in cursor.fetchall():
        out.setdefault(int(r["IdOper"]), set()).add(int(r["IdPrimita"]))
    cursor.execute("SELECT pf.IdPlata AS IdOper, pf.IdPrimita " + baza + "JOIN TZ_PlatiFacturi pf ON pf.IdPlata = o.IdOper "
                   "WHERE pf.IdPrimita IS NOT NULL AND " + where, params)
    for r in cursor.fetchall():
        out.setdefault(int(r["IdOper"]), set()).add(int(r["IdPrimita"]))
    return out


def stare_receptie(facturi, cu_receptie):
    """'' when the operation has no invoice or none of its invoices has a reception, 'T' when ALL have one, 'P' when only SOME have."""
    if not facturi:
        return ""
    cu = sum(1 for f in facturi if f in cu_receptie)
    if cu == 0:
        return ""
    return "T" if cu == len(facturi) else "P"


def motiv_refuz(row, inchidere, azi=None):
    """None when the operation can be deleted, else why not (Romanian, the operator reads it). The rules of the Access buttons
    «Sterge operatie» / «Sterge Document» (ModificareNC1280: STO_Click, STD_Click; mdl_Salvare_NoteContabile: StergeOperatie2020), with the gaps closed:
    the same checks for a document, plus the ordonantare and the locked order of payment."""
    azi = azi or date.today()
    if (row["FelDoc"] or "").upper() == FEL_INCHIDERE:
        return "Operațiile de închidere nu se șterg de aici."
    if (row["NumarDocument"] or "").upper().startswith("CCV"):
        return "Nu se poate șterge o operație din nota de închidere."
    data = row["DataDocument"]
    if inchidere is not None and data is not None and data <= inchidere:
        return "Operația face parte dintr-o lună închisă (ultima închidere: %s)." % inchidere.strftime("%d.%m.%Y")
    if data is not None and data.year < azi.year and azi.month > 1:
        return "Operația este dintr-un an anterior, care este închis."
    if stare_ord(row):
        return "Operația are o ordonanțare generată. Ștergeți întâi ordonanțarea."
    if row["PlatiPeEvidenta"]:
        return "Operația selectată este plătită! NU SE POATE ȘTERGE!"
    if row["IdOP"] is not None:
        if row["IdOPME"] is not None:
            return "Ordinul de plată %s este adăugat într-un OPME." % row["NrOP"]
        if row["OPExtras"]:
            return "Ordinul de plată %s este confirmat printr-un extras de cont." % row["NrOP"]
    return None


def inchideri(cursor, perechi):
    """{(IdSubunitate, An): date of the last closing | None} for the pairs met in the rows (postare.data_ultimei_inchideri, read once per pair)."""
    return {p: postare.data_ultimei_inchideri(cursor, p[0], p[1]) for p in perechi}

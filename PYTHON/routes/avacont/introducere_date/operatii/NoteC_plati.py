# routes/avacont/introducere_date/operatii/NoteC_plati.py
"""
Payments, orders of payment (OP) and the settings of the accounting (slice AV02-02; first caller: 04EF-01, the payment from «Contari»).

Like NoteC it takes a cursor from the CALLER'S transaction (never commits, never opens a connection) and has no Flask in it.

Rules kept here (the database cannot say them across rows):
- SETTINGS (TZ_Settings): the row of the subunit wins, else the row of the whole unit (IdSubunitate NULL), else the default written here.
- The number of an OP is the highest NrOP + 1. PlajaComunaOPuri ON: the highest of ALL the subunits; OFF (the default): of the issuing subunit only.
  Before saving the number is only a PROVISIONAL one (nothing locked); on save it is read again under lock and the real one goes back to the caller.
- An OP is LOCKED when it sits in an OPME or a bank statement line confirms it (FX_Extrase.IDOP). A locked OP and its payment can be neither edited nor deleted.
- An invoice is paid up to what is evidenced and not yet allocated: rest = evidence - SUM(TZ_PlatiFacturi.Platit); payments never go into EF_PrimiteOperatii.
"""
import logging
from decimal import Decimal, ROUND_HALF_UP

from . import NoteC

logger = logging.getLogger(__name__)

from .NoteC_tranzactie import CENT  # noqa: E402  (one copy of the constant)
PLAJA_COMUNA = "PlajaComunaOPuri"
IMPLICITE = {PLAJA_COMUNA: "0"}


class PlatiEroare(Exception):
    """A refusal the operator can act on: `reason` is a code, the text is Romanian."""

    def __init__(self, message, reason, status=409):
        super().__init__(message)
        self.reason = reason
        self.status = status


def _dec(value):
    return Decimal(str(value)) if value is not None else Decimal(0)


def cent(value):
    return _dec(value).quantize(CENT, rounding=ROUND_HALF_UP)


def _marks(n):
    return ", ".join(["%s"] * n)


# ---------------------------------------------------------------------------------------------
# settings
# ---------------------------------------------------------------------------------------------
def citeste_setare(cursor, cheie, id_subunitate=None):
    """The value (text) of a setting for a subunit: its own row, else the row of the unit, else the default of the code."""
    if cheie not in IMPLICITE:
        raise ValueError("Setare necunoscuta: %s" % cheie)
    cursor.execute("SELECT Valoare FROM TZ_Settings WHERE SettingKey = %s AND IdSubunitateCheie = %s", (cheie, id_subunitate or 0))
    row = cursor.fetchone()
    if row is None and id_subunitate:
        cursor.execute("SELECT Valoare FROM TZ_Settings WHERE SettingKey = %s AND IdSubunitateCheie = 0", (cheie,))
        row = cursor.fetchone()
    return row["Valoare"] if row else IMPLICITE[cheie]


def plaja_comuna(cursor, id_subunitate):
    return citeste_setare(cursor, PLAJA_COMUNA, id_subunitate).strip() in ("1", "true", "True")


def scrie_setare(cursor, cheie, id_subunitate, valoare):
    """Writes (or replaces) one setting row; the generated column makes a second row for the same key and subunit impossible. id_subunitate None = the whole unit."""
    if cheie not in IMPLICITE:
        raise ValueError("Setare necunoscuta: %s" % cheie)
    cursor.execute("INSERT INTO TZ_Settings (SettingKey, IdSubunitate, Valoare) VALUES (%s, %s, %s) "
                   "ON DUPLICATE KEY UPDATE Valoare = VALUES(Valoare), Version = Version + 1", (cheie, id_subunitate, str(valoare)))


# ---------------------------------------------------------------------------------------------
# numbering
# ---------------------------------------------------------------------------------------------
def numar_op_urmator(cursor, id_subunitate, blocheaza=False):
    """Highest NrOP + 1 by PlajaComunaOPuri. With `blocheaza` the rows read are locked (the real number, inside the save); without, it is only a provisional number."""
    comuna = plaja_comuna(cursor, id_subunitate)
    sql = "SELECT COALESCE(MAX(NrOP), 0) + 1 AS N FROM TZ_OPuri" + ("" if comuna else " WHERE IdSubunitate = %s")
    cursor.execute(sql + (" FOR UPDATE" if blocheaza else ""), () if comuna else (id_subunitate,))
    return int(cursor.fetchone()["N"])


# ---------------------------------------------------------------------------------------------
# the lock of an OP
# ---------------------------------------------------------------------------------------------
def motiv_blocare(cursor, id_op):
    """None when the OP can still be changed, else the reason in Romanian: it sits in an OPME, or a bank statement line confirms it."""
    cursor.execute("SELECT IdOP, IdOPME, IdOPAccess FROM TZ_OPuri WHERE IdOP = %s", (id_op,))
    op = cursor.fetchone()
    if op is None:
        raise PlatiEroare("Ordinul de plată nu mai există.", "OP_INEXISTENT", 404)
    if op["IdOPME"] is not None:
        return "Ordinul de plată este adăugat într-un OPME."
    # a migrated order is found in the statement by its Access id, a new one by its own id
    cursor.execute("SELECT 1 AS X FROM FX_Extrase WHERE IDOP = %s LIMIT 1", (op["IdOPAccess"] if op["IdOPAccess"] is not None else op["IdOP"],))
    if cursor.fetchone() is not None:
        return "Ordinul de plată este confirmat printr-un extras de cont."
    return None


# ---------------------------------------------------------------------------------------------
# what is evidenced, allocated and left to pay
# ---------------------------------------------------------------------------------------------
def operatii_legate(cursor, ids_primita):
    """{IdPrimita: [{Legatura: the EF_PrimiteOperatii row, Oper: its TZ_Oper row | None}]}, in the order the links were made.

    One plain key: EF_PrimiteOperatii.IdOperatie = TZ_Oper.IdOper (sql/00EF_66; the Access number lives in EF_PrimiteOperatii.IdOperatieAccess and is only used by the
    migration to fill IdOperatie). No TZ_Oper row for the key (IdOperatie 0 or a deleted operation) = Oper None."""
    out = {i: [] for i in ids_primita}
    if not ids_primita:
        return out
    cursor.execute(
        "SELECT e.IdOperatiePrimita, e.IdPrimita, e.IdOperatie, e.IdClsf, e.IdUnitate, e.Suma, o.IdOper, o.IdSablon, o.Explicatie, o.Tip AS TipOp, o.DataOperatie, o.IdClsf AS IdClsfOp, "
        "d.NumarDocument, d.DataDocument, d.FelDocument, d.Fel AS FelDoc, n.NumarNC, n.DataNc, n.IdSubunitate, s.Denumire AS Subunitate, c.Clsf, c.Denumire AS DenClsf, c.ClsfX, c.SS "
        "FROM EF_PrimiteOperatii e LEFT JOIN TZ_Oper o ON o.IdOper = e.IdOperatie LEFT JOIN TZ_Documente d ON d.IdDocument = o.IdDocument "
        "LEFT JOIN TZ_NoteC n ON n.IdNota = d.IdNota LEFT JOIN TZ_Subunitati s ON s.IdSubunitate = n.IdSubunitate "
        "LEFT JOIN Clasificatii c ON c.IDClsf = COALESCE(o.IdClsf, e.IdClsf) "
        "WHERE e.IdPrimita IN (" + _marks(len(ids_primita)) + ") ORDER BY e.IdPrimita, e.IdOperatiePrimita", tuple(ids_primita))
    for r in cursor.fetchall():
        out[r["IdPrimita"]].append({"Legatura": r, "Oper": r if r["IdOper"] is not None else None})
    return out


def evidentieri(cursor, ids_primita, fara_legatura=None):
    """{IdPrimita: [{IdOper | None, Suma}]}: the evidence rows of the invoices, WITHOUT payment operations (a migrated payment row is not evidence).
    A link with no TZ_Oper row under its key still counts (IdOper None): the row says the amount was evidenced."""
    out = {i: [] for i in ids_primita}
    for i, randuri in operatii_legate(cursor, ids_primita).items():
        for r in randuri:
            if r["Legatura"]["IdOperatiePrimita"] == fara_legatura:
                continue
            op = r["Oper"]
            if op is not None and (op["TipOp"] == "P" or op["FelDoc"] == "P"):
                continue
            out[i].append({"IdOper": op["IdOper"] if op else None, "Suma": cent(r["Legatura"]["Suma"])})
    return out


def alocat(cursor, ids_primita, fara_plata=None):
    """{IdPrimita: amount already allocated on it}. `fara_plata` = the payment operation to leave out (an edit)."""
    if not ids_primita:
        return {}
    marks = _marks(len(ids_primita))
    # Legacy AVACONT allocations may have only IdEvidentiere. Count them by the exact, unique invoice link.
    cursor.execute("SELECT DISTINCT a.IdEvidentiere FROM TZ_PlatiFacturi a JOIN EF_PrimiteOperatii e ON e.IdOperatie = a.IdEvidentiere "
                   "WHERE a.IdPrimita IS NULL AND e.IdPrimita IN (" + marks + ")", tuple(ids_primita))
    primite_pe_evidentieri(cursor, [r["IdEvidentiere"] for r in cursor.fetchall()])
    sql = ("SELECT COALESCE(a.IdPrimita, e.IdPrimita) AS IdFactura, SUM(a.Platit) AS S FROM TZ_PlatiFacturi a LEFT JOIN "
           "(SELECT IdOperatie, MIN(IdPrimita) AS IdPrimita FROM EF_PrimiteOperatii "
           "WHERE IdOperatie > 0 GROUP BY IdOperatie HAVING COUNT(DISTINCT IdPrimita) = 1) e "
           "ON a.IdPrimita IS NULL AND e.IdOperatie = a.IdEvidentiere "
           "WHERE (a.IdPrimita IN (" + marks + ") OR (a.IdPrimita IS NULL AND e.IdPrimita IN (" + marks + ")))")
    params = list(ids_primita) * 2
    if fara_plata is not None:
        sql += " AND a.IdPlata <> %s"
        params.append(fara_plata)
    cursor.execute(sql + " GROUP BY IdFactura", tuple(params))
    return {r["IdFactura"]: cent(r["S"]) for r in cursor.fetchall()}


def primite_pe_evidentieri(cursor, ids_oper, strict=True):
    if not ids_oper:
        return {}
    cursor.execute("SELECT IdOperatie, MIN(IdPrimita) AS IdPrimita, COUNT(DISTINCT IdPrimita) AS N, SUM(Suma) AS Suma "
                   "FROM EF_PrimiteOperatii WHERE IdOperatie IN (" + _marks(len(ids_oper)) + ") GROUP BY IdOperatie", tuple(ids_oper))
    rezultat = {}
    for rand in cursor.fetchall():
        if rand["N"] != 1:
            if strict:
                raise PlatiEroare("Evidențierea %s are mai multe e-facturi. Selectați și plătiți factura exactă în Contări." % rand["IdOperatie"], "FACTURA_AMBIGUA")
            rezultat[rand["IdOperatie"]] = (None, Decimal(0))
        else:
            rezultat[rand["IdOperatie"]] = (rand["IdPrimita"], cent(rand["Suma"]))
    return rezultat


def in_limita(suma, rest):
    return (0 <= suma <= rest) if rest >= 0 else (rest <= suma <= 0)


def limita_comuna(a, b):
    if a > 0 and b > 0:
        return min(a, b)
    if a < 0 and b < 0:
        return max(a, b)
    return Decimal(0)


def rest_de_plata(cursor, ids_primita, evidente_noi=None, fara_plata=None):
    """{IdPrimita: rest to pay}: evidenced (what the database holds + `evidente_noi` {IdPrimita: amount} posted in the same transaction) minus allocated."""
    ev = evidentieri(cursor, ids_primita)
    al = alocat(cursor, ids_primita, fara_plata)
    out = {}
    for i in ids_primita:
        total = sum((e["Suma"] for e in ev.get(i, [])), Decimal(0)) + cent((evidente_noi or {}).get(i, 0))
        out[i] = total - al.get(i, Decimal(0))
    return out


def are_receptie(cursor, id_primita):
    """True when the invoice has a link to a reception that still exists (EF_PrimiteReceptii -> FX_Receptii_R, not deleted; the old links go through IDRH)."""
    cursor.execute(
        "SELECT 1 AS X FROM EF_PrimiteReceptii l JOIN FX_Receptii_R R ON R.IDRR = COALESCE(l.IDRR, (SELECT H.IDRR FROM FX_Receptii_H H WHERE H.IDRH = l.IDRH LIMIT 1)) "
        "WHERE l.IdPrimita = %s AND COALESCE(R.Sters, 0) = 0 LIMIT 1", (id_primita,))
    return cursor.fetchone() is not None


def primite_cu_receptie(cursor, ids_primita, bucata=500):
    """The set of the invoices (among ids_primita) that have a reception: the same rule as are_receptie, read for many invoices at once (slice AV02-05)."""
    ids = sorted({int(i) for i in ids_primita if i is not None})
    out = set()
    for start in range(0, len(ids), bucata):
        parte = ids[start:start + bucata]
        cursor.execute(
            "SELECT DISTINCT l.IdPrimita FROM EF_PrimiteReceptii l JOIN FX_Receptii_R R ON R.IDRR = COALESCE(l.IDRR, (SELECT H.IDRR FROM FX_Receptii_H H WHERE H.IDRH = l.IDRH LIMIT 1)) "
            "WHERE l.IdPrimita IN (" + _marks(len(parte)) + ") AND COALESCE(R.Sters, 0) = 0", tuple(parte))
        out.update(int(r["IdPrimita"]) for r in cursor.fetchall())
    return out


# ---------------------------------------------------------------------------------------------
# writing: the OP and the allocations
# ---------------------------------------------------------------------------------------------
def creeaza_op(cursor, antet, user, numar=None):
    """Inserts the order and returns (IdOP, NrOP) with the REAL number, read under lock (or the `numar` the caller already took under lock in the same transaction,
    because the number is written on the payment document first). antet: IdOper, IdSubunitate, DataOP, Valoare, Explicatie, Documente, IdPartener, DenPartener,
    CFPartener, ContBeneficiar, bBIC, DenUnitate, CFUnitate, ContUnitate, pBIC, CodAngajament, CodIndicator, CodProgram."""
    if numar is None:
        numar = numar_op_urmator(cursor, antet["IdSubunitate"], blocheaza=True)
    cursor.execute(
        "INSERT INTO TZ_OPuri (IdOper, IdSubunitate, NrOP, DataOP, Valoare, Explicatie, Documente, IdPartener, DenPartener, CFPartener, ContBeneficiar, bBIC, "
        "DenUnitate, CFUnitate, ContUnitate, pBIC, CodAngajament, CodIndicator, CodProgram, Auto, DataIN, UN) "
        "VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, 1, NOW(), %s)",
        (antet["IdOper"], antet["IdSubunitate"], numar, antet["DataOP"], antet["Valoare"], antet["Explicatie"], antet.get("Documente"), antet.get("IdPartener"),
         antet.get("DenPartener"), antet.get("CFPartener"), antet.get("ContBeneficiar"), antet.get("bBIC"), antet.get("DenUnitate"), antet.get("CFUnitate"),
         antet.get("ContUnitate"), antet.get("pBIC"), antet.get("CodAngajament"), antet.get("CodIndicator"), antet.get("CodProgram"), user))
    return cursor.lastrowid, numar


def aloca_pe_evidentieri(cursor, id_plata, id_op, facturi, data_plata, total_plata, user):
    """One allocation row per (invoice, evidence operation) the payment covers. facturi: [{IdPrimita, NrFact, DataFact, Total, Platit}]. The evidence posted in the
    same transaction is already visible to this cursor. The operations are taken oldest first, each up to what is not allocated on it yet."""
    ids = [f["IdPrimita"] for f in facturi]
    ev = evidentieri(cursor, ids)
    cursor.execute("SELECT IdEvidentiere, SUM(Platit) AS S FROM TZ_PlatiFacturi WHERE IdEvidentiere IS NOT NULL GROUP BY IdEvidentiere")
    deja = {r["IdEvidentiere"]: cent(r["S"]) for r in cursor.fetchall()}
    for f in facturi:
        ramas_de_alocat = cent(f["Platit"])
        surse = ev.get(f["IdPrimita"]) or [{"IdOper": None, "Suma": ramas_de_alocat}]
        for e in surse:
            if ramas_de_alocat == 0:
                break
            disponibil = e["Suma"] - deja.get(e["IdOper"], Decimal(0)) if e["IdOper"] is not None else e["Suma"]
            parte = limita_comuna(ramas_de_alocat, disponibil)
            if parte == 0:
                continue
            cursor.execute(
                "INSERT INTO TZ_PlatiFacturi (IdPlata, IdOP, IdEvidentiere, IdPrimita, NumarDocument, DataDocument, ValoareFactura, Platit, Diferenta, ValoarePlata, "
                "DataPlata, UN) VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s)",
                (id_plata, id_op, e["IdOper"], f["IdPrimita"], f["NrFact"], f["DataFact"], cent(f["Total"]), parte, cent(f["Total"]) - parte, cent(total_plata),
                 data_plata, user))
            ramas_de_alocat -= parte
            if e["IdOper"] is not None:
                deja[e["IdOper"]] = deja.get(e["IdOper"], Decimal(0)) + parte
        if ramas_de_alocat:
            raise PlatiEroare("Restul de plată al facturii %s s-a modificat. Reîncărcați situația." % f["NrFact"], "PLATA_PESTE_REST")


def aloca_pe_operatii(cursor, id_plata, id_op, alocari, data_plata, total_plata, user):
    """One allocation row per evidence OPERATION the payment covers (invoices typed by hand, or any evidence already in the books; slice AV02-03).
    alocari: [{IdOper, NumarDocument, DataDocument, Valoare, Platit}]. Nothing is checked here: the caller has proved that every amount fits the rest."""
    for a in alocari:
        valoare, platit = cent(a["Valoare"]), cent(a["Platit"])
        cursor.execute(
            "INSERT INTO TZ_PlatiFacturi (IdPlata, IdOP, IdEvidentiere, IdPrimita, NumarDocument, DataDocument, ValoareFactura, Platit, Diferenta, ValoarePlata, "
            "DataPlata, UN) VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s)",
            (id_plata, id_op, a["IdOper"], a.get("IdPrimita"), a["NumarDocument"], a["DataDocument"], valoare, platit,
             valoare - platit, cent(total_plata), data_plata, user))


def valori_pe_operatii(cursor, ids_oper, id_partener, legaturi=None):
    """{IdOper: (value, allocated directly to this evidence, remaining eligible amount)}."""
    if not ids_oper:
        return {}
    marks = _marks(len(ids_oper))
    cursor.execute("SELECT IdOper, SUM(Suma) AS S FROM TZ_Credit WHERE IdPartener = %s AND IdOper IN (" + marks + ") GROUP BY IdOper", (id_partener,) + tuple(ids_oper))
    total = {r["IdOper"]: cent(r["S"]) for r in cursor.fetchall()}
    cursor.execute("SELECT IdEvidentiere, SUM(Platit) AS S FROM TZ_PlatiFacturi WHERE IdEvidentiere IN (" + marks + ") GROUP BY IdEvidentiere", tuple(ids_oper))
    deja = {r["IdEvidentiere"]: cent(r["S"]) for r in cursor.fetchall()}
    if legaturi is None:
        legaturi = primite_pe_evidentieri(cursor, ids_oper)
    disponibil = rest_de_plata(cursor, sorted({legatura[0] for legatura in legaturi.values() if legatura[0] is not None}))
    valori = {}
    for i in ids_oper:
        valoare = total.get(i, Decimal(0))
        platit = deja.get(i, Decimal(0))
        rest = valoare - platit
        if i in legaturi:
            id_primita, valoare_legata = legaturi[i]
            rest = Decimal(0) if id_primita is None else limita_comuna(limita_comuna(rest, valoare_legata - platit), disponibil[id_primita])
        valori[i] = (valoare, platit, rest)
    return valori


def id_partener_linie(mod, id_fix, id_operatie):
    """The partner on one side of a template line: the partner of the operation, the fixed one, or none."""
    if mod == "OPERATIE":
        return id_operatie
    return id_fix if mod == "FIX" else None


def posteaza_ordin(cursor, c):
    """Posts a PAYMENT and its ORDER (document «OP» whose number is the number of the order, the payment operation, the TZ_OPuri row) in the caller's transaction.
    c: sid, an_nota, id_nota, data_op, id_p (the payment template), sablon (IdJurnal, Explicatie, Compartiment), linie (the template line), clsf (IdClsf, ClsfF, ClsfE),
    id_partener, total, header (the extra columns of TZ_OPuri: Explicatie, Documente, DenPartener, CFPartener, ContBeneficiar, DenUnitate, CFUnitate, pBIC, CodAngajament,
    CodIndicator, CodProgram). Returns (IdOper, IdOP, NrOP, IdDocument). The number is read under lock HERE: it is the real one."""
    sid, linie, sablon, clsf = c["sid"], c["linie"], c["sablon"], c["clsf"]
    numar = numar_op_urmator(cursor, sid, blocheaza=True)
    id_doc = NoteC.creeaza_document(cursor, c["id_nota"], str(numar), c["data_op"], "OP", "P", True, c["user"])
    total = c["total"]
    antet, debit, credit = NoteC.operatie_din_sablon(cursor, sid, dict(sablon, IdSablon=c["id_p"]), linie, clsf, "P", c["data_op"], c["id_partener"], total)
    id_oper = NoteC.creeaza_operatie(cursor, sid, c["an_nota"], id_doc, antet, debit, credit, c["user"])
    id_op, numar = creeaza_op(cursor, dict(c["header"], IdOper=id_oper, IdSubunitate=sid, DataOP=c["data_op"], Valoare=total, IdPartener=c["id_partener"]),
                              c["user"], numar=numar)
    return id_oper, id_op, numar, id_doc


# ---------------------------------------------------------------------------------------------
# deleting a payment (with its OP)
# ---------------------------------------------------------------------------------------------
def sterge_plata(cursor, id_op):
    """Deletes an UNLOCKED order together with its allocations, its payment operation (debit, credit, operation) and, when it stays empty, its document and note.
    Returns the number of the deleted order. Refuses (PlatiEroare) a locked order."""
    motiv = motiv_blocare(cursor, id_op)
    if motiv:
        raise PlatiEroare(motiv + " Plata și ordinul nu se mai pot șterge.", "OP_BLOCAT")
    cursor.execute("SELECT IdOper, NrOP FROM TZ_OPuri WHERE IdOP = %s FOR UPDATE", (id_op,))
    op = cursor.fetchone()
    cursor.execute("SELECT o.IdDocument, o.IdOrdTbl, d.IdNota, d.DataDocument, n.IdSubunitate, n.An FROM TZ_Oper o JOIN TZ_Documente d ON d.IdDocument = o.IdDocument "
                   "JOIN TZ_NoteC n ON n.IdNota = d.IdNota WHERE o.IdOper = %s FOR UPDATE", (op["IdOper"],))
    doc = cursor.fetchone()
    # SLICE-AV08-04: the delete rules of «Modificare operatii» (Access STO_Click / EInchis) hold for «Sterge plata» of every window too
    if doc is not None:
        if doc["IdOrdTbl"] is not None:
            raise PlatiEroare("Plata are o ordonanțare generată. Ștergeți întâi ordonanțarea.", "PLATA_ORDONANTATA")
        if NoteC.perioada_inchisa(doc["DataDocument"], NoteC.data_ultimei_inchideri(cursor, doc["IdSubunitate"], doc["An"])):
            raise PlatiEroare("Plata face parte dintr-o perioadă închisă și nu se mai poate șterge.", "PLATA_INCHISA")
    cursor.execute("DELETE FROM TZ_PlatiFacturi WHERE IdOP = %s OR IdPlata = %s", (id_op, op["IdOper"]))
    cursor.execute("DELETE FROM TZ_OPuri WHERE IdOP = %s", (id_op,))
    NoteC.sterge_operatie(cursor, op["IdOper"])   # SLICE-AV08-04: the one delete of an operation
    if doc is not None:
        NoteC.curata_goale(cursor, doc["IdDocument"], doc["IdNota"])
    return int(op["NrOP"])

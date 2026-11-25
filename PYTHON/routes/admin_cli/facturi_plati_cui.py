"""KBotAdmin command `facturi_plati_cui` (SLICE-ME06-01): the invoices and payments of ONE CodFiscal, per source/sector (SS), read only.

The operator picks a partner in KBotAdmin (the CodFiscal goes here, with or without «RO»). In the chosen database every Parteneri row with that normalized
CodFiscal is found (one per unit: the same supplier is a partner of several units), and every accounting operation (TZ_Oper) that has a debit or credit
line on one of those partners is read, with its note (year, subunit, SS). Nothing is written.

The CodFiscal of the UNIT itself never links anything (operator rule, 19.11.2026): the special partners (burse, funds, ...) carry the code of the unit in
the cell instead of one of their own, so a match on it would tie a supplier to the state of burse or of salaries. Partners of Parteneri.Tip 2 (internal: not a
firm by CUI, not a person by CNP) are left out too. The unit's code is read from AVACONT_COMUN.Unitati_Date (else Unitati.CF).

Blocks:
  1. the partners found (unit, SS of the unit, code, name)
  2. summary per subunit and year: invoices (value, paid, rest, how many still open), payments, other operations
  3. invoices: evidence operations (Tip E) with a credit line of the partner; value = the credit lines, paid = what TZ_PlatiFacturi allocated
     on the operation, rest = value - paid. «in lista» says whether «Facturi si plati» shows it (it lists the ones with a rest)
  4. payments: payment operations (Tip P) with a debit line of the partner, with the order number when the payment has one
  5. other operations of the partner (any other Tip), so a partner whose invoices carry another Tip shows up instead of staying invisible

The same rule as the window (routes/avacont/introducere_date/operatii/facturi_plati.py): an invoice = Tip E + credit line of the partner;
the rest comes only from TZ_PlatiFacturi.IdEvidentiere (payments of the previous year of Access that point at IdEvidentiereAnterior are not counted).
"""
import re
from collections import OrderedDict
from decimal import Decimal

from routes.admin_cli.admin_common import AdminError, _rows
from routes.schema_sync.schema_common import COMMON_DB

ROW_LIMIT = 2000
INTERNAL_TIP = "2"      # Parteneri.Tip 2 = an internal partner (burse, fonduri...), not a firm (CUI) and not a person (CNP)
_RX_CUI = re.compile(r"^[A-Za-z0-9 .-]{2,20}$")
ZERO = Decimal("0.00")

REQUIRED = {
    "Parteneri": "IdPartener IdUnitate CodPartener DenumirePartener CodFiscal Tip Ascuns",
    "Unitati": "IdUnitate SursaSector An",
    "TZ_Subunitati": "IdSubunitate Denumire SursaSector",
    "TZ_NoteC": "IdNota IdSubunitate An NumarNC",
    "TZ_Documente": "IdDocument IdNota NumarDocument DataDocument FelDocument",
    "TZ_Oper": "IdOper IdDocument Tip DataOperatie Explicatie",
    "TZ_Credit": "IdOper IdPartener Suma",
    "TZ_Debit": "IdOper IdPartener Suma",
    "TZ_PlatiFacturi": "IdEvidentiere Platit",
    "TZ_OPuri": "IdOper NrOP",
}


def _normalize(value):
    """Letters and digits only, upper case, a leading «RO» before a digit dropped (the same rule as routes/forexe/partner_links.normalize_cui)."""
    cleaned = "".join(ch for ch in str(value or "") if ch.isalnum()).upper()
    if len(cleaned) > 2 and cleaned.startswith("RO") and cleaned[2].isdigit():
        cleaned = cleaned[2:]
    return cleaned


def _fiscal_sql(expression):
    """The SQL form of _normalize for a column."""
    clean = "UPPER(REGEXP_REPLACE(COALESCE(" + expression + ", ''), '[^A-Za-z0-9]', ''))"
    return "(CASE WHEN " + clean + " REGEXP '^RO[0-9]' THEN SUBSTRING(" + clean + ", 3) ELSE " + clean + " END)"


def _unit_cui(conn, db):
    """The normalized CodFiscal of the unit whose database `db` is: the one the unit filled in (AVACONT_COMUN.Unitati_Date), else Unitati.CF; '' when neither is set.
    The special partners (burse, funds...) carry THIS code instead of one of their own, so it never identifies a supplier."""
    for sql in ("SELECT CodFiscal AS cf FROM " + COMMON_DB + ".Unitati_Date WHERE DC = %s", "SELECT CF AS cf FROM " + COMMON_DB + ".Unitati WHERE DC = %s"):
        found = _rows(conn, sql, (db,))
        cui = _normalize(found[0]["cf"]) if found else ""
        if cui:
            return cui
    return ""


def _check_schema(conn, db):
    columns = {}
    for row in _rows(conn, "SELECT TABLE_NAME AS t, COLUMN_NAME AS c FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=%s", (db,)):
        columns.setdefault(row["t"], set()).add(row["c"])
    missing = [t + "." + c for t, fields in REQUIRED.items() for c in fields.split() if c not in columns.get(t, set())]
    if missing:
        raise AdminError("Baza «%s» nu are datele contabile cerute (lipsesc: %s). Comanda merge pe o baza de unitate cu tabelele TZ_."
                         % (db, ", ".join(missing[:8]) + (" ..." if len(missing) > 8 else "")))


def _cut(rows, what):
    """The rows up to ROW_LIMIT and the note that says when the list was cut."""
    if len(rows) > ROW_LIMIT:
        return rows[:ROW_LIMIT], "Lista de %s este taiata la %d randuri din %d." % (what, ROW_LIMIT, len(rows))
    return rows, ""


def _money(value):
    return Decimal(str(value if value is not None else 0)).quantize(ZERO)


def partners(conn, db, block):
    """The choice list of KBotAdmin: every visible partner of the database that has a CodFiscal, ONE row per normalized CodFiscal
    (the same supplier is a partner of several units): codfiscal, denumire, parteneri (how many rows), SS (the sources of the units it sits in)."""
    _check_schema(conn, db)
    unit_cui = _unit_cui(conn, db)
    rows = _rows(conn,
                 "SELECT t.cui, MIN(t.den) AS den, COUNT(*) AS n, GROUP_CONCAT(DISTINCT t.ss ORDER BY t.ss SEPARATOR ', ') AS ss FROM ("
                 "SELECT " + _fiscal_sql("p.CodFiscal") + " AS cui, p.DenumirePartener AS den, u.SursaSector AS ss FROM Parteneri p "
                 "LEFT JOIN Unitati u ON u.IdUnitate = p.IdUnitate WHERE COALESCE(p.Ascuns, 0) = 0 AND COALESCE(TRIM(p.Tip), '') <> %s) t "
                 "WHERE t.cui <> '' AND t.cui <> %s GROUP BY t.cui ORDER BY den, t.cui", (INTERNAL_TIP, unit_cui))
    return block(db, "Parteneri cu CodFiscal", ["codfiscal", "denumire", "parteneri", "SS"],
                 [[r["cui"], r["den"] or "", r["n"], r["ss"] or ""] for r in rows],
                 "%d furnizori. Lista pentru caseta de alegere din KBotAdmin. Lasati deoparte: partenerii fara CodFiscal, cei de Tip %s (interni) si cei care au "
                 "CodFiscal-ul unitatii (%s): acestia nu se leaga dupa CodFiscal." % (len(rows), INTERNAL_TIP, unit_cui or "nesetat"))


def check(conn, db, param, block):
    """The blocks for the CodFiscal in `param`; `block` is admin_cli._block."""
    raw = (param or "").strip()
    if not _RX_CUI.match(raw):
        raise AdminError("CodFiscal nu este valid (litere si cifre, 2 pana la 20 de caractere; «RO» in fata se ignora).")
    cui = _normalize(raw)
    if not cui:
        raise AdminError("CodFiscal nu este valid (litere si cifre, 2 pana la 20 de caractere; «RO» in fata se ignora).")
    _check_schema(conn, db)
    if cui == _unit_cui(conn, db):
        raise AdminError("CodFiscal %s este al unitatii: partenerii speciali (burse, fonduri...) il folosesc in locul unuia propriu, deci nu se cauta dupa el. "
                         "Alegeti furnizorul din lista." % cui)

    partners = _rows(conn,
                     "SELECT p.IdPartener, p.IdUnitate, u.SursaSector, u.An, p.CodPartener, p.DenumirePartener, p.CodFiscal, COALESCE(p.Ascuns, 0) AS Ascuns "
                     "FROM Parteneri p LEFT JOIN Unitati u ON u.IdUnitate = p.IdUnitate WHERE " + _fiscal_sql("p.CodFiscal") + " = %s "
                     "AND COALESCE(TRIM(p.Tip), '') <> %s ORDER BY u.SursaSector, p.IdUnitate, p.IdPartener", (cui, INTERNAL_TIP))
    blocks = [block(db, "Parteneri cu CodFiscal " + cui, ["IdPartener", "IdUnitate", "SS", "an unitate", "cod", "denumire", "CodFiscal", "ascuns"],
                    [[p["IdPartener"], p["IdUnitate"], p["SursaSector"] or "", p["An"] or "", p["CodPartener"], p["DenumirePartener"] or "", p["CodFiscal"] or "",
                      "da" if p["Ascuns"] else ""] for p in partners],
                    "Doar citire. Un furnizor este partener separat in fiecare unitate; toate randurile de mai jos se cauta pe toti acesti parteneri.")]
    if not partners:
        blocks[0]["note"] = "Niciun partener cu acest CodFiscal in baza %s. Nu s-a cautat nimic mai departe." % db
        return blocks

    ids = tuple(p["IdPartener"] for p in partners)
    marks = ", ".join(["%s"] * len(ids))
    operations = _rows(conn,
                       "SELECT n.IdSubunitate, s.Denumire AS Subunitate, s.SursaSector, n.An, n.NumarNC, o.IdOper, o.Tip, d.NumarDocument, d.DataDocument, d.FelDocument, "
                       "o.DataOperatie, o.Explicatie, COALESCE(cr.S, 0) AS Credit, COALESCE(db.S, 0) AS Debit, COALESCE(al.S, 0) AS Alocat, op.NrOP "
                       "FROM TZ_Oper o JOIN TZ_Documente d ON d.IdDocument = o.IdDocument JOIN TZ_NoteC n ON n.IdNota = d.IdNota "
                       "JOIN TZ_Subunitati s ON s.IdSubunitate = n.IdSubunitate "
                       "LEFT JOIN (SELECT IdOper, SUM(Suma) AS S FROM TZ_Credit WHERE IdPartener IN (" + marks + ") GROUP BY IdOper) cr ON cr.IdOper = o.IdOper "
                       "LEFT JOIN (SELECT IdOper, SUM(Suma) AS S FROM TZ_Debit WHERE IdPartener IN (" + marks + ") GROUP BY IdOper) db ON db.IdOper = o.IdOper "
                       "LEFT JOIN (SELECT IdEvidentiere, SUM(Platit) AS S FROM TZ_PlatiFacturi WHERE IdEvidentiere IS NOT NULL GROUP BY IdEvidentiere) al ON al.IdEvidentiere = o.IdOper "
                       "LEFT JOIN TZ_OPuri op ON op.IdOper = o.IdOper "
                       "WHERE cr.IdOper IS NOT NULL OR db.IdOper IS NOT NULL "
                       "ORDER BY s.SursaSector, n.IdSubunitate, n.An, d.DataDocument, o.IdOper", ids + ids)

    invoices, payments, others = [], [], []
    summary = OrderedDict()
    for o in operations:
        key = (o["IdSubunitate"], o["An"])
        s = summary.setdefault(key, {"sub": o["Subunitate"], "ss": o["SursaSector"], "inv": 0, "inv_value": ZERO, "inv_paid": ZERO, "inv_rest": ZERO, "inv_open": 0,
                                     "pay": 0, "pay_value": ZERO, "other": 0})
        tip = (o["Tip"] or "").strip()
        credit, debit, allocated = _money(o["Credit"]), _money(o["Debit"]), _money(o["Alocat"])
        place = [o["SursaSector"], o["Subunitate"], o["An"], o["NumarNC"]]
        if tip == "E" and credit != 0:
            rest = credit - allocated
            s["inv"] += 1
            s["inv_value"] += credit
            s["inv_paid"] += allocated
            s["inv_rest"] += rest
            s["inv_open"] += 1 if rest != 0 else 0
            invoices.append(place + [o["IdOper"], o["NumarDocument"], o["DataDocument"], o["FelDocument"], credit, allocated, rest, "da" if rest != 0 else "nu (achitata)"])
        elif tip == "P" and debit != 0:
            s["pay"] += 1
            s["pay_value"] += debit
            payments.append(place + [o["IdOper"], o["NrOP"] if o["NrOP"] is not None else "", o["NumarDocument"], o["DataDocument"], o["DataOperatie"], debit, o["Explicatie"]])
        else:
            s["other"] += 1
            others.append(place + [o["IdOper"], tip, o["NumarDocument"], o["DataDocument"], o["FelDocument"], debit, credit, o["Explicatie"]])

    rows = [[s["ss"], s["sub"], key[1], s["inv"], s["inv_value"], s["inv_paid"], s["inv_rest"], s["inv_open"], s["pay"], s["pay_value"], s["other"]]
            for key, s in summary.items()]
    blocks.append(block(db, "Sumar pe SS si an", ["SS", "subunitate", "an", "facturi", "valoare facturi", "platit", "rest", "facturi cu rest", "plati", "valoare plati", "alte operatii"],
                        rows, "Facturi = operatii Tip E cu linie de credit pe partener. «facturi cu rest» = ce arata fereastra «Facturi si plati» pentru acest furnizor. "
                              "Fara randuri: partenerul nu apare in nicio operatie contabila a bazei."))
    cut_rows, cut_note = _cut(invoices, "facturi")
    blocks.append(block(db, "Facturi (evidente Tip E)", ["SS", "subunitate", "an", "nota", "IdOper", "numar", "data", "fel", "valoare", "platit", "rest", "in lista"],
                        cut_rows, cut_note or "«in lista» = apare in «Facturi si plati» (are rest). Platit = alocarile din TZ_PlatiFacturi pe aceasta evidenta."))
    cut_rows, cut_note = _cut(payments, "plati")
    blocks.append(block(db, "Plati (operatii Tip P)", ["SS", "subunitate", "an", "nota", "IdOper", "nr OP", "document", "data doc", "data oper", "valoare", "explicatie"],
                        cut_rows, cut_note or "Valoare = liniile de debit ale partenerului pe operatia de plata. Nr OP gol = plata fara ordin (de ex. migrata)."))
    cut_rows, cut_note = _cut(others, "alte operatii")
    blocks.append(block(db, "Alte operatii ale partenerului", ["SS", "subunitate", "an", "nota", "IdOper", "tip", "document", "data doc", "fel", "debit", "credit", "explicatie"],
                        cut_rows, cut_note or "Operatii cu alt Tip decat E (credit) sau P (debit): ele nu intra in «Facturi si plati». O factura care apare aici, cu alt Tip, explica de ce lipseste din lista."))
    return blocks
